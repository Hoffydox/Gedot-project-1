using Godot;

public partial class EnemySpawner : Node2D
{
	[Export] PackedScene enemyScene;
	[Export] float spawnDistance = 700;
	PlayerCharacter targetNode;
	Timer spawnTimer;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// finds player
		targetNode = GetTree().GetFirstNodeInGroup("player") as PlayerCharacter;
		// waits for Timeout signal and calls method: OnSpawnTimerTimeout;
		spawnTimer = GetNode<Timer>("SpawnTimer");
		spawnTimer.Timeout += OnSpawnTimerTimeout;
	}

	public void OnSpawnTimerTimeout()
	{
		if (!IsInstanceValid(targetNode))
		{
			return;
		}

		float spawnAngle = (float)GD.RandRange(0, Mathf.Tau);
		Vector2 spawnDirection = Vector2.Right.Rotated(spawnAngle);

		var offset = spawnDirection * spawnDistance;
		var spawnPoint = targetNode.GlobalPosition + offset;
		var enemy = enemyScene.Instantiate<Enemy>();

		AddChild(enemy);
		enemy.GlobalPosition = spawnPoint;
	}

	public void StartSpawning()
	{
		spawnTimer.Start();

	}
	
	public void StopSpawning()
	{
		spawnTimer.Stop();

	}


}
