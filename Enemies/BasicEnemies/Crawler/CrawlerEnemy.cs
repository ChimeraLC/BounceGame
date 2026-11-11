// Enemy that crawls around on the background; Larger, but doesn't directly chase player
using System.Data;
using Godot;

public partial class CrawlerEnemy : Enemy
{
    CircleHitbox hitbox;
    private const int radius = 48;
    private Node2D body;

    private const float moveSpeed = 40;
    private const float edgePadding = 120; // How far from edge move targets have to be
    private const float desiredRadius = 360;
    private Vector2 targetPosition;
    private enum CrawlerState
    {
        Idle,
        Moving
    }

    private CrawlerState currentState = CrawlerState.Moving;
    private float idleTimer = 0;
    private const float minIdle = 3;
    private const float maxIdle = 6;

    public override void _Ready()
    {
        base._Ready();

        hitbox = new CircleHitbox(this, Position, radius);

        body = GetNode<Node2D>("CrawlerBody");
    
        health = 3;

        ChooseTarget();
    }

    protected override void TickMain(float delta)
    {
        switch (currentState)
        {
            case CrawlerState.Idle:
                idleTimer -= delta;
                if (idleTimer <= 0)
                {
                    ChooseTarget();
                    currentState = CrawlerState.Moving;
                }
            break;
            case CrawlerState.Moving:
                (Vector2 newPosition, bool reached) = Utils.MoveTowards(Position, targetPosition, delta * moveSpeed);
                Position = newPosition;
                if (reached)
                {
                    currentState = CrawlerState.Idle;
                    idleTimer = Utils.RandRange(minIdle, maxIdle);
                }
            break;
        }
    }

    public override Vector2 GetSpawnpoint()
    {
        return Utils.GenerateRandomEdgepoint(0, -100, FieldEdge.Left | FieldEdge.Right | FieldEdge.Top | FieldEdge.Bottom).Item1;
    }

    private void ChooseTarget(bool firstTime = true)
    {
        targetPosition = new Vector2(Utils.RandRange(Consts.FieldLeft + edgePadding, Consts.FieldRight - edgePadding),
            Utils.RandRange(Consts.FieldTop + edgePadding, Consts.FieldBottom - edgePadding));
        
        // Prefer movement that's further away, but don't force it
        if (firstTime && (targetPosition - Position).LengthSquared() < desiredRadius * desiredRadius)
            ChooseTarget(false);
    }

    public override void UpdateHitbox()
    {
        hitbox.center = Position;
    }

    public override Hitbox GetHitbox()
    {
        return hitbox;
    }
}