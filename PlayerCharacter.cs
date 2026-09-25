using Godot;

public partial class PlayerCharacter : CharacterBody2D
{

	[Export] int maxHealth = 20;
	int health;
	[Export] float moveSpeed = 300f;

    public override void _Ready()
    {
		health = maxHealth;
		GD.Print(what:"in _Ready method");
      		
		health = Damage(2);
		health = Heal(3);
		GD.Print(health);
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
	
	}

	public int Damage(int damageAmt)
	{
		GD.Print(what:"in Damage method");

		

		return health - damageAmt;

	}

	public int Heal(int healAmt)
	{
		GD.Print(what:"in Heal method");

		if (health + healAmt >= maxHealth)
		{
			return maxHealth;

		}

		return health + healAmt;
		
	}


}