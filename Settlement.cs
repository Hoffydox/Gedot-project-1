using Godot;

public partial class Settlement : Area2D
{

	[Export] int orePrice = 10;
	[Export] int scrapPrice = 3;

	PlayerCharacter playerInside;
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;

	}

	public void OnBodyEntered(Node2D body)
	{
		if (body is PlayerCharacter player)
		{
			playerInside = player;
			player.EnterSafeZone();
			GD.Print(what: "Welcome! Press E to Sell");
		}
	}
	public void OnBodyExited(Node2D body)
	{
		if (body is PlayerCharacter player)
		{
			player.ExitSafeZone();
			playerInside = null;
		}
	}


	public override void _Process(double delta)
	{
		if (IsInstanceValid(playerInside) && Input.IsActionJustPressed("interact"))
		{
			SellLoot();
		}
	}
	private void SellLoot()
	{
		int credits = playerInside.CarriedOre * orePrice + playerInside.CarriedScrap * scrapPrice;
		if (credits == 0)
		{
			GD.Print(what: "Nothing to sell");
			return;
		}
		playerInside.AddCredits(credits);
		playerInside.ClearLoot();
		GD.Print(what: "Sold for " + credits + " credits");
	}
}
