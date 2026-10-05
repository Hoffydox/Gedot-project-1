using Godot;

public partial class Drill : Deployable
{
	[Export] float drillDuration = 30;
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
	}

	public void FinishDrilling()
	{
		if (!IsInstanceValid(targetWaveManager))
		{
			return;
		} 
		GD.Print(what: "Drilling Complete!");
		targetWaveManager.EndSwarm();
		/* ADD an animation here in the future. maybe switch mineral patch with exhausted patch. */
		QueueFree();
		
	}

	public override void OnDestroyed()
	{
		if (!IsInstanceValid(targetWaveManager))
		{
			return;
		} 
		GD.Print(what: "Drill destroyed!");
		targetWaveManager.EndSwarm();
		/* should switch mineral patch with exhausted patch. also happend here? can player get back and mine later? */
		base.OnDestroyed();
	}

}
