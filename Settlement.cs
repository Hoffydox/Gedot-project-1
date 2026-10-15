using Godot;

public partial class Settlement : Area2D
{

	[Export] int orePrice = 10;
	[Export] int scrapPrice = 3;
	[Export] int turretPrice = 30;
	[Export] int ammoPrice = 10;
	[Export] int ammoPackSize = 15;
	[Export] int wallPrice = 15;

	Control shopMenu;
	Button sellButton;
	Button buyTurretButton;
	Button buyAmmoButton;
	Button buyWallButton;
	Button closeButton;

	PlayerCharacter playerInside;
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;

		shopMenu = GetNode<PanelContainer>("CanvasLayer/ShopMenu");
		sellButton = GetNode<Button>("CanvasLayer/ShopMenu/VBoxContainer/SellButton");
		buyTurretButton = GetNode<Button>("CanvasLayer/ShopMenu/VBoxContainer/BuyTurretButton");
		buyWallButton = GetNode<Button>("CanvasLayer/ShopMenu/VBoxContainer/BuyWallButton");
		closeButton = GetNode<Button>("CanvasLayer/ShopMenu/VBoxContainer/CloseButton");
		buyAmmoButton = GetNode<Button>("CanvasLayer/ShopMenu/VBoxContainer/BuyAmmoButton");


		sellButton.Pressed += OnSellPressed;
		buyTurretButton.Pressed += OnBuyTurretPressed;
		buyWallButton.Pressed += OnBuyWallPressed;
		closeButton.Pressed += OnClosePressed;
		buyAmmoButton.Pressed += OnBuyAmmoPressed;
	}

	public void OnBodyEntered(Node2D body)
	{
		if (body is PlayerCharacter player)
		{
			playerInside = player;
			player.EnterSafeZone();
			GD.Print(what: "Welcome! Press E to Trade");
		}
	}
	public void OnBodyExited(Node2D body)
	{
		if (body is PlayerCharacter player)
		{
			player.ExitSafeZone();
			playerInside = null;
			shopMenu.Visible = false;
		}
	}


	public override void _Process(double delta)
	{
		if (IsInstanceValid(playerInside) && Input.IsActionJustPressed("interact"))
		{
			shopMenu.Visible = true;
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
	public void OnSellPressed()
	{
		SellLoot();
	}
	public void OnBuyTurretPressed()
	{
		BuyItem(Deployable.Kind.Turret, turretPrice);
	}
	public void OnBuyWallPressed()
	{
		BuyItem(Deployable.Kind.Wall, wallPrice);
	}
	public void OnBuyAmmoPressed()
	{
		BuyAmmo();
	}
	public void BuyItem(Deployable.Kind kind, int price)
	{
		if (!IsInstanceValid(playerInside))
		{
			return;
		}
		if (playerInside.GetCount(kind) >= playerInside.GetMax(kind))
		{
			GD.Print(what: "Pack is full");
			return;
		}
		if (!playerInside.TrySpendCredits(price))
		{
			GD.Print(what: "Not Enough credits.");
			return;
		}
		playerInside.TryAddItem(kind);
		GD.Print(what: "Bought a " + kind);
	}

	void BuyAmmo()
	{
		if (!IsInstanceValid(playerInside))
		{
			return;
		}

		if (playerInside.Ammo >= playerInside.MaxAmmo)
		{
			GD.Print("Ammo is full");
			return;
		}

		if (!playerInside.TrySpendCredits(ammoPrice))
		{
			GD.Print("Not enough credits");
			return;
		}

		playerInside.TryAddAmmo(ammoPackSize);
		GD.Print("Bought " + ammoPackSize + " ammo");
	}
	public void OnClosePressed()
	{
		shopMenu.Visible = false;
	}
}
