using Godot;

// A box of ammo you place near your defences. It feeds nearby turrets and the player,
// and is gone when empty.
public partial class AmmoCrate : Deployable
{
	[Export] AmmoType ammoType = AmmoType.Small;
	[Export] int capacity = 150;
	[Export] float feedRange = 60f;
	[Export] float feedInterval = 1f;
	[Export] int feedAmount = 2;
	[Export] int playerFeedAmount = 5;

	int ammo;
	float feedTimeLeft;
	Label ammoLabel;
	PlayerCharacter player;

	public override void OnPlaced()
	{
		base.OnPlaced();
		ammoLabel = GetNode<Label>("AmmoLabel");
		player = GetTree().GetFirstNodeInGroup("player") as PlayerCharacter;
		ammo = capacity;
		UpdateAmmoLabel();
	}

	public override void _PhysicsProcess(double delta)
	{
		feedTimeLeft -= (float)delta;
		if (feedTimeLeft > 0)
		{
			return;
		}
		feedTimeLeft = feedInterval;
		FeedTurrets();
		FeedPlayer();
	}

	// Gives a few rounds to every turret in reach that uses our ammo and isn't full.
	void FeedTurrets()
	{
		foreach (var node in GetTree().GetNodesInGroup("deployables"))
		{
			if (ammo <= 0)
			{
				return;
			}
			if (node is not Turret turret || turret.AmmoType != ammoType || !turret.NeedsAmmo)
			{
				continue;
			}
			if (GlobalPosition.DistanceTo(turret.GlobalPosition) > feedRange)
			{
				continue;
			}

			UseAmmo(turret.Refill(Mathf.Min(feedAmount, ammo)));
		}
	}

	// Tops up the player's own ammo too, while they stand within reach.
	void FeedPlayer()
	{
		if (ammo <= 0 || !IsInstanceValid(player))
		{
			return;
		}
		if (GlobalPosition.DistanceTo(player.GlobalPosition) > feedRange)
		{
			return;
		}

		int room = player.GetMaxAmmo(ammoType) - player.GetAmmo(ammoType);
		int given = Mathf.Min(Mathf.Min(playerFeedAmount, room), ammo);
		if (given <= 0)
		{
			return;
		}
		player.TryAddAmmo(ammoType, given);
		UseAmmo(given);
	}

	void UseAmmo(int amount)
	{
		ammo -= amount;
		UpdateAmmoLabel();
		if (ammo <= 0)
		{
			QueueFree(); // used up
		}
	}

	void UpdateAmmoLabel()
	{
		ammoLabel.Text = ammo.ToString();
	}
}
