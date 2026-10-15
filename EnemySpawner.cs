using Godot;

public partial class EnemySpawner : Node2D
{
	[Export] PackedScene enemyScene;
	[Export] PackedScene skitterScene;
	[Export] float spawnDistance = 700;
	[Export] float skitterPackChance = 0.3f;
	[Export] int minPackSize = 6;
	[Export] int maxPackSize = 10;
	[Export] float packSpread = 120;

	PlayerCharacter player;
	Timer spawnTimer;

	public override void _Ready()
	{
		player = GetTree().GetFirstNodeInGroup("player") as PlayerCharacter;
		spawnTimer = GetNode<Timer>("SpawnTimer");
		spawnTimer.Timeout += OnSpawnTimerTimeout;
	}

	public void StartSpawning()
	{
		spawnTimer.Start();
	}

	public void StopSpawning()
	{
		spawnTimer.Stop();
	}

	void OnSpawnTimerTimeout()
	{
		if (!IsInstanceValid(player))
		{
			return;
		}

		var spawnPoint = player.GlobalPosition + RandomOffset(spawnDistance, spawnDistance);

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

	void SpawnEnemy(PackedScene scene, Vector2 position)
	{
		var enemy = scene.Instantiate<Enemy>();
		AddChild(enemy);
		enemy.GlobalPosition = position;
		enemy.MakeSwarmEnemy();
	}

	// A random direction, scaled to a random distance between min and max.
	Vector2 RandomOffset(float minDistance, float maxDistance)
	{
		float angle = (float)GD.RandRange(0, Mathf.Tau);
		Vector2 direction = Vector2.Right.Rotated(angle);
		float distance = (float)GD.RandRange(minDistance, maxDistance);
		return direction * distance;
	}
}
