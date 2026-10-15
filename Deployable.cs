using Godot;

public partial class Deployable : StaticBody2D, IDamageable
{
	public enum Kind { Turret, Wall }

	[Export] int maxHealth = 50;
	[Export] PackedScene pickupScene;

	int health;
	Color baseModulate;

	public int Health => health;
	// Whether enemies go looking for this deployable. Walls override this to false.
	public virtual bool IsTargetable => true;

	public override void _Ready()
	{
		AddToGroup("deployables");
		health = maxHealth;
		baseModulate = Modulate;
		OnPlaced();
	}

	// Children override these to add their own behaviour; call base.X() to keep the default.
	public virtual void OnPlaced()
	{
		GD.Print("Placed!");
	}

	public virtual void OnDestroyed()
	{
		QueueFree();
	}

	public void Damage(int amount)
	{
		health -= amount;
		GD.Print(Name + " HP: " + health);
		if (health <= 0)
		{
			OnDestroyed();
		}
	}

	// Red flash that fades back to this deployable's own tint (used for friendly fire).
	public void FlashHit()
	{
		Modulate = Colors.Red;
		var tween = CreateTween();
		tween.TweenProperty(this, "modulate", baseModulate, 0.25f);
	}

	protected void DropPickup(Pickup.LootType type, int amount)
	{
		if (amount <= 0)
		{
			return;
		}
		var pickup = pickupScene.Instantiate<Pickup>();
		pickup.Setup(type, amount);
		pickup.GlobalPosition = GlobalPosition;
		GetParent().CallDeferred(Node.MethodName.AddChild, pickup);
	}
}
