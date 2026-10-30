using Godot;
using System.Collections.Generic;

public partial class PlayerCharacter : CharacterBody2D, IDamageable
{
	[Signal] public delegate void DiedEventHandler();
	[Signal] public delegate void HealthChangedEventHandler(int current, int max);
	[Signal] public delegate void LootChangedEventHandler(int ore, int scrap, float load);
	[Signal] public delegate void CreditsChangedEventHandler(int credits);
	[Signal] public delegate void AmmoChangedEventHandler(AmmoType type, int ammo, int max);

	[Signal] public delegate void InventoryChangedEventHandler(Deployable.Kind kind, int count, int max);

	enum TurretAction { None, PickUp, Refill, Repair, Upgrade, Move } // I added some ideas here could be used in future versions

	// --- Settings ---
	[Export] int maxHealth = 20;
	[Export] float moveSpeed = 100f;
	[Export] float oreWeight = 1f, scrapWeight = 0.5f;
	[Export] float maxLoad = 40f;
	[Export] float heavySpeedFactor = 0.7f;
	[Export] float invincibleDuration = 0.5f;
	[Export] int startingCredits = 30;

	[Export] PackedScene bulletScene;
	[Export] float fireCooldown = 0.25f;
	[Export] AmmoType gunAmmoType = AmmoType.Small;
	[Export] int startingSmallAmmo = 70, maxSmallAmmo = 120;

	[Export] PackedScene drillScene;
	[Export] PackedScene turretScene;
	[Export] PackedScene wallScene;
	[Export] PackedScene crateScene;
	[Export] int maxTurrets = 3, startingTurrets = 1;
	[Export] int maxWalls = 5, startingWalls = 2;
	[Export] int maxCrates = 2, startingCrates = 1;
	[Export] float pickupRange = 50f;
	[Export] float drillRange = 40f;
	[Export] float pickUpTime = 1f;
	[Export] float refillTime = 1.5f;
	[Export] float repairTime = 1f;
	[Export] int repairPerScrap = 5;

	// --- State ---
	int health;
	float invincibleTimeLeft;
	int credits;
	Dictionary<AmmoType, int> ammo = new();
	Dictionary<AmmoType, int> maxAmmo = new();

	float fireTimeLeft;
	int carriedOre;
	int carriedScrap;
	bool inSafeZone;
	Drill placedDrill;
	Dictionary<Deployable.Kind, int> inventory = new();
	Dictionary<Deployable.Kind, int> maxInventory = new();
	AnimatedSprite2D body;
	Stack<TurretState> storedTurrets = new();
	Turret focusedTurret;
	TurretAction currentAction;
	Turret actionTurret;
	float actionTime;

	// --- Read-only access for others ---
	public int Health => health;
	public int MaxHealth => maxHealth;
	public int Credits => credits;
	public int GetAmmo(AmmoType type) => ammo[type];
	public int GetMaxAmmo(AmmoType type) => maxAmmo[type];

	public int CarriedOre => carriedOre;
	public int CarriedScrap => carriedScrap;

	// How loaded down you are: 0 = empty-handed, 1 = fully loaded (or more).
	public float Load => Mathf.Min((carriedOre * oreWeight + carriedScrap * scrapWeight) / maxLoad, 1f);

	public bool InSafeZone => inSafeZone;
	public int GetCount(Deployable.Kind kind) => inventory[kind];
	public int GetMax(Deployable.Kind kind) => maxInventory[kind];

	public override void _Ready()
	{
		health = maxHealth;
		credits = startingCredits;
		ammo[AmmoType.Small] = startingSmallAmmo;
		maxAmmo[AmmoType.Small] = maxSmallAmmo;

		inventory[Deployable.Kind.Turret] = startingTurrets;
		maxInventory[Deployable.Kind.Turret] = maxTurrets;
		inventory[Deployable.Kind.Wall] = startingWalls;
		maxInventory[Deployable.Kind.Wall] = maxWalls;
		inventory[Deployable.Kind.AmmoCrate] = startingCrates;
		maxInventory[Deployable.Kind.AmmoCrate] = maxCrates;

		// spawn protection
		invincibleTimeLeft = invincibleDuration;

		body = GetNode<AnimatedSprite2D>("Body");
	}

