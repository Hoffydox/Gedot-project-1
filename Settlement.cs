using Godot;

public partial class Settlement : Area2D
{

	[Export] int orePrice = 10;
	[Export] int scrapPrice = 3;
	[Export] int turretPrice = 30;
	[Export] int ammoPrice = 10;
	[Export] int ammoPackSize = 15;

	Control shopMenu;
	Button sellButton;
	Button buyTurretButton;
	Button buyAmmoButton;
	Button closeButton;

	PlayerCharacter playerInside;
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;

		shopMenu = GetNode<PanelContainer>("CanvasLayer/ShopMenu");
		sellButton = GetNode<Button>("CanvasLayer/ShopMenu/VBoxContainer/SellButton");
		buyTurretButton = GetNode<Button>("CanvasLayer/ShopMenu/VBoxContainer/BuyTurretButton");
		closeButton = GetNode<Button>("CanvasLayer/ShopMenu/VBoxContainer/CloseButton");
		buyAmmoButton = GetNode<Button>("CanvasLayer/ShopMenu/VBoxContainer/BuyAmmoButton");


		sellButton.Pressed += OnSellPressed;
		buyTurretButton.Pressed += OnBuyTurretPressed;
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
		BuyTurret();
	}
	public void BuyTurret()
	{
		if (!IsInstanceValid(playerInside))
		{
			return;
		}
		if (playerInside.GetCount(Deployable.Kind.Turret) >= playerInside.GetMax(Deployable.Kind.Turret))
		{
			GD.Print(what: "Pack is full");
			return;
		}
		if (!playerInside.TrySpendCredits(turretPrice))
		{
			GD.Print(what: "Not Enough credits.");
			return;
		}
		playerInside.TryAddItem(Deployable.Kind.Turret);
		GD.Print(what: "Bought a turret");
	}
	public void OnBuyAmmoPressed()
	{
		BuyAmmo();
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
