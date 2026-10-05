using Godot;


public partial class Pickup : Area2D
{
	public enum LootType { Ore, Scrap }
	[Export] Texture2D oreTexture;
	[Export] Texture2D scrapTexture;
	LootType type;
	int amount;

	public void Setup(LootType newType, int newAmount)
	{
		type = newType;
		amount = newAmount;
		var spriteUsed = GetNode<Sprite2D>("Sprite2D");
		if (type == LootType.Ore)
		{
			spriteUsed.Texture = oreTexture;
		}
		else if (type == LootType.Scrap)
		{
			spriteUsed.Texture = scrapTexture;
		}


	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}


	public void OnBodyEntered(Node2D body)
	{
		if (body is PlayerCharacter player)
		{
			player.AddLoot(type, amount);
			QueueFree();
		}

	}

}
