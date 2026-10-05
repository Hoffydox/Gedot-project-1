using Godot;


public partial class Hud : Node2D
{
	[Export] WaveManager waveManager;

	Label healthLabel;
	Label waveLabel;
	Label statusLabel;

	bool gameOver = false;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		healthLabel = GetNode<Label>("CanvasLayer/HealthLabel");
		waveLabel = GetNode<Label>("CanvasLayer/WaveLabel");
		statusLabel = GetNode<Label>("CanvasLayer/StatusLabel");



	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		if (gameOver)
		{
			return;
		}
		switch (waveManager.State)
		{
			case WaveManager.WaveState.Active:
				statusLabel.Text = "Active";
				break;
			case WaveManager.WaveState.Resting:
				statusLabel.Text = "Resting for " + Mathf.Ceil(waveManager.TimeLeft) + " Seconds";
				break;
			case WaveManager.WaveState.Waiting:
				statusLabel.Text = "Press E to start the next wave!"; break;
		}
	}

	public void UpdateUI(int current, int max)
	{
		healthLabel.Text = "HP: " + current + " / " + max;
	}
	public void OnWaveStarted(int waveNumber)
	{
		if (gameOver)
		{
			return;
		}
		if (waveNumber <= 0)
		{
			waveLabel.Text = " ";
		}
		else
		{
			waveLabel.Text = "Wave: " + waveNumber;
		}
	}
	public void ShowGameOver()
	{

		gameOver = true;
		statusLabel.Text = "Game Over - Press R to restart!";
	}
}
