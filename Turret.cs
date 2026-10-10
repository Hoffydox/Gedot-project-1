using Godot;

public partial class Turret : Deployable
{
	[Export] PackedScene bulletScene;
	[Export] float fireCooldown = 0.5f;
	[Export] float lifetime = 60;
    [Export] bool expires = true;
    [Export] int scrapWorth = 2;
    float lifeTimeLeft;
	float fireTimeLeft;
	Area2D range;
	Sprite2D head;
	Label timeLeftLabel;


	public override void OnPlaced()
	{
		base.OnPlaced();
		range = GetNode<Area2D>("Range");
		head = GetNode<Sprite2D>("Head");
		lifeTimeLeft = lifetime;
		timeLeftLabel = GetNode<Label>("TimeLeftLabel");
	}

	public void UpdateTimeLeftLabel(float timeLeft)
	{
		timeLeftLabel.Text = Mathf.Ceil(timeLeft) + "s";
	}

	public override void _PhysicsProcess(double delta)
	{
		if (expires)
		{
			lifeTimeLeft = lifeTimeLeft - (float)delta;
			if (lifeTimeLeft <= 0)
			{
				OnDestroyed();
				return;	
			}
			UpdateTimeLeftLabel(lifeTimeLeft);
		}

		fireTimeLeft = fireTimeLeft - (float)delta;

		Enemy closest = null;
		float closestDistance = float.MaxValue;
		foreach (var body in range.GetOverlappingBodies())
		{
			if (body is Enemy enemy) // checks if enemy
			{
				float distanceToEnemy = GlobalPosition.DistanceTo(enemy.GlobalPosition);
				if (distanceToEnemy < closestDistance)
				{
					closest = enemy;
					closestDistance = distanceToEnemy;
				}
			}
		}
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

	public void Shoot(Enemy enemy)
	{
		var fireDirection = (enemy.GlobalPosition - this.GlobalPosition).Normalized();

		var spawnPoint = GlobalPosition;
		var bullet = bulletScene.Instantiate<Bullet>();

		GetParent().AddChild(bullet);
		bullet.GlobalPosition = spawnPoint;
		bullet.Launch(fireDirection, Bullet.Team.Turret);
	}
    public override void OnDestroyed()
  	{
		DropPickup(Pickup.LootType.Scrap, scrapWorth);
        base.OnDestroyed();
    }

}
