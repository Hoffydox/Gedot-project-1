using Godot;

public partial class PlayerCharacter : CharacterBody2D
{
	[Export] int maxHealth = 20;
	int health;
	[Export] float moveSpeed = 300f;

	[Export] float invicibleDuration = 0.5f;
	float invincibleTimeLeft;

	public override void _Ready()
	{
		health = maxHealth;
	invincibleTimeLeft = invicibleDuration;
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
			GD.Print(what:"No damage - playerInvincible = " + invincibleTimeLeft);
			return;	
		} 
		
		health = health - damageAmount;
		invincibleTimeLeft = invicibleDuration;
		GD.Print(what:"Emotional Damage! New Hp = " + health);
	}

	public void Heal(int healAmount)
	{
		if (health + healAmount >= maxHealth)
		{
			health = maxHealth;
			GD.Print("Overhealed.");
		} else
		{
			health = health + healAmount;
		}

		
	}
}
