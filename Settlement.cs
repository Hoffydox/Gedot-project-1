using Godot;

public partial class Settlement : Area2D
{
	[Export] int orePrice = 10;
	[Export] int scrapPrice = 3;
	[Export] int turretPrice = 30;
	[Export] int wallPrice = 15;
	[Export] int cratePrice = 120;
	[Export] int ammoPrice = 10;
	[Export] int ammoPackSize = 15;

	Control shopMenu;
	PlayerCharacter playerInside; // null when nobody is inside

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;

		shopMenu = GetNode<PanelContainer>("CanvasLayer/ShopMenu");

		// The buttons are only needed here, to connect them, so they can be locals.
		GetNode<Button>("CanvasLayer/ShopMenu/VBoxContainer/SellButton").Pressed += SellLoot;
		GetNode<Button>("CanvasLayer/ShopMenu/VBoxContainer/BuyTurretButton").Pressed += OnBuyTurretPressed;
		GetNode<Button>("CanvasLayer/ShopMenu/VBoxContainer/BuyWallButton").Pressed += OnBuyWallPressed;
		GetNode<Button>("CanvasLayer/ShopMenu/VBoxContainer/BuyCrateButton").Pressed += OnBuyCratePressed;
		GetNode<Button>("CanvasLayer/ShopMenu/VBoxContainer/BuyAmmoButton").Pressed += OnBuyAmmoPressed;
		GetNode<Button>("CanvasLayer/ShopMenu/VBoxContainer/CloseButton").Pressed += CloseShop;
	}

	public override void _Process(double delta)
	{
		if (IsInstanceValid(playerInside) && Input.IsActionJustPressed("interact"))
		{
			shopMenu.Visible = true;
		}
	}

	void OnBodyEntered(Node2D body)
	{
		if (body is PlayerCharacter player)
		{
			playerInside = player;
			player.EnterSafeZone();
			GD.Print("Welcome! Press E to trade");
		}
	}

	void OnBodyExited(Node2D body)
	{
		if (body is PlayerCharacter player)
		{
			player.ExitSafeZone();
			playerInside = null;
			CloseShop();
		}
	}

	void CloseShop()
	{
		shopMenu.Visible = false;
	}

	void SellLoot()
	{
		if (!IsInstanceValid(playerInside))
		{
			return;
		}

		int credits = playerInside.CarriedOre * orePrice + playerInside.CarriedScrap * scrapPrice;
		if (credits == 0)
		{
			GD.Print("Nothing to sell");
			return;
		}
		playerInside.AddCredits(credits);
		playerInside.ClearLoot();
		GD.Print("Sold for " + credits + " credits");
	}

	void OnBuyTurretPressed()
	{
		BuyItem(Deployable.Kind.Turret, turretPrice);
	}

	void OnBuyWallPressed()
	{
		BuyItem(Deployable.Kind.Wall, wallPrice);
	}

	void OnBuyCratePressed()
	{
		BuyItem(Deployable.Kind.AmmoCrate, cratePrice);
	}

	// Safe order for every purchase: valid player -> room in the pack -> pay -> add.
	void BuyItem(Deployable.Kind kind, int price)
	{
		if (!IsInstanceValid(playerInside))
		{
			return;
		}
		if (playerInside.GetCount(kind) >= playerInside.GetMax(kind))
		{
			GD.Print("Pack is full");
			return;
		}
		if (!playerInside.TrySpendCredits(price))
		{
			GD.Print("Not enough credits");
			return;
		}
		playerInside.TryAddItem(kind);
		GD.Print("Bought a " + kind);
	}

	void OnBuyAmmoPressed()
	{
		BuyAmmo(AmmoType.Small, ammoPrice);
	}

	// Same safe order as BuyItem: valid player -> room for it -> pay -> add.
	void BuyAmmo(AmmoType type, int price)
	{
		if (!IsInstanceValid(playerInside))
		{
			return;
		}
		if (playerInside.GetAmmo(type) >= playerInside.GetMaxAmmo(type))
		{
			GD.Print(type + " ammo is full");
			return;
		}
		if (!playerInside.TrySpendCredits(price))
		{
			GD.Print("Not enough credits");
			return;
		}
		playerInside.TryAddAmmo(type, ammoPackSize);
		GD.Print("Bought " + ammoPackSize + " " + type + " ammo");
	}
}