	// _PhysicsProcess runs ca. 60 times per second
	public override void _PhysicsProcess(double delta)
	{
		var moveDirection = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		Velocity = moveDirection * moveSpeed * Mathf.Lerp(1f, heavySpeedFactor, Load);

		// Move the body using Velocity. If we hit something (like a wall), slide
		// along it instead of stopping dead. Velocity is already "per second",
		// so no need to multiply by delta - MoveAndSlide handles that itself.
		MoveAndSlide();

		invincibleTimeLeft -= (float)delta;
		fireTimeLeft -= (float)delta;

		if (Input.IsActionJustPressed("deploy"))
		{
			PlaceDrill();
		}
		if (Input.IsActionJustPressed("deploy_turret"))
		{
			PlaceDeployable(Deployable.Kind.Turret);
		}
		if (Input.IsActionJustPressed("deploy_wall"))
		{
			PlaceDeployable(Deployable.Kind.Wall);
		}
		if (Input.IsActionJustPressed("deploy_crate"))
		{
			PlaceDeployable(Deployable.Kind.AmmoCrate);
		}
		if (Input.IsActionPressed("shoot"))
		{
			TryShoot();
		}
		UpdateFocusedTurret();
		UpdateTurretAction((float)delta);
		UpdateAnimation();
	}




	// --- Animation ---

	public void UpdateAnimation()
	{
		Vector2 aim = GetGlobalMousePosition() - GlobalPosition;

		string direction;
		bool flip = false;

		if (Mathf.Abs(aim.X) > Mathf.Abs(aim.Y))
		{
			direction = "side";
			flip = aim.X < 0;
		}
		else if (aim.Y < 0)
		{
			direction = "up";
		}
		else
		{
			direction = "down";
		}

		string pose = Input.IsActionPressed("shoot") ? "aim" : "idle";

		body.FlipH = flip;
		body.Play($"{pose}_{direction}");
	}

	// --- Health ---

	public void Damage(int damageAmount)
	{
		if (inSafeZone)
		{
			return;
		}
		if (invincibleTimeLeft > 0)
		{
			return;
		}

		health -= damageAmount;
		invincibleTimeLeft = invincibleDuration;
		GD.Print("Emotional Damage! New Hp = " + health);
		EmitSignalHealthChanged(health, maxHealth);

		if (health <= 0)
		{
			PlayerDeath();
		}
	}

	public void Heal(int healAmount)
	{
		if (health == maxHealth)
		{
			return;
		}

		health = Mathf.Min(health + healAmount, maxHealth);
		EmitSignalHealthChanged(health, maxHealth);
	}

	public void PlayerDeath()
	{
		// Announce first, so listeners can react before the player is freed.
		EmitSignalDied();
		QueueFree();
	}

	// --- Deploying ---

	public void PlaceDrill()
	{
		if (IsInstanceValid(placedDrill))
		{
			return;
		}

		Deposit deposit = FindClosestDeposit();
		if (deposit == null)
		{
			GD.Print("No deposit here");
			return;
		}

		var drill = drillScene.Instantiate<Drill>();
		drill.Setup(deposit);
		GetParent().AddChild(drill);
		drill.GlobalPosition = deposit.GlobalPosition;
		drill.ResetPhysicsInterpolation();
		placedDrill = drill;
	}

	void PlaceDeployable(Deployable.Kind kind)
	{
		if (!TryUseItem(kind))
		{
			GD.Print("No " + kind + " in your pack");
			return;
		}

		var deployable = GetSceneFor(kind).Instantiate<Deployable>();
		var spawnPoint = GlobalPosition + Vector2.Up * 50;

		// Add it to the world (our parent), not to the player, so it stays put.
		GetParent().AddChild(deployable);
		deployable.GlobalPosition = spawnPoint;
		deployable.ResetPhysicsInterpolation();

		// A picked-up turret comes back with the health and ammo it had.
		if (deployable is Turret turret && storedTurrets.Count > 0)
		{
			turret.LoadState(storedTurrets.Pop());
		}
	}

