using Godot;

public partial class Drill : Deployable
{
	[Export] float drillDuration = 30.3f;
	[Export] float oreRate = 0.5f;

	float drillTimeLeft;
	float oreMined;
	WaveManager waveManager;

	public float DrillTimeLeft => drillTimeLeft;

	public override void OnPlaced()
	{
		base.OnPlaced();
		drillTimeLeft = drillDuration;
		waveManager = GetTree().GetFirstNodeInGroup("wave_manager") as WaveManager;
		if (!IsInstanceValid(waveManager))
		{
			return;
		}
		waveManager.StartSwarm(this);
	}

	public override void _Process(double delta)
	{
		oreMined += oreRate * (float)delta;
		drillTimeLeft -= (float)delta;
		if (drillTimeLeft <= 0)
		{
			FinishDrilling();
		}
	}

	void FinishDrilling()
	{
		GD.Print("Drilling Complete!");
		if (IsInstanceValid(waveManager))
		{
			waveManager.EndSwarm();
		}
		// Idea: an animation, and swap the mineral patch for an exhausted one.
		DropPickup(Pickup.LootType.Ore, (int)oreMined);
		QueueFree();
	}

	public override void OnDestroyed()
	{
		GD.Print("Drill destroyed!");
		if (IsInstanceValid(waveManager))
		{
			waveManager.EndSwarm();
		}
		// The ore is lost. Idea: exhausted patch here too? Can the player come back and mine later?
		base.OnDestroyed();
	}
}
