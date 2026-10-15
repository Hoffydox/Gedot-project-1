using Godot;

public partial class Deployable : StaticBody2D, IDamageable
{

	public enum Kind { Turret, Wall}

	[Export] int maxHealth = 50;
	[Export] PackedScene pickupScene;

	Color baseModulate;
	int health;
	public int Health => health;
	public virtual bool IsTargetable => true;
	public override void _Ready()
	{
		AddToGroup("deployables");
		health = maxHealth;
		OnPlaced();
		baseModulate = Modulate;
	}
	protected void DropPickup(Pickup.LootType type, int amount)
	{
		if (amount <= 0)
		{
			return;
		}
		var spawnPoint = GlobalPosition;

		var pickup = pickupScene.Instantiate<Pickup>();

		pickup.Setup(type, amount);

		pickup.GlobalPosition = spawnPoint;
		GetParent().CallDeferred(Node.MethodName.AddChild, pickup);
	}

	public virtual void OnPlaced()
	{

		GD.Print(what: "Placed!");

	}
	public void Damage(int amount)
	{

		health -= amount;
		GD.Print(what: "my HP: " + health);
		if (health <= 0)
		{
			OnDestroyed();
		}
	}
	public virtual void OnDestroyed()
	{
		QueueFree();
	}

	public void FlashHit()
	{
		Modulate = Colors.Red;
		var tween = CreateTween();
		tween.TweenProperty(this, "modulate", baseModulate, 0.25f);


	}
}
