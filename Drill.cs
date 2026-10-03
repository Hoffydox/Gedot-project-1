using Godot;

public partial class Drill : Deployable
{
	WaveManager targetWaveManager;

	public override void OnPlaced()
	{
		base.OnPlaced();
		targetWaveManager = GetTree().GetFirstNodeInGroup("wave_manager") as WaveManager;
		if (!IsInstanceValid(targetWaveManager))
		{
			return;
		}
		targetWaveManager.StartWave();
	}

}
