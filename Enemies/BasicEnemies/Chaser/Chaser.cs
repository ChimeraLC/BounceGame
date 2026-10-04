using Godot;

// Enemy that pops out of walls and then  homes in on the player
public class ChaserEnemy : Enemy
{
    // TODO: Maybe needs a polygon hitbox of some sort?
    RectHitbox hitbox;

    public override void _Ready()
    {
        base._Ready();

        hitbox = new RectHitbox(this, Position, new Vector2(30, 30));
        CollisionManager.AddHitbox(hitbox);
    }

    public override Vector2 GetSpawnpoint()
    {
        return Utils.GenerateRandomEdgepoint(32, 0, FieldEdge.Left | FieldEdge.Right).Item1;
    }

    protected override void TickMain(float delta)
    {
        
    }

    public override void UpdateHitbox()
    {
        hitbox.center = Position;
    }

    public override Hitbox GetHitbox()
    {
        return hitbox;
    }

    protected override void OnDeath()
    {
        CollisionManager.RemoveHitbox(hitbox);
    }
}