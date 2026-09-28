using Godot;


public partial class WaveManager : Node
{

	enum WaveState { Waiting, Active, Resting }

	[Export] bool autoStartNextWave = true;
	[Export] float waveDuration = 20;
	[Export] float restDuration = 8;
	[Export] EnemySpawner spawner;

	//fields:
	WaveState currentState = WaveState.Active;
	float timeLeft;
	int waveNumber = 0;

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
