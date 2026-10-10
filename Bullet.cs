using Godot;

public partial class Bullet : Area2D
{
	public enum Team { Player, Turret, Drill, Hazard }
	Team team;

	[Export] float speed = 900;
	[Export] int damage = 20;
	[Export] float lifetime = 2;

	[Export] float knockbackForce = 400f;
	[Export] float stunDuration = 0.15f;

	Vector2 direction;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	public override void _Process(double delta)
	{
		GlobalPosition += direction * speed * (float)delta;
		lifetime = lifetime - (float)delta;
		if (lifetime < 0)
		{
			QueueFree();
		}
	}

	public void Launch(Vector2 newDirection, Team newTeam)
	{
		direction = newDirection;
		Rotation = direction.Angle();               //(point the sprite the way it flies)
		team = newTeam;
		if (team == Team.Player)
		{
			SetCollisionMaskValue(3, true);
		}
	}

	public void OnBodyEntered(Node2D body)
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
