using Godot;

public partial class Bullet : Area2D
{
	//[Signal] public delegate void HealthChangedEventHandler(int current, int max);

	[Export] float speed = 900;
    [Export] int damage = 20;
    [Export] float lifetime = 2;
    Vector2 direction;
	
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		// subscribe to your OWN BodyEntered signal   (no prefix: it's your own signal, like Velocity)
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// move: GlobalPosition += direction * speed * delta
        // count lifetime down → when ≤ 0, QueueFree()
		GlobalPosition += direction * speed * (float)delta;
		lifetime = lifetime - (float)delta;
		if (lifetime < 0)
		{
			QueueFree();
		}

	}

	public void Launch(Vector2 newDirection)
	{
		direction = newDirection;
        Rotation = direction.Angle();               //(point the sprite the way it flies)
	}

	public void OnBodyEntered(Node2D body)
	{
		if (body is Enemy enemy)
		{
			enemy.Damage(damage);
			QueueFree();
		}  

	}
        
}
