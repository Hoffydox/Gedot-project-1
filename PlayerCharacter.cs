using Godot;
using System.Collections.Generic;

public partial class PlayerCharacter : CharacterBody2D, IDamageable
{
	[Signal] public delegate void InventoryChangedEventHandler(Deployable.Kind kind, int count, int max);
	[Signal] public delegate void DiedEventHandler();
	[Signal] public delegate void HealthChangedEventHandler(int current, int max);
	[Signal] public delegate void LootChangedEventHandler(int ore, int scrap);
	[Signal] public delegate void CreditsChangedEventHandler(int credits);

	[Signal] public delegate void AmmoChangedEventHandler(int ammo, int max);



	[Export] int maxTurrets = 3, startingTurrets = 1;
	[Export] int maxWalls = 5, startingWalls = 2;

	[Export] int startingAmmo = 30;
	[Export] int maxAmmo = 90;

	Dictionary<Deployable.Kind, int> inventory = new();
	Dictionary<Deployable.Kind, int> maxInventory = new();

	int ammo;
	public int Ammo => ammo;
	public int MaxAmmo => maxAmmo;
	int credits;
	public int Credits => credits;
	bool inSafeZone;
	public bool InSafeZone => inSafeZone;

	int carriedOre;
	int carriedScrap;
	public int CarriedOre => carriedOre;
	public int CarriedScrap => carriedScrap;
	/* 	[Export] int maxTurrets = 3;
		public int MaxTurrets => maxTurrets;
	 */    /* [Export] int startingTurrets = 1; */
	[Export] int startingCredits = 30;
	[Export] PackedScene bulletScene;
	[Export] float fireCooldown = 0.25f;
	float fireTimeLeft;

	[Export] int maxHealth = 20;
	[Export] PackedScene turretScene;
	int health;
	public int Health => health;
	public int MaxHealth => maxHealth;
	[Export] float moveSpeed = 300f;

	[Export] float invincibleDuration = 0.5f;

	[Export] PackedScene drillScene;

	Drill placedDrill;
	Turret placedTurret;

	float invincibleTimeLeft;

	public override void _Ready()
	{
		inventory[Deployable.Kind.Turret] = startingTurrets;
		maxInventory[Deployable.Kind.Turret] = maxTurrets;
		inventory[Deployable.Kind.Wall] = startingWalls;
		maxInventory[Deployable.Kind.Wall] = maxWalls;

		health = maxHealth;
		credits = startingCredits;
		ammo = startingAmmo;

		//spawn protection
		invincibleTimeLeft = invincibleDuration;
	}

	public int GetCount(Deployable.Kind kind) => inventory[kind];
	public int GetMax(Deployable.Kind kind) => maxInventory[kind];

	
	// _PhysicsProcess calls ca. 60 times per second
	public override void _PhysicsProcess(double delta)
	{
		// Get direction
		var moveDirection = Input.GetVector("move_left", "move_right", "move_up", "move_down");

		// Set Velocity to direction × speed.
		Velocity = moveDirection * moveSpeed;

		// Move the body using Velocity. If we hit something (like a wall), slide
		// along it instead of stopping dead. Velocity is already "per second",
		// so no need to multiply by delta - MoveAndSlide handles that itself.
		MoveAndSlide();
		invincibleTimeLeft = invincibleTimeLeft - (float)delta;

		if (Input.IsActionJustPressed("deploy"))
		{
			PlaceDrill();
		}
		if (Input.IsActionJustPressed("deploy_turret"))
		{
			PlaceTurret();
		}
		fireTimeLeft -= (float)delta;

		if (Input.IsActionPressed("shoot"))
		{
			TryShoot();
		}
	}

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

		health = health - damageAmount;
		invincibleTimeLeft = invincibleDuration;
		GD.Print(what: "Emotional Damage! New Hp = " + health);
		EmitHealthChanged();
		if (health <= 0)
		{
			//dies.
			PlayerDeath();
		}

	}

	public void PlayerDeath()
	{
		EmitSignal(SignalName.Died);
		//Death method. Can trigger other things before QueueFree();
		QueueFree();
	}

	public void EmitHealthChanged()
	{
		EmitSignal(SignalName.HealthChanged, health, maxHealth);
	}
	public void EmitLootChanged()
	{
		EmitSignal(SignalName.LootChanged, carriedOre, carriedScrap);
	}


	public void Heal(int healAmount)
	{
		if (health == maxHealth)
		{
			return;
		}
		else if (health + healAmount >= maxHealth)
		{
			health = maxHealth;
			GD.Print("Overhealed.");
			EmitHealthChanged();
		}
		else
		{
			health = health + healAmount;
			EmitHealthChanged();
		}
	}

	public void PlaceDrill()
	{
		if (IsInstanceValid(placedDrill))
		{
			return;
		}
		var drill = drillScene.Instantiate<Drill>();

		Vector2 spawnDirection = Vector2.Down;

		var offset = spawnDirection * 150;
		var spawnPoint = GlobalPosition + offset;

		GetParent().AddChild(drill);

		drill.GlobalPosition = spawnPoint;

		placedDrill = drill;


	}
	public void PlaceTurret()
	{

		if (!TryUseItem(Deployable.Kind.Turret))
		{
			GD.Print(what: GetCount(Deployable.Kind.Turret) + " turrets in inventory!");
			return;
		}

		var turret = turretScene.Instantiate<Turret>();

		Vector2 spawnDirection = Vector2.Up;

		var offset = spawnDirection * 150;
		var spawnPoint = GlobalPosition + offset;

		GetParent().AddChild(turret);

		turret.GlobalPosition = spawnPoint;

		placedTurret = turret;

	}
	void TryShoot()
	{
		if (fireTimeLeft > 0)
		{
			return;
		}

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
		EmitLootChanged();
	}

	public void EnterSafeZone()
	{
		inSafeZone = true;
	}
	public void ExitSafeZone()
	{
		inSafeZone = false;
	}
	public void AddCredits(int amount)
	{
		credits += amount;
		EmitSignal(SignalName.CreditsChanged, credits);
	}
	public void ClearLoot()
	{
		carriedOre = 0;
		carriedScrap = 0;
		EmitLootChanged();
	}
	public bool TrySpendCredits(int amount)
	{

		if (credits < amount)
		{
			return false;
		}

		credits = credits - amount;

		EmitSignalCreditsChanged(credits);
		return true;
	}
	public bool TryAddItem(Deployable.Kind kind)
	{
		if (inventory[kind] >= maxInventory[kind])
		{
			return (false);
		}
		inventory[kind]++;
		EmitSignalInventoryChanged(kind, inventory[kind], maxInventory[kind]);
		return (true);
	}

	public bool TryUseItem(Deployable.Kind kind)
	{
		if (inventory[kind] <= 0)
		{
			return (false);
		}
		inventory[kind]--;
		EmitSignalInventoryChanged(kind,inventory[kind], maxInventory[kind]);
		return (true);
	}

}
