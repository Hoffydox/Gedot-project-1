using Godot;

// A patch of ore on the ground. The drill can only be placed on one of these.
public partial class Deposit : Node2D
{
	[Export] int tier = 1;

	public int Tier => tier;

	public override void _Ready()
	{
		AddToGroup("deposits");
	}
}
