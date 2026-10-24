using Godot;

public partial class Turret : Deployable
{
	[Export] PackedScene bulletScene;
	[Export] float fireCooldown = 0.5f;
	[Export] int scrapWorth = 2;
	// Lifetime is planned to be replaced by repair + ammo (side quest step 23).
	[Export] bool expires = true;
	[Export] float lifetime = 60;

	float fireTimeLeft;
	float lifeTimeLeft;
	Area2D range;
	Sprite2D head;
	Label timeLeftLabel;

	public override void OnPlaced()
	{
		base.OnPlaced();
		range = GetNode<Area2D>("Range");
		head = GetNode<Sprite2D>("Head");
		timeLeftLabel = GetNode<Label>("TimeLeftLabel");
		lifeTimeLeft = lifetime;
	}

	public override void _PhysicsProcess(double delta)
	{
		// Before any early return, so the turret ages even with no enemies around.
		if (expires)
		{
			lifeTimeLeft -= (float)delta;
			if (lifeTimeLeft <= 0)
			{
				OnDestroyed();
				return;
			}
			timeLeftLabel.Text = Mathf.Ceil(lifeTimeLeft) + "s";
		}

		fireTimeLeft -= (float)delta;

		Enemy closest = FindClosestEnemy();
		if (closest == null)
		{
			return;
		}

		head.LookAt(closest.GlobalPosition);
		if (fireTimeLeft > 0)
		{
			return;
		}
		Shoot(closest);
		fireTimeLeft = fireCooldown;
	}

	public override void OnDestroyed()
	{
		DropPickup(Pickup.LootType.Scrap, scrapWorth);
		base.OnDestroyed();
	}

	Enemy FindClosestEnemy()
	{
		Enemy closest = null;
		float closestDistance = float.MaxValue;
		foreach (var body in range.GetOverlappingBodies())
		{
			if (body is Enemy enemy)
			{
				float distance = GlobalPosition.DistanceTo(enemy.GlobalPosition);
				if (distance < closestDistance)
				{
					closest = enemy;
					closestDistance = distance;
				}
			}
		}
		return closest;
	}

	void Shoot(Enemy enemy)
	{
		var fireDirection = (enemy.GlobalPosition - GlobalPosition).Normalized();
		var bullet = bulletScene.Instantiate<Bullet>();
		GetParent().AddChild(bullet);
		bullet.GlobalPosition = GlobalPosition;
		bullet.Launch(fireDirection, Bullet.Team.Turret);
		bullet.ResetPhysicsInterpolation();
	}
}
