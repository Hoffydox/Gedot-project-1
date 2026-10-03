using Godot;

public partial class Enemy : CharacterBody2D, IDamageable
{
	[Export] int maxHealth = 20;
	int health;
	[Export] float moveSpeed = 150f;
	[Export] float attackCooldown = 1.0f;
	float attackTimeLeft;


	//store player node
	PlayerCharacter targetNode;
	Area2D hitbox;
	public override void _Ready()
	{
		health = maxHealth;
		// Get reference of player
		targetNode = GetTree().GetFirstNodeInGroup("player") as PlayerCharacter;
		hitbox = GetNode<Area2D>("Hitbox");
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
		var directionToTarget = targetPosition - thisPosition;

		// Set Velocity to direction × speed.
		Velocity = directionToTarget.Normalized() * moveSpeed;

		MoveAndSlide();

		attackTimeLeft = attackTimeLeft - (float)delta;
		if (attackTimeLeft > 0)
		{
			return;
		}

		var overlapCollection = hitbox.GetOverlappingBodies();
		foreach (var body in overlapCollection)
		{
			// runs once for each item; 'body' is the current one
			if (body is IDamageable target)
			{
				if (target is Enemy) // to prevent self-damage.
				{
					continue;
				}
				target.Damage(2);
				attackTimeLeft = attackCooldown;
				break; //only attacks 1 thing on the foreach "list". Break leaves the loop.
			}
		}


	}

	public void Damage(int amount)
	{
		health -= amount;
		if (health <= 0)
		{
			QueueFree();
		}
	}
}
