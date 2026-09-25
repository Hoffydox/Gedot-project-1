using Godot;

public partial class Enemy : CharacterBody2D
{
	[Export] int maxHealth = 20;
	int health;
	[Export] float moveSpeed = 150f;

	//store player node
	PlayerCharacter targetNode;

	public override void _Ready()
	{
		health = maxHealth;
		// Get reference of player
		targetNode = GetTree().GetFirstNodeInGroup("player") as PlayerCharacter;
		
	}

	public override void _PhysicsProcess(double delta)
	{
		// Get direction
		// target position − my position = arrow from me to target
		
		var thisPosition = GlobalPosition;
		var targetPosition = targetNode.GlobalPosition;
		// change arrow to more correct and discriptive name.
		var arrow = targetPosition - thisPosition;
		
		// Set Velocity to direction × speed.
		Velocity = arrow.Normalized() * moveSpeed;

		MoveAndSlide();
	}

	public int Damage(int damageAmt)
	{
		return health - damageAmt;
	}

	public int Heal(int healAmt)
	{
		if (health + healAmt >= maxHealth)
		{
			return maxHealth;
		}

		return health + healAmt;
	}
}
