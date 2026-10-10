using Godot;

public partial class Enemy : CharacterBody2D, IDamageable
{
	public enum EnemyState { Wander, Chase, Attack }
	EnemyState currentState = EnemyState.Wander;
	bool alwaysChase = false;

	[Export] float detectionRange = 450;
	[Export] float loseInterestRange = 1000;
	[Export] float wanderSpeed = 50;
	[Export] float wanderRadius = 200;

	[Export] int attackDamage = 4;
	[Export] float attackWindup = 0.3f;
	float windupTimeLeft;

	[Export] float knockbackFriction = 1500f;
	[Export] float separationRadius = 90f;
	[Export] float separationStrength = 200f;
	Area2D separationArea;
	Vector2 knockbackVelocity;
	float stunTimeLeft;

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
		separationArea = GetNode<Area2D>("Separation");
		homePosition = GlobalPosition;
		PickWanderTarget();
	}

	public Vector2 GetSeparation()
	{
		Vector2 push = Vector2.Zero;
		
		foreach (var body in separationArea.GetOverlappingBodies())
		{
			if (body is Enemy other && other != this)
			{
				Vector2 away = GlobalPosition - other.GlobalPosition;
				float distance = away.Length();
				if (distance > 0)
				{
					push += away / distance * (1 - distance / separationRadius);
				}
			}
		}
		return push * separationStrength;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!IsInstanceValid(targetNode))
		{
			return;
		}
		attackTimeLeft -= (float)delta;

		if (stunTimeLeft > 0)
		{
			stunTimeLeft -= (float)delta;
			Velocity = knockbackVelocity;
			knockbackVelocity = knockbackVelocity.MoveToward(Vector2.Zero, knockbackFriction * (float)delta);
			MoveAndSlide();
			return;
		}

		switch (currentState)
		{
			case EnemyState.Wander: Wander(); break;
			case EnemyState.Chase: Chase(); break;
			case EnemyState.Attack: Attack((float)delta); break;
		}
		
		if (currentState != EnemyState.Attack)
		{
			Velocity += GetSeparation();
		}

		MoveAndSlide();

	}

	public void Damage(int amount)
	{
		health -= amount;
		if (health <= 0)
		{

			EnemyDeath();

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

		if (IsBlockedByWall())
		{
			PickWanderTarget();
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
		bool targetInReach = hitbox.OverlapsBody(target);

		if (IsBlockedByWall() && attackTimeLeft <= 0)
		{
			windupTimeLeft = attackWindup;
			currentState = EnemyState.Attack;
			Velocity = Vector2.Zero;
			return;
		}

		if (targetInReach && attackTimeLeft <= 0)
		{
			windupTimeLeft = attackWindup;
			currentState = EnemyState.Attack;
			Velocity = Vector2.Zero;
			return;
		}
		else if (targetInReach)
		{
			// in reach but still on cooldown: hold position instead of pushing into the target
			Velocity = Vector2.Zero;
			return;
		}

		var thisPosition = GlobalPosition;
		var targetPosition = target.GlobalPosition;
		var directionToTarget = targetPosition - thisPosition;
		Velocity = directionToTarget.Normalized() * moveSpeed;
	}

	public void Attack(float delta)
	{
		Velocity = Vector2.Zero;
		windupTimeLeft -= (float)delta;
		if (windupTimeLeft > 0)
		{
			return;
		}
		Strike();
		currentState = EnemyState.Chase;
	}

	public void Strike()
	{

		var overlapCollection = hitbox.GetOverlappingBodies();
		foreach (var body in overlapCollection)
		{
			if (body is IDamageable target)
			{
				if (target is Enemy) // to prevent self-damage and hurting other enemies.
				{
					continue;
				}
				target.Damage(attackDamage);
				// or here.
				break; //only attacks 1 thing on the foreach "list". Break leaves the loop.
			}
		}
				attackTimeLeft = attackCooldown; // either here or line 4 lines above.
				// please explain why here.
	}

	public void ApplyKnockback(Vector2 direction, float force, float stunDuration)
	{
		knockbackVelocity = direction * force;

		stunTimeLeft = stunDuration;

		if (currentState == EnemyState.Attack)
		{
			currentState = EnemyState.Chase;
		}  


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
				if (!deployable.IsTargetable)
				{
					continue;
				}
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
	public bool IsBlockedByWall()
	{
		var blockedCollection = hitbox.GetOverlappingBodies();
		foreach (var body in blockedCollection)
		{
			if (body is Deployable d && !d.IsTargetable)
			{
				return true;
			}
			
		}
		return false;
	}
}
