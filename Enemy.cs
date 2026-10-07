using Godot;

public partial class Enemy : CharacterBody2D, IDamageable
{
	public enum EnemyState { Wander, Chase }
	EnemyState currentState = EnemyState.Wander;
	bool alwaysChase = false;

	[Export] float detectionRange = 450;
	[Export] float loseInterestRange = 1000;
	[Export] float wanderSpeed = 50;
	[Export] float wanderRadius = 200;

	Vector2 homePosition;
	Vector2 wanderTarget;

	[Export] int maxHealth = 20;
	[Export] PackedScene pickupScene;
	int health;
	[Export] float moveSpeed = 350f;
	[Export] float attackCooldown = 1.0f;
	float attackTimeLeft;
	bool isDead = false;

	[Export] int minScrap = 0;
	[Export] int maxScrap = 1;


	//store player node
	PlayerCharacter targetNode;
	Area2D hitbox;
	public override void _Ready()
	{
		health = maxHealth;
		// Get reference of player
		targetNode = GetTree().GetFirstNodeInGroup("player") as PlayerCharacter;
		hitbox = GetNode<Area2D>("Hitbox");
		homePosition = GlobalPosition;
		PickWanderTarget();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!IsInstanceValid(targetNode))
		{
			return;
		}

		switch (currentState)
		{
			case EnemyState.Wander: Wander(); break;
			case EnemyState.Chase: Chase(); break;
		}

		MoveAndSlide();
		TryAttack(delta);
	}

	public void Damage(int amount)
	{
		health -= amount;
		if (health <= 0)
		{

			EnemyDeath();

		}
	}
	public void TryAttack(double delta)
	{
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
				if (target is Enemy) // to prevent self-damage and hurting other enemies.
				{
					continue;
				}
				target.Damage(2);
				attackTimeLeft = attackCooldown;
				break; //only attacks 1 thing on the foreach "list". Break leaves the loop.
			}
		}
	}

	public void EnemyDeath()
	{

		if (isDead)
		{
			return;
		}
		int scrapWorth = GD.RandRange(minScrap, maxScrap);
		if (scrapWorth > 0)
		{
			var spawnPoint = GlobalPosition;
			var pickup = pickupScene.Instantiate<Pickup>();
			pickup.GlobalPosition = spawnPoint;
			pickup.Setup(Pickup.LootType.Scrap, scrapWorth);
			GetParent().CallDeferred(Node.MethodName.AddChild, pickup);
		}
		isDead = true;
		QueueFree();
	}
	public void Wander()
	{
		var target = FindTarget(detectionRange);
		if (target != null)
		{
			currentState = EnemyState.Chase;
			return;
		}

		if (GlobalPosition.DistanceTo(wanderTarget) < 10)
		{
			PickWanderTarget();

		}
		var thisPosition = GlobalPosition;
		var targetPosition = wanderTarget;
		var directionToTarget = targetPosition - thisPosition;
		Velocity = directionToTarget.Normalized() * wanderSpeed;
	}
	public void Chase()
	{
		float range = alwaysChase ? float.MaxValue : loseInterestRange;
		var target = FindTarget(range);
		if (target == null)
		{
			currentState = EnemyState.Wander;
			return;
		}

		var thisPosition = GlobalPosition;
		var targetPosition = target.GlobalPosition;
		var directionToTarget = targetPosition - thisPosition;
		Velocity = directionToTarget.Normalized() * moveSpeed;
	}
	public void PickWanderTarget()
	{
		float wanderAngle = (float)GD.RandRange(0, Mathf.Tau);
		Vector2 wanderDirection = Vector2.Right.Rotated(wanderAngle);
		float wanderDistance = (float)GD.RandRange(50, wanderRadius);

		var offset = wanderDirection * wanderDistance;
		wanderTarget = homePosition + offset;

	}
	public void MakeSwarmEnemy()
	{
		alwaysChase = true;
		currentState = EnemyState.Chase;
	}

	public Node2D FindTarget(float range)
	{
		Node2D best = null;
		float bestDistance = range;

		if (IsInstanceValid(targetNode) && !targetNode.InSafeZone)
		{
			float d = GlobalPosition.DistanceTo(targetNode.GlobalPosition);
			if (d < bestDistance)
			{
				best = targetNode;
				bestDistance = d;

			}
		}
		foreach (var node in GetTree().GetNodesInGroup("deployables"))
		{
			if (node is Deployable deployable)
			{
				float d = GlobalPosition.DistanceTo(deployable.GlobalPosition);
				if (d < bestDistance)
				{
					best = deployable;
					bestDistance = d;
				}
			}
		}

		return best;
	}
}
