using Godot;

public partial class Enemy : CharacterBody2D, IDamageable
{
	public enum EnemyState { Wander, Chase, Attack }

	// --- Settings ---
	[Export] int maxHealth = 20;
	[Export] float moveSpeed = 117f;
	[Export] float wanderSpeed = 17;
	[Export] float wanderRadius = 67;
	[Export] float detectionRange = 150;
	[Export] float loseInterestRange = 333;

	[Export] int attackDamage = 4;
	[Export] float attackWindup = 0.3f;
	[Export] float attackCooldown = 1.0f;

	[Export] float knockbackFriction = 500f;
	[Export] float separationRadius = 30f;
	[Export] float separationStrength = 67f;

	[Export] PackedScene pickupScene;
	[Export] int minScrap = 0;
	[Export] int maxScrap = 1;

	// --- State ---
	EnemyState currentState = EnemyState.Wander;
	bool alwaysChase = false;
	int health;
	bool isDead = false;
	float attackTimeLeft;
	float windupTimeLeft;
	Vector2 knockbackVelocity;
	float stunTimeLeft;
	Vector2 homePosition;
	Vector2 wanderTarget;

	PlayerCharacter player;
	Area2D hitbox;
	Area2D separationArea;

	public override void _Ready()
	{
		health = maxHealth;
		player = GetTree().GetFirstNodeInGroup("player") as PlayerCharacter;
		hitbox = GetNode<Area2D>("Hitbox");
		separationArea = GetNode<Area2D>("Separation");
		homePosition = GlobalPosition;
		PickWanderTarget();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!IsInstanceValid(player))
		{
			return;
		}
		attackTimeLeft -= (float)delta;

		// Stunned: slide back from the hit and skip everything else.
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

	// --- Called from outside ---

	public void MakeSwarmEnemy()
	{
		alwaysChase = true;
		currentState = EnemyState.Chase;
	}

	public void Damage(int amount)
	{
		health -= amount;
		if (health <= 0)
		{
			EnemyDeath();
		}
	}

	public void ApplyKnockback(Vector2 direction, float force, float stunDuration)
	{
		knockbackVelocity = direction * force;
		stunTimeLeft = stunDuration;

		// A hit interrupts the wind-up.
		if (currentState == EnemyState.Attack)
		{
			currentState = EnemyState.Chase;
		}
	}

	// --- States ---

	void Wander()
	{
		var target = FindTarget(detectionRange);
		if (target != null)
		{
			currentState = EnemyState.Chase;
			return;
		}

		if (IsBlockedByWall() || GlobalPosition.DistanceTo(wanderTarget) < 3)
		{
			PickWanderTarget();
		}

		var directionToTarget = wanderTarget - GlobalPosition;
		Velocity = directionToTarget.Normalized() * wanderSpeed;
	}

	void Chase()
	{
		float range = alwaysChase ? float.MaxValue : loseInterestRange;
		var target = FindTarget(range);
		if (target == null)
		{
			currentState = EnemyState.Wander;
			return;
		}
		bool targetInReach = hitbox.OverlapsBody(target);

		if ((IsBlockedByWall() || targetInReach) && attackTimeLeft <= 0)
		{
			windupTimeLeft = attackWindup;
			currentState = EnemyState.Attack;
			Velocity = Vector2.Zero;
			return;
		}
		if (targetInReach)
		{
			// In reach but still on cooldown: hold position instead of pushing into the target.
			Velocity = Vector2.Zero;
			return;
		}

		var directionToTarget = target.GlobalPosition - GlobalPosition;
		Velocity = directionToTarget.Normalized() * moveSpeed;
	}

	void Attack(float delta)
	{
		Velocity = Vector2.Zero;
		windupTimeLeft -= delta;
		if (windupTimeLeft > 0)
		{
			return;
		}
		Strike();
		currentState = EnemyState.Chase;
	}

	void Strike()
	{
		foreach (var body in hitbox.GetOverlappingBodies())
		{
			if (body is IDamageable target)
			{
				// Enemies don't hurt themselves or each other.
				if (target is Enemy)
				{
					continue;
				}
				target.Damage(attackDamage);
				break; // only hit one thing per swing
			}
		}
		// Cooldown after every swing, hit or miss, so dodging gives an opening.
		attackTimeLeft = attackCooldown;
	}

	// --- Helpers ---

	// The closest valid target within range: the player (unless in a safe zone)
	// or any targetable deployable. Returns null if nothing is in range.
	Node2D FindTarget(float range)
	{
		Node2D best = null;
		float bestDistance = range;

		if (IsInstanceValid(player) && !player.InSafeZone)
		{
			float d = GlobalPosition.DistanceTo(player.GlobalPosition);
			if (d < bestDistance)
			{
				best = player;
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

	bool IsBlockedByWall()
	{
		foreach (var body in hitbox.GetOverlappingBodies())
		{
			if (body is Deployable deployable && !deployable.IsTargetable)
			{
				return true;
			}
		}
		return false;
	}

	// A push away from nearby enemies, stronger the closer they are, so packs spread out.
	Vector2 GetSeparation()
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

	void PickWanderTarget()
	{
		float wanderAngle = (float)GD.RandRange(0, Mathf.Tau);
		Vector2 wanderDirection = Vector2.Right.Rotated(wanderAngle);
		float wanderDistance = (float)GD.RandRange(17, wanderRadius);
		wanderTarget = homePosition + wanderDirection * wanderDistance;
	}

	void EnemyDeath()
	{
		if (isDead)
		{
			return;
		}
		isDead = true;

		int scrapWorth = GD.RandRange(minScrap, maxScrap);
		if (scrapWorth > 0)
		{
			var pickup = pickupScene.Instantiate<Pickup>();
			pickup.GlobalPosition = GlobalPosition;
			pickup.Setup(Pickup.LootType.Scrap, scrapWorth);
			// Deferred: deaths can happen during a physics callback (bullet hit).
			GetParent().CallDeferred(Node.MethodName.AddChild, pickup);
		}
		QueueFree();
	}
}