	PackedScene GetSceneFor(Deployable.Kind kind)
	{
		switch (kind)
		{
			case Deployable.Kind.Turret: return turretScene;
			case Deployable.Kind.Wall: return wallScene;
			case Deployable.Kind.AmmoCrate: return crateScene;
		}
		return null;
	}

	void TryPickUpTurret(Turret turret)
	{
		if (!TryAddItem(Deployable.Kind.Turret))
		{
			GD.Print("No room for another turret in your pack");
			return;
		}

		storedTurrets.Push(turret.SaveState());
		turret.QueueFree();
	}

	Turret FindClosestTurret()
	{
		Turret closest = null;
		float closestDistance = pickupRange;

		foreach (var node in GetTree().GetNodesInGroup("deployables"))
		{
			if (node is Turret turret)
			{
				float distance = GlobalPosition.DistanceTo(turret.GlobalPosition);
				if (distance < closestDistance)
				{
					closest = turret;
					closestDistance = distance;
				}
			}
		}
		return closest;
	}

	Deposit FindClosestDeposit()
	{
		Deposit closest = null;
		float closestDistance = drillRange;

		foreach (var node in GetTree().GetNodesInGroup("deposits"))
		{
			if (node is Deposit deposit)
			{
				float distance = GlobalPosition.DistanceTo(deposit.GlobalPosition);
				if (distance < closestDistance)
				{
					closest = deposit;
					closestDistance = distance;
				}
			}
		}
		return closest;
	}

	// Shows the prompt on the closest turret in range and hides it on the one before.
	void UpdateFocusedTurret()
	{
		Turret closest = FindClosestTurret();
		if (closest == focusedTurret)
		{
			return;
		}

		if (IsInstanceValid(focusedTurret))
		{
			focusedTurret.ShowPrompt(false);
		}
		if (closest != null)
		{
			closest.ShowPrompt(true);
		}
		focusedTurret = closest;
	}

	// Hold a key near a turret to act on it. Letting go, walking off or switching turret starts over.
	void UpdateTurretAction(float delta)
	{
		TurretAction held = GetHeldTurretAction();

		if (held != currentAction || focusedTurret != actionTurret)
		{
			StopTurretAction();
			currentAction = held;
			actionTurret = focusedTurret;
		}

		if (currentAction == TurretAction.None || actionTurret == null)
		{
			return;
		}
		if (!CanDoTurretAction(currentAction, actionTurret))
		{
			StopTurretAction();
			return;
		}

		actionTime += delta;
		float duration = GetActionTime(currentAction);
		actionTurret.ShowProgress(GetActionText(currentAction, actionTurret), actionTime / duration);

		if (actionTime >= duration)
		{
			FinishTurretAction(currentAction, actionTurret);
			StopTurretAction();
		}
	}

	TurretAction GetHeldTurretAction()
	{
		if (Input.IsActionPressed("interact"))
		{
			return TurretAction.PickUp;
		}
		if (Input.IsActionPressed("refill"))
		{
			return TurretAction.Refill;
		}
		if (Input.IsActionPressed("repair"))
		{
			return TurretAction.Repair;
		}
		return TurretAction.None;
	}

	void StopTurretAction()
	{
		if (IsInstanceValid(actionTurret))
		{
			actionTurret.HideProgress();
		}
		actionTime = 0;
	}

	// Is this action possible right now? Checked before the bar starts, and every frame while it fills.
	bool CanDoTurretAction(TurretAction action, Turret turret)
	{
		switch (action)
		{
			case TurretAction.PickUp: return GetCount(Deployable.Kind.Turret) < GetMax(Deployable.Kind.Turret);
			case TurretAction.Refill: return turret.NeedsAmmo && ammo[turret.AmmoType] > 0;
			case TurretAction.Repair: return turret.NeedsRepair && carriedScrap > 0;
		}
		return false;
	}

	float GetActionTime(TurretAction action)
	{
		switch (action)
		{
			case TurretAction.PickUp: return pickUpTime;
			case TurretAction.Refill: return refillTime;
			case TurretAction.Repair: return repairTime;
		}
		return 1f;
	}

