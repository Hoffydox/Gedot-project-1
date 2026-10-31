using Godot;

// A patch of ore on the ground. The drill can only be placed on one of these.
public partial class Deposit : Node2D
{
	[Export] int tier = 1;
	// One drawing per tier: [0] = tier 1 ... [4] = tier 5.
	[Export] Texture2D[] tierTextures;
	[Export] Color exhaustedTint = new Color(0.4f, 0.4f, 0.4f);

	bool exhausted;
	Sprite2D sprite;
	Label promptLabel;

	public int Tier => tier;
	public bool IsExhausted => exhausted;

	public override void _Ready()
	{
		AddToGroup("deposits");
		sprite = GetNode<Sprite2D>("Sprite2D");
		sprite.Texture = tierTextures[tier - 1];
		promptLabel = GetNode<Label>("PromptLabel");
		promptLabel.Text = "Tier " + tier + " deposit\n[F] Drill";
		promptLabel.Visible = false;
	}

	// Called by the drill when it finishes. The deposit can't be drilled again.
	public void Exhaust()
	{
		exhausted = true;
		sprite.Modulate = exhaustedTint;
		ShowPrompt(false);
	}

	public void ShowPrompt(bool show)
	{
		promptLabel.Visible = show;
	}
}
