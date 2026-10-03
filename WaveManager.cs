using Godot;


public partial class WaveManager : Node
{

	[Signal] public delegate void WaveStartedEventHandler(int waveNumber);
	public enum WaveState { Waiting, Active, Resting }
	
	[Export] bool autoStartNextWave = true;
	[Export] public float waveDuration = 2;
	[Export] float restDuration = 8;
	[Export] EnemySpawner spawner;

	//fields:
	WaveState currentState = WaveState.Active; 
	public WaveState State => currentState; 
	float timeLeft;

	public float TimeLeft => timeLeft;
	int waveNumber = 0;   
	public int WaveNumber => waveNumber;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ChangeState(WaveState.Waiting);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		switch (currentState)
		{
			case WaveState.Waiting:

				if (Input.IsActionJustPressed("start_wave"))
				{
					StartWave();
				}
				break;

			case WaveState.Active:


				if (timeLeft <= 0)
				{
					ChangeState(WaveState.Resting);
				}
				else
				{
					timeLeft = timeLeft - (float)delta;
				}
				break;



			case WaveState.Resting:
				
					//auto-Start on?
					if (timeLeft <= 0)
					{
						if (autoStartNextWave)
						{
							StartWave();
						} else
						{
							ChangeState(WaveState.Waiting);
						}
					} else
					{
						timeLeft = timeLeft - (float)delta;
					}
					
					break;

			default: break;
		}
	}

	public void StartWave()
	{
		waveNumber++;
		ChangeState(WaveState.Active);
		//Signal that wave started.
		EmitSignal(SignalName.WaveStarted, waveNumber);
		 
	}

	private void ChangeState(WaveState newState)
	{
		currentState = newState;
		switch (currentState)
		{
			case WaveState.Waiting:
				GD.Print(what:"Press E to start the #" + (waveNumber + 1) + " wave!");
				break;
			case WaveState.Active:
			timeLeft = waveDuration;
			spawner.StartSpawning();
			GD.Print(what:"Wave " + waveNumber + " started!");
				break;
			case WaveState.Resting:
				timeLeft = restDuration;
				spawner.StopSpawning();
				GD.Print(what:"Wave over, rest for " + restDuration + " seconds");
				break;
			default: break;
		}
	}


}
