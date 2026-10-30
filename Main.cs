using Godot;

// The game manager: finds the main nodes and connects their signals.
public partial class Main : Node2D
{
	PlayerCharacter player;
	Hud hud;
	WaveManager waveManager;
	bool isGameOver = false;

	// Main's _Ready runs after all its children's _Ready, so everything is set up by now.
	public override void _Ready()
	{
		player = GetNode<PlayerCharacter>("Player");
		hud = GetNode<Hud>("HUD");
		waveManager = GetNode<WaveManager>("WaveManager");

		// Every future change...
		player.Died += GameOver;
		player.HealthChanged += hud.OnHealthChanged;
		player.LootChanged += hud.OnLootChanged;
		player.CreditsChanged += hud.OnCreditsChanged;
		player.AmmoChanged += hud.OnAmmoChanged;
		player.InventoryChanged += hud.OnInventoryChanged;
		waveManager.SwarmStarted += hud.OnSwarmStarted;

		// ...plus the values right now (could come from a saved state later).
		hud.OnHealthChanged(player.Health, player.MaxHealth);
		hud.OnLootChanged(player.CarriedOre, player.CarriedScrap);
		hud.OnCreditsChanged(player.Credits);
		hud.OnAmmoChanged(AmmoType.Small, player.GetAmmo(AmmoType.Small), player.GetMaxAmmo(AmmoType.Small));
		hud.OnInventoryChanged(Deployable.Kind.Turret, player.GetCount(Deployable.Kind.Turret), player.GetMax(Deployable.Kind.Turret));
		hud.OnInventoryChanged(Deployable.Kind.Wall, player.GetCount(Deployable.Kind.Wall), player.GetMax(Deployable.Kind.Wall));
		hud.OnSwarmStarted(waveManager.SwarmNumber);
	}

	public override void _Process(double delta)
	{
		if (isGameOver && Input.IsActionJustPressed("restart"))
		{
			GetTree().ReloadCurrentScene();
		}
	}

	void GameOver()
	{
		isGameOver = true;
		GD.Print("Game Over!");
		hud.ShowGameOver();
	}
}
