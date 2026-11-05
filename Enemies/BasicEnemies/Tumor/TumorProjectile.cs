using Godot;

public partial class TumorProjectile : Enemy
{
    private CircleHitbox hitbox;
    private Node2D spriteBody;
    private Vector2 direction;
    private float lifetime = 10;
    private const float moveSpeed = 30;

    public override void _Ready()
    {
        base._Ready();
    
        hitbox = new CircleHitbox(this, Position, 10);

        health = 1;

        if (Player.Instance != null)
        {
            direction = (Player.Instance.GlobalPosition - GlobalPosition).Normalized();
        }
    }

    protected override void TickMain(float delta)
    {
        GlobalPosition += direction * delta * 30;

        lifetime -= delta;
        if (lifetime <= 0 && IsAlive())
            Suicide();
    }


    public override void UpdateHitbox()
    {
        hitbox.center = GlobalPosition;
    }

    public override Hitbox GetHitbox() { return hitbox; }
}