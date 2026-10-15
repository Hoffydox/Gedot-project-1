using Godot;


public partial class Hud : Node2D
{
	[Export] WaveManager waveManager;

	Label healthLabel;
	Label lootLabel;
	Label waveLabel;
	Label statusLabel;
	Label creditsLabel;
	Label turretsLabel;
	Label ammoLabel;
	Label wallsLabel;


	bool gameOver = false;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		healthLabel = GetNode<Label>("CanvasLayer/HealthLabel");
		lootLabel = GetNode<Label>("CanvasLayer/LootLabel");
		waveLabel = GetNode<Label>("CanvasLayer/WaveLabel");
		statusLabel = GetNode<Label>("CanvasLayer/StatusLabel");
		creditsLabel = GetNode<Label>("CanvasLayer/CreditsLabel");
		turretsLabel = GetNode<Label>("CanvasLayer/TurretsLabel");
		wallsLabel = GetNode<Label>("CanvasLayer/WallsLabel");
		ammoLabel = GetNode<Label>("CanvasLayer/AmmoLabel");


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
			case WaveManager.WaveState.Swarm:
				var drill = waveManager.CurrentDrill;
				if (IsInstanceValid(drill))
				{
					statusLabel.Text = "SWARM! Drilling: " + Mathf.Ceil(drill.DrillTimeLeft) + "s";
				}
				else
				{
					statusLabel.Text = "SWARM!";
				}

				break;

			case WaveManager.WaveState.Calm:
				statusLabel.Text = "Place a drill (F) to start drilling";
				break;
		}
	}

	public void UpdateUI(int current, int max)
	{
		healthLabel.Text = "HP: " + current + " / " + max;
	}
	public void OnSwarmStarted(int swarmNumber)
	{
		if (gameOver)
		{
			return;
		}
		if (swarmNumber <= 0)
		{
			waveLabel.Text = " ";
		}
		else
		{
			waveLabel.Text = "Swarm: " + swarmNumber;
		}
	}
	public void ShowGameOver()
	{

		gameOver = true;
		statusLabel.Text = "Game Over - Press R to restart!";
	}

	public void OnLootChanged(int ore, int scrap)
	{
		lootLabel.Text = "Ore: " + ore + " Scrap: " + scrap + " ";
	}
	public void OnCreditsChanged(int credits)
	{
		creditsLabel.Text = "Credits: " + credits + " ";
	}
	public void OnInventoryChanged(Deployable.Kind kind, int count, int max)
	{
		if (kind == Deployable.Kind.Turret)
		{
		turretsLabel.Text = "Turrets: " + count + " / " + max;
		} else if (kind == Deployable.Kind.Wall)
		{
			wallsLabel.Text = "Walls: " + count + " / " + max;
		}
	}
	public void OnAmmoChanged(int ammo, int max)
	{
		ammoLabel.Text = "Ammo: " + ammo + " / " + max;
	}
}
