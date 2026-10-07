using Godot;

public partial class Drill : Deployable
{
	[Export] float drillDuration = 30.3f;
	[Export] float oreRate = 0.5f;
	float oreMined;

	float drillTimeLeft;

	public float DrillTimeLeft => drillTimeLeft;

	WaveManager targetWaveManager;


	public override void OnPlaced()
	{
		base.OnPlaced();
		targetWaveManager = GetTree().GetFirstNodeInGroup("wave_manager") as WaveManager;
		drillTimeLeft = drillDuration;
		if (!IsInstanceValid(targetWaveManager))
		{
			return;
		}
		targetWaveManager.StartSwarm(this);
	}

	public override void _Process(double delta)
	{
		drillTimeLeft = drillTimeLeft - (float)delta;
		if (drillTimeLeft <= 0)
		{
			FinishDrilling();
		}
		oreMined += oreRate * (float)delta;
	}

	public void FinishDrilling()
	{
		GD.Print(what: "Drilling Complete!");
		if (IsInstanceValid(targetWaveManager))
		{
			targetWaveManager.EndSwarm();
		}
		/* ADD an animation here in the future. maybe switch mineral patch with exhausted patch. */
		// drop an ore pickup 

		DropPickup(Pickup.LootType.Ore, (int)oreMined);
		
		QueueFree();

	}

	public override void OnDestroyed()
	{
		GD.Print(what: "Drill destroyed!");
		if (IsInstanceValid(targetWaveManager))
		{
			targetWaveManager.EndSwarm();
		}
		/* should switch mineral patch with exhausted patch. also happend here? can player get back and mine later? */
		oreMined = 0;
		base.OnDestroyed();

	}

}
