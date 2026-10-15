using Godot;

public partial class Wall : Deployable
{
	// Enemies don't go looking for walls; they only attack one that blocks them.
	public override bool IsTargetable => false;
}
