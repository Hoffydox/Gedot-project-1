using Godot;

public partial class Bullet : Area2D
{
	// Who fired the bullet. Decides what it can hit (see Launch).
	public enum Team { Player, Turret, Drill, Hazard }

	[Export] float speed = 900;
	[Export] int damage = 20;
	[Export] float lifetime = 2;
	[Export] float knockbackForce = 400f;
	[Export] float stunDuration = 0.15f;

	Team team;
	Vector2 direction;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	public override void _Process(double delta)
	{
		GlobalPosition += direction * speed * (float)delta;
		lifetime -= (float)delta;
		if (lifetime < 0)
		{
			QueueFree();
		}
	}

	// Called by the shooter right after creating the bullet.
	public void Launch(Vector2 newDirection, Team newTeam)
	{
		direction = newDirection;
		Rotation = direction.Angle(); // point the sprite the way it flies
		team = newTeam;

		// Player bullets can also hit deployables (friendly fire); turret bullets only see enemies.
		if (team == Team.Player)
		{
			SetCollisionMaskValue(3, true);
		}
	}

	void OnBodyEntered(Node2D body)
	{
		if (body is Enemy enemy)
		{
			enemy.ApplyKnockback(direction, knockbackForce, stunDuration);
			enemy.Damage(damage);
			QueueFree();
		}
		else if (body is IDamageable target)
		{
			if (body is Deployable deployable)
			{
				deployable.FlashHit();
			}
			target.Damage(damage);
			QueueFree();
		}
	}
}
