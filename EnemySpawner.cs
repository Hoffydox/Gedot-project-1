using Godot;

public partial class EnemySpawner : Node2D
{
	[Export] PackedScene enemyScene;
	[Export] PackedScene skitterScene;
	[Export] float spawnDistance = 233;
	[Export] float skitterPackChance = 0.3f;
	[Export] int minPackSize = 6;
	[Export] int maxPackSize = 10;
	[Export] float packSpread = 40;
	// One entry per tier: [0] = tier 1, [1] = tier 2, [2] = tier 3.
	[Export] float[] spawnIntervalPerTier = { 2f, 1.4f, 1f };
	[Export] int[] extraPackSizePerTier = { 0, 2, 4 };

	PlayerCharacter player;
	Timer spawnTimer;
	int tier = 1;

	public override void _Ready()
	{
		player = GetTree().GetFirstNodeInGroup("player") as PlayerCharacter;
		spawnTimer = GetNode<Timer>("SpawnTimer");
		spawnTimer.Timeout += OnSpawnTimerTimeout;
	}

	public void StartSpawning(int newTier)
	{
		tier = newTier;
		spawnTimer.WaitTime = spawnIntervalPerTier[tier - 1];
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
			int extra = extraPackSizePerTier[tier - 1];
			int count = GD.RandRange(minPackSize + extra, maxPackSize + extra);
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
		enemy.ResetPhysicsInterpolation();
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
