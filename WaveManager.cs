using Godot;


public partial class WaveManager : Node
{

	[Signal] public delegate void SwarmStartedEventHandler(int swarmNumber);
	[Signal] public delegate void SwarmEndedEventHandler();

	public enum WaveState { Calm, Swarm } // we should maybe add a boss phase in the future? (last wave stuff)


	[Export] EnemySpawner spawner;

	//fields:
	Drill currentDrill;
	public Drill CurrentDrill => currentDrill;
	
	WaveState currentState = WaveState.Swarm;
	public WaveState State => currentState;

	int swarmNumber = 0;
	public int SwarmNumber => swarmNumber;

	public override void _Ready()
	{
		ChangeState(WaveState.Calm);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	public void StartSwarm(Drill drill)
	{
		if (currentState == WaveState.Swarm) //Prevents start of wave, mid-wave. 
		{
			return;
		}
		currentDrill = drill;
		swarmNumber++;
		ChangeState(WaveState.Swarm);
		//Signal that wave started.
		EmitSignal(SignalName.SwarmStarted, swarmNumber);

	}

	private void ChangeState(WaveState newState)
	{
		currentState = newState;
		switch (currentState)
		{
			case WaveState.Calm:
				spawner.StopSpawning();
				GD.Print(what: "Stopped spawning.");
				break;
			case WaveState.Swarm:

				spawner.StartSpawning();
				GD.Print(what: "Swarm " + swarmNumber + " started!");
				break;

			default: break;
		}
	}
	public void EndSwarm()
	{
		if (currentState != WaveState.Swarm) //Prevents start of swarm, mid-wave. 
		{
			return;
		}
		ChangeState(WaveState.Calm);
		EmitSignal(SignalName.SwarmEnded);
	}
}
