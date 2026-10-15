using Godot;

public partial class Pickup : Area2D
{
	public enum LootType { Ore, Scrap }

	[Export] Texture2D oreTexture;
	[Export] Texture2D scrapTexture;

	LootType type;
	int amount;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	// Called by whoever creates the pickup, right after instantiating it.
	public void Setup(LootType newType, int newAmount)
	{
		type = newType;
		amount = newAmount;

		var sprite = GetNode<Sprite2D>("Sprite2D");
		if (type == LootType.Ore)
		{
			sprite.Texture = oreTexture;
		}
		else if (type == LootType.Scrap)
		{
			sprite.Texture = scrapTexture;
		}
	}

	void OnBodyEntered(Node2D body)
	{
		if (body is PlayerCharacter player)
		{
			player.AddLoot(type, amount);
			QueueFree();
		}
	}
}
