using Godot;

public partial class WaveManager : Node
{
	[Signal] public delegate void SwarmStartedEventHandler(int swarmNumber);
	[Signal] public delegate void SwarmEndedEventHandler();

	public enum WaveState { Calm, Swarm } // idea: a boss phase later

	[Export] EnemySpawner spawner;

	WaveState currentState = WaveState.Calm;
	int swarmNumber = 0;
	Drill currentDrill;

	public WaveState State => currentState;
	public int SwarmNumber => swarmNumber;
	public Drill CurrentDrill => currentDrill;

	public override void _Ready()
	{
		ChangeState(WaveState.Calm);
	}

	public void StartSwarm(Drill drill)
	{
		// Only one swarm at a time.
		if (currentState == WaveState.Swarm)
		{
			return;
		}
		currentDrill = drill;
		swarmNumber++;
		ChangeState(WaveState.Swarm);
		EmitSignalSwarmStarted(swarmNumber);
	}

	public void EndSwarm()
	{
		// Nothing to end if no swarm is running.
		if (currentState != WaveState.Swarm)
		{
			return;
		}
		ChangeState(WaveState.Calm);
		EmitSignalSwarmEnded();
	}

	void ChangeState(WaveState newState)
	{
		currentState = newState;
		switch (currentState)
		{
			case WaveState.Calm:
				spawner.StopSpawning();
				GD.Print("Stopped spawning.");
				break;
			case WaveState.Swarm:
				spawner.StartSpawning(currentDrill.Tier);
				GD.Print("Swarm " + swarmNumber + " started!");
				break;
		}
	}
}
