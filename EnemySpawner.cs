using Godot;

public partial class EnemySpawner : Node2D
{
	[Export] PackedScene enemyScene;
	[Export] PackedScene skitterScene;
	[Export] float skitterPackChance = 0.3f;
	[Export] int minPackSize = 6;
	[Export] int maxPackSize = 10;
	[Export] float packSpread = 120;

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

		var offset = RandomOffset(spawnDistance, spawnDistance);
		var spawnPoint = targetNode.GlobalPosition + offset;
		if (GD.Randf() < skitterPackChance)
		{
			int count = GD.RandRange(minPackSize, maxPackSize);
			for (int i = 0; i < count; i++)
			{
				SpawnEnemy(skitterScene, spawnPoint + RandomOffset(0, packSpread));
			}
		}
		else
		{
			SpawnEnemy(enemyScene, spawnPoint);
		}
	}

	private void SpawnEnemy(PackedScene scene, Vector2 position)
	{
		var enemy = scene.Instantiate<Enemy>();
		
		AddChild(enemy);
		enemy.GlobalPosition = position;

		enemy.MakeSwarmEnemy();
	}

	private Vector2 RandomOffset(float minDistance, float maxDistance)
	{
		var angle = (float)GD.RandRange(0, Mathf.Tau);
		Vector2 direction = Vector2.Right.Rotated(angle);
		var randomDistance = (float)GD.RandRange(minDistance, maxDistance);
		return direction * randomDistance;
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
