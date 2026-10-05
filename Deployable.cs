using Godot;

public partial class Deployable : StaticBody2D, IDamageable
{

	[Export] int maxHealth = 50;
	int health;
	public int Health => health;
	public override void _Ready()
	{
		health = maxHealth;
		OnPlaced();
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
