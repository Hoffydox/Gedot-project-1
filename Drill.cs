using Godot;

public partial class Drill : Deployable
{
	// One entry per tier: [0] = tier 1, [1] = tier 2, [2] = tier 3.
	[Export] float[] drillDurationPerTier = { 45f, 60f, 75f };
	[Export] float oreRate = 0.4f;

	float drillTimeLeft;
	float oreMined;
	Deposit deposit;
	WaveManager waveManager;

	public float DrillTimeLeft => drillTimeLeft;
	public int Tier => deposit.Tier;

	// Called by the player before the drill is added to the world.
	public void Setup(Deposit onDeposit)
	{
		deposit = onDeposit;
	}

	public override void OnPlaced()
	{
		base.OnPlaced();
		drillTimeLeft = drillDurationPerTier[Tier - 1];
		waveManager = GetTree().GetFirstNodeInGroup("wave_manager") as WaveManager;
		if (!IsInstanceValid(waveManager))
		{
			return;
		}
		waveManager.StartSwarm(this);
	}

	public override void _Process(double delta)
	{
		oreMined += oreRate * Tier * (float)delta;
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
