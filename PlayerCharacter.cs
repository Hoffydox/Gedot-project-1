using Godot;

public partial class PlayerCharacter : CharacterBody2D
{
	[Signal] public delegate void DiedEventHandler();
	[Export] int maxHealth = 20;
	int health;
	[Export] float moveSpeed = 300f;

	[Export] float invincibleDuration = 0.5f;
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

	}

	public void Damage(int damageAmount)
	{
		if (invincibleTimeLeft > 0)
		{
			return;
		}

		health = health - damageAmount;
		invincibleTimeLeft = invincibleDuration;
		GD.Print(what: "Emotional Damage! New Hp = " + health);
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

	public void Heal(int healAmount)
	{
		if (health + healAmount >= maxHealth)
		{
			health = maxHealth;
			GD.Print("Overhealed.");
		}
		else
		{
			health = health + healAmount;
		}


	}
}
