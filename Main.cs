using Godot;

public partial class Main : Node2D
{

	bool isGameOver;
	PlayerCharacter fetchedPlayerNode;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		isGameOver = false;
		fetchedPlayerNode = GetNode<PlayerCharacter>("Player");
		fetchedPlayerNode.Died += GameOver;
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

	}
}
