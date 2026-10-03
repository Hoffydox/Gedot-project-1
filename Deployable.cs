using Godot;

public partial class Deployable : StaticBody2D
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
}
