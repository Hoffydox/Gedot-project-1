using Godot;

public partial class Main : Node2D
{

	bool isGameOver;
	PlayerCharacter fetchedPlayerNode;
	Hud fetchedHudNode;

	WaveManager fetchedWaveNode;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		isGameOver = false;
		fetchedPlayerNode = GetNode<PlayerCharacter>("Player");
		fetchedHudNode = GetNode<Hud>("HUD");
		fetchedWaveNode = GetNode<WaveManager>("WaveManager");
		fetchedPlayerNode.Died += GameOver;
		fetchedPlayerNode.HealthChanged += fetchedHudNode.UpdateUI;
		fetchedWaveNode.SwarmStarted += fetchedHudNode.OnSwarmStarted;

		fetchedHudNode.UpdateUI(fetchedPlayerNode.Health, fetchedPlayerNode.MaxHealth);
		fetchedHudNode.OnSwarmStarted(fetchedWaveNode.SwarmNumber);

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("restart") && isGameOver == true)
		{
			GetTree().ReloadCurrentScene();
		}
	}

	public void GameOver()
	{

		isGameOver = true;
		GD.Print(what: "Game Over!");
		fetchedHudNode.ShowGameOver();

	}

}
