// Enemy that hides in the wall and pops out ocasionally
using Godot;

public partial class BurrowerEnemy : Enemy
{
    RectHitbox hitbox;
    Node2D body;

    protected FieldEdge spawnEdge;
    protected Vector2 basePosition;
    protected Vector2 popoutDirection;

    protected bool poppedOut;
    protected float popTimer;

    public override void _Ready()
    {
        base._Ready();

        body  = GetNode<Node2D>("BodySprite");

        
        popTimer = rand.Next() % 2 + 6;

        switch (spawnEdge)
        {
            case FieldEdge.Left:
            popoutDirection = Vector2.Right;
            hitbox = new RectHitbox(this, basePosition + new Vector2(32, 0), new Vector2(32, 16));
            Rotation = Mathf.Pi / 2;
            break;
            case FieldEdge.Bottom:
            popoutDirection = Vector2.Up;
            hitbox = new RectHitbox(this, basePosition + new Vector2(0, -32), new Vector2(16, 32));
            break;
            case FieldEdge.Right:
            popoutDirection = Vector2.Left;
            hitbox = new RectHitbox(this, basePosition + new Vector2(-32, 0), new Vector2(32, 16));
            Rotation = -Mathf.Pi / 2;
            break;
        }

        
        health = 3;
        poppedOut = false;
        hitbox.SetEnabled(false);

        Position = basePosition - 64 * popoutDirection;
    }

    protected override void TickMainPlayer(float delta)
    {
        popTimer -= delta;
        if (popTimer <= 0)
        {
            SwapPopped();
            popTimer += rand.Next() % 2 + 6;
        }
    }

    protected void SwapPopped()
    {
        if (poppedOut)
        {
            // Going into wall
            hitbox.SetEnabled(false);
            Position = basePosition - 64 * popoutDirection;
        }
        else
        {
            hitbox.SetEnabled(true);
            Position = basePosition;
        }

        poppedOut = !poppedOut;
    }

    public override Vector2 GetSpawnpoint()
    {
        (basePosition, spawnEdge) = Utils.GenerateRandomEdgepoint(48, -5, FieldEdge.Left | FieldEdge.Right | FieldEdge.Bottom);
        return basePosition;
    }

    
    public override void UpdateHitbox() {} // Position doesn't change; just deactivated

    public override Hitbox GetHitbox()
    {
        return hitbox;
    }
}