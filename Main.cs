using Godot;

public partial class Main : Node2D
{

	bool isGameOver;
	PlayerCharacter fetchedPlayerNode;
	Hud fetchedHudNode;

	WaveManager fetchedWaveNode;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		isGameOver = false;
		fetchedPlayerNode = GetNode<PlayerCharacter>("Player");
		fetchedHudNode = GetNode<Hud>("HUD");
		fetchedWaveNode = GetNode<WaveManager>("WaveManager");
		
		fetchedPlayerNode.Died += GameOver;
		fetchedPlayerNode.HealthChanged += fetchedHudNode.UpdateUI;
		fetchedPlayerNode.LootChanged += fetchedHudNode.OnLootChanged;
		
		fetchedWaveNode.SwarmStarted += fetchedHudNode.OnSwarmStarted;
		fetchedPlayerNode.CreditsChanged += fetchedHudNode.OnCreditsChanged;
		
		fetchedPlayerNode.InventoryChanged += fetchedHudNode.OnInventoryChanged;
		fetchedPlayerNode.AmmoChanged += fetchedHudNode.OnAmmoChanged;

		fetchedHudNode.UpdateUI(fetchedPlayerNode.Health, fetchedPlayerNode.MaxHealth);
		fetchedHudNode.OnSwarmStarted(fetchedWaveNode.SwarmNumber);
		fetchedHudNode.OnCreditsChanged(fetchedPlayerNode.Credits);
		fetchedHudNode.OnInventoryChanged(Deployable.Kind.Turret, fetchedPlayerNode.GetCount(Deployable.Kind.Turret), fetchedPlayerNode.GetMax(Deployable.Kind.Turret));
		fetchedHudNode.OnAmmoChanged(fetchedPlayerNode.Ammo, fetchedPlayerNode.MaxAmmo);

		fetchedHudNode.OnLootChanged(fetchedPlayerNode.CarriedOre, fetchedPlayerNode.CarriedScrap); // intantiate loot. Could be taken from savedstate later.

	}		

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("restart") && isGameOver == true)
		{
			GetTree().ReloadCurrentScene();
		}
	}

	public void GameOver()
	{

		isGameOver = true;
		GD.Print(what: "Game Over!");
		fetchedHudNode.ShowGameOver();

	}

}
