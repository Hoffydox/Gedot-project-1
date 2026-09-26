using Godot;

public partial class Enemy : CharacterBody2D
{
	[Export] int maxHealth = 20;
	int health;
	[Export] float moveSpeed = 150f;

	//store player node
	PlayerCharacter targetNode;
	Area2D hitbox;
	//Area2D hitbox = GetNode<Area2D>("Hitbox");



	public override void _Ready()
	{
		health = maxHealth;
		// Get reference of player
		targetNode = GetTree().GetFirstNodeInGroup("player") as PlayerCharacter;
		hitbox = GetNode<Area2D>("Hitbox");

		// hitbox.BodyEntered += HitboxTriggered;


	}

	public override void _PhysicsProcess(double delta)
	{

		if (!IsInstanceValid(targetNode))
		{
			return;

		}
		// Get direction
		var thisPosition = GlobalPosition;
		var targetPosition = targetNode.GlobalPosition;
		// change arrow to more correct and discriptive name.
		var arrow = targetPosition - thisPosition;

		// Set Velocity to direction × speed.
		Velocity = arrow.Normalized() * moveSpeed;

		MoveAndSlide();

		var overlapCollection = hitbox.GetOverlappingBodies();
		foreach (var body in overlapCollection)
		{
			// runs once for each item; 'body' is the current one
			if (body is PlayerCharacter player)
			{
				player.Damage(2);
			}
		}
	}

	/*
		public void HitboxTriggered(Node2D body)
		{
			if (body is PlayerCharacter player)
			{
				player.Damage(2);
			}
		}
	*/
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