	string GetActionText(TurretAction action, Turret turret)
	{
		switch (action)
		{
			case TurretAction.PickUp: return "Picking up...";
			case TurretAction.Refill: return "Refilling...";
			case TurretAction.Repair: return "Repairing " + turret.Health + "/" + turret.MaxHealth;
		}
		return "";
	}

	void FinishTurretAction(TurretAction action, Turret turret)
	{
		switch (action)
		{
			case TurretAction.PickUp:
				TryPickUpTurret(turret);
				break;
			case TurretAction.Refill:
				RefillTurret(turret);
				break;
			case TurretAction.Repair:
				RepairTurret(turret);
				break;
		}
	}

	void RefillTurret(Turret turret)
	{
		AmmoType type = turret.AmmoType;
		int used = turret.Refill(ammo[type]);
		ammo[type] -= used;
		EmitSignalAmmoChanged(type, ammo[type], maxAmmo[type]);
	}

	void RepairTurret(Turret turret)
	{
		carriedScrap--;
		EmitSignalLootChanged(carriedOre, carriedScrap, Load);
		turret.Repair(repairPerScrap);
	}

	// --- Inventory ---

	public bool TryAddItem(Deployable.Kind kind)
	{
		if (inventory[kind] >= maxInventory[kind])
		{
			return false;
		}
		inventory[kind]++;
		EmitSignalInventoryChanged(kind, inventory[kind], maxInventory[kind]);
		return true;
	}

	public bool TryUseItem(Deployable.Kind kind)
	{
		if (inventory[kind] <= 0)
		{
			return false;
		}
		inventory[kind]--;
		EmitSignalInventoryChanged(kind, inventory[kind], maxInventory[kind]);
		return true;
	}

	// --- Shooting ---

	void TryShoot()
	{
		if (fireTimeLeft > 0)
		{
			return;
		}
		// No shooting in the settlement: clicking shop buttons uses the same mouse button.
		if (inSafeZone)
		{
			return;
		}
		if (ammo[gunAmmoType] <= 0)
		{
			return;
		}

		Vector2 direction = (GetGlobalMousePosition() - GlobalPosition).Normalized();

		var bullet = bulletScene.Instantiate<Bullet>();
		GetParent().AddChild(bullet);
		bullet.GlobalPosition = GlobalPosition;
		bullet.Launch(direction, Bullet.Team.Player);
		bullet.ResetPhysicsInterpolation();

		ammo[gunAmmoType]--;
		EmitSignalAmmoChanged(gunAmmoType, ammo[gunAmmoType], maxAmmo[gunAmmoType]);

		fireTimeLeft = fireCooldown;
	}

	public bool TryAddAmmo(AmmoType type, int amount)
	{
		if (ammo[type] >= maxAmmo[type])
		{
			return false;
		}

		ammo[type] = Mathf.Min(ammo[type] + amount, maxAmmo[type]);
		EmitSignalAmmoChanged(type, ammo[type], maxAmmo[type]);
		return true;
	}

	// --- Loot and credits ---

	public void AddLoot(Pickup.LootType type, int amount)
	{
		if (type == Pickup.LootType.Ore)
		{
			carriedOre += amount;
		}
		else if (type == Pickup.LootType.Scrap)
		{
			carriedScrap += amount;
		}
		EmitSignalLootChanged(carriedOre, carriedScrap, Load);
	}

	public void ClearLoot()
	{
		carriedOre = 0;
		carriedScrap = 0;
		EmitSignalLootChanged(carriedOre, carriedScrap, Load);
	}

	public void AddCredits(int amount)
	{
		credits += amount;
		EmitSignalCreditsChanged(credits);
	}

	public bool TrySpendCredits(int amount)
	{
		if (credits < amount)
		{
			return false;
		}
		credits -= amount;
		EmitSignalCreditsChanged(credits);
		return true;
	}

	// --- Safe zone ---

	public void EnterSafeZone()
	{
		inSafeZone = true;
	}

	public void ExitSafeZone()
	{
		inSafeZone = false;
	}
}
