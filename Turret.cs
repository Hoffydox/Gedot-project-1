using Godot;

public partial class Turret : Deployable
{
	[Export] PackedScene bulletScene;
	[Export] float fireCooldown = 0.5f;
	[Export] int scrapWorth = 2;
	[Export] int maxAmmo = 30;
	[Export] AmmoType ammoType = AmmoType.Small;

	float fireTimeLeft;
	int ammo;
	Area2D range;
	Sprite2D head;
	Label ammoLabel;
	Label promptLabel;

	public override void OnPlaced()
	{
		base.OnPlaced();
		range = GetNode<Area2D>("Range");
		head = GetNode<Sprite2D>("Head");
		ammoLabel = GetNode<Label>("AmmoLabel");
		promptLabel = GetNode<Label>("PromptLabel");
		promptLabel.Visible = false;
		ammo = maxAmmo;
		UpdateAmmoLabel();
	}



	public override void _PhysicsProcess(double delta)
	{
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
		if (ammo <= 0)
		{
			return; // out of ammo: still tracks enemies, but can't fire
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

		ammo--;
		UpdateAmmoLabel();
	}

	void UpdateAmmoLabel()
	{
		ammoLabel.Text = ammo > 0 ? ammo.ToString() : "EMPTY";
	}

	// Writes down everything a turret should remember while it's packed away.
	public TurretState SaveState()
	{
		var state = new TurretState();
		state.Health = Health;
		state.Ammo = ammo;
		return state;
	}

	// Brings a placed turret back to how it was when it was picked up.
	public void LoadState(TurretState state)
	{
		RestoreHealth(state.Health);
		ammo = Mathf.Min(state.Ammo, maxAmmo);
		UpdateAmmoLabel();
	}

	// The player calls this when this turret becomes, or stops being, the one you'd interact with.
	public void ShowPrompt(bool show)
	{
		promptLabel.Visible = show;
	}
}
