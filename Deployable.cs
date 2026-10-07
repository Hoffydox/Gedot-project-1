using Godot;

public partial class Deployable : StaticBody2D, IDamageable
{

	[Export] int maxHealth = 50;
	[Export] PackedScene pickupScene;
	int health;
	public int Health => health;
	public override void _Ready()
	{
		health = maxHealth;
		OnPlaced();
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
}
