using Godot;

public partial class Hud : Node2D
{
	[Export] WaveManager waveManager;

	Label healthLabel;
	Label lootLabel;
	Label creditsLabel;
	Label ammoLabel;
	Label turretsLabel;
	Label wallsLabel;
	Label waveLabel;
	Label statusLabel;

	bool gameOver = false;

	public override void _Ready()
	{
		healthLabel = GetNode<Label>("CanvasLayer/HealthLabel");
		lootLabel = GetNode<Label>("CanvasLayer/LootLabel");
		creditsLabel = GetNode<Label>("CanvasLayer/CreditsLabel");
		ammoLabel = GetNode<Label>("CanvasLayer/AmmoLabel");
		turretsLabel = GetNode<Label>("CanvasLayer/TurretsLabel");
		wallsLabel = GetNode<Label>("CanvasLayer/WallsLabel");
		waveLabel = GetNode<Label>("CanvasLayer/WaveLabel");
		statusLabel = GetNode<Label>("CanvasLayer/StatusLabel");
	}

	// The status text changes every frame (drill countdown), so it's read here
	// instead of being pushed by a signal.
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

	// --- Signal listeners (connected in Main) ---

	public void OnHealthChanged(int current, int max)
	{
		healthLabel.Text = "HP: " + current + " / " + max;
	}

	public void OnLootChanged(int ore, int scrap)
	{
		lootLabel.Text = "Ore: " + ore + "  Scrap: " + scrap;
	}

	public void OnCreditsChanged(int credits)
	{
		creditsLabel.Text = "Credits: " + credits;
	}

	public void OnAmmoChanged(AmmoType type, int ammo, int max)
	{
		ammoLabel.Text = type + " ammo: " + ammo + " / " + max;
	}

	public void OnInventoryChanged(Deployable.Kind kind, int count, int max)
	{
		switch (kind)
		{
			case Deployable.Kind.Turret:
				turretsLabel.Text = "Turrets: " + count + " / " + max;
				break;
			case Deployable.Kind.Wall:
				wallsLabel.Text = "Walls: " + count + " / " + max;
				break;
		}
	}

	public void OnSwarmStarted(int swarmNumber)
	{
		if (gameOver)
		{
			return;
		}
		waveLabel.Text = swarmNumber <= 0 ? " " : "Swarm: " + swarmNumber;
	}

	public void ShowGameOver()
	{
		gameOver = true;
		statusLabel.Text = "Game Over - Press R to restart!";
	}
}
