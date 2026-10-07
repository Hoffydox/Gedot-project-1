using Godot;

public partial class PlayerCharacter : CharacterBody2D, IDamageable
{
	[Signal] public delegate void DiedEventHandler();
	[Signal] public delegate void HealthChangedEventHandler(int current, int max);
	[Signal] public delegate void LootChangedEventHandler(int ore, int scrap);
	[Signal] public delegate void CreditsChangedEventHandler(int credits);
	int credits;
	public int Credits => credits;
	bool inSafeZone;
	public bool InSafeZone => inSafeZone;

	int carriedOre;
	int carriedScrap;
	public int CarriedOre => carriedOre;
	public int CarriedScrap => carriedScrap;
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
		health = maxHealth;

		//spawn protection
		invincibleTimeLeft = invincibleDuration;
	}
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

		var turret = turretScene.Instantiate<Turret>();

		Vector2 spawnDirection = Vector2.Up;

		var offset = spawnDirection * 150;
		var spawnPoint = GlobalPosition + offset;

		GetParent().AddChild(turret);

		turret.GlobalPosition = spawnPoint;

		placedTurret = turret;

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
		// did i use the right params and fields?
		credits += amount;
		EmitSignal(SignalName.CreditsChanged, credits);
	}
	public void ClearLoot()
	{
		carriedOre = 0;
		carriedScrap = 0;
		EmitLootChanged();
	}
}
