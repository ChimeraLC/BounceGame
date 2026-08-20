using Godot;

// Basic square enemy
public partial class BasicEnemy : Enemy
{
    RectHitbox hitbox;

    public override void _Ready()
    {
        base._Ready();

        hitbox = new RectHitbox(this, Position, new Vector2(50, 50));
        CollisionManager.AddHitbox(hitbox);
    }

    public override void UpdateHitbox()
    {
        // This enemy doesn't move
        return;
    }

    public override Hitbox GetHitbox() { return hitbox; }

    protected override void OnDeath()
    {
        CollisionManager.RemoveHitbox(hitbox);
    }
}