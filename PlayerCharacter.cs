using Godot;
using System.Collections.Generic;

public partial class PlayerCharacter : CharacterBody2D, IDamageable
{
	[Signal] public delegate void DiedEventHandler();
	[Signal] public delegate void HealthChangedEventHandler(int current, int max);
	[Signal] public delegate void LootChangedEventHandler(int ore, int scrap);
	[Signal] public delegate void CreditsChangedEventHandler(int credits);
	[Signal] public delegate void AmmoChangedEventHandler(int ammo, int max);
	[Signal] public delegate void InventoryChangedEventHandler(Deployable.Kind kind, int count, int max);

	// --- Settings ---
	[Export] int maxHealth = 20;
	[Export] float moveSpeed = 100f;
	[Export] float invincibleDuration = 0.5f;
	[Export] int startingCredits = 30;

	[Export] PackedScene bulletScene;
	[Export] float fireCooldown = 0.25f;
	[Export] int startingAmmo = 30;
	[Export] int maxAmmo = 90;

	[Export] PackedScene drillScene;
	[Export] PackedScene turretScene;
	[Export] PackedScene wallScene;
	[Export] int maxTurrets = 3, startingTurrets = 1;
	[Export] int maxWalls = 5, startingWalls = 2;

	// --- State ---
	int health;
	float invincibleTimeLeft;
	int credits;
	int ammo;
	float fireTimeLeft;
	int carriedOre;
	int carriedScrap;
	bool inSafeZone;
	Drill placedDrill;
	Dictionary<Deployable.Kind, int> inventory = new();
	Dictionary<Deployable.Kind, int> maxInventory = new();

	// --- Read-only access for others ---
	public int Health => health;
	public int MaxHealth => maxHealth;
	public int Credits => credits;
	public int Ammo => ammo;
	public int MaxAmmo => maxAmmo;
	public int CarriedOre => carriedOre;
	public int CarriedScrap => carriedScrap;
	public bool InSafeZone => inSafeZone;
	public int GetCount(Deployable.Kind kind) => inventory[kind];
	public int GetMax(Deployable.Kind kind) => maxInventory[kind];

	public override void _Ready()
	{
		health = maxHealth;
		credits = startingCredits;
		ammo = startingAmmo;

		inventory[Deployable.Kind.Turret] = startingTurrets;
		maxInventory[Deployable.Kind.Turret] = maxTurrets;
		inventory[Deployable.Kind.Wall] = startingWalls;
		maxInventory[Deployable.Kind.Wall] = maxWalls;

		// spawn protection
		invincibleTimeLeft = invincibleDuration;
	}

	// _PhysicsProcess runs ca. 60 times per second
	public override void _PhysicsProcess(double delta)
	{
		var moveDirection = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		Velocity = moveDirection * moveSpeed;

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
		if (Input.IsActionPressed("shoot"))
		{
			TryShoot();
		}
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

		var drill = drillScene.Instantiate<Drill>();
		var spawnPoint = GlobalPosition + Vector2.Down * 50;

		GetParent().AddChild(drill);
		drill.GlobalPosition = spawnPoint;
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
	}

	PackedScene GetSceneFor(Deployable.Kind kind)
	{
		switch (kind)
		{
			case Deployable.Kind.Turret: return turretScene;
			case Deployable.Kind.Wall: return wallScene;
		}
		return null;
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
		if (ammo <= 0)
		{
			return;
		}

		Vector2 direction = (GetGlobalMousePosition() - GlobalPosition).Normalized();

		var bullet = bulletScene.Instantiate<Bullet>();
		GetParent().AddChild(bullet);
		bullet.GlobalPosition = GlobalPosition;
		bullet.Launch(direction, Bullet.Team.Player);
		bullet.ResetPhysicsInterpolation();

		ammo--;
		EmitSignalAmmoChanged(ammo, maxAmmo);
		fireTimeLeft = fireCooldown;
	}

	public bool TryAddAmmo(int amount)
	{
		if (ammo >= maxAmmo)
		{
			return false;
		}

		ammo = Mathf.Min(ammo + amount, maxAmmo);
		EmitSignalAmmoChanged(ammo, maxAmmo);
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
		EmitSignalLootChanged(carriedOre, carriedScrap);
	}

	public void ClearLoot()
	{
		carriedOre = 0;
		carriedScrap = 0;
		EmitSignalLootChanged(carriedOre, carriedScrap);
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
