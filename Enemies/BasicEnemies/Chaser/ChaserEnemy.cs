using Godot;

// Enemy that pops out of walls and then  homes in on the player
public class ChaserEnemy : Enemy
{
    protected enum ChaserState
    {
        None,
        Spawning,
        Chasing,
        Exploding,
    }

    // TODO: Maybe needs a polygon hitbox of some sort?
    CircleHitbox hitbox;

    protected float moveSpeed = 40;
    protected float turnSpeed = Mathf.Pi / 3;
    protected float facingAngle = 0;
    protected Vector2 spawnPoint;
    protected Vector2 spawnDirection;
    protected ChaserState state = ChaserState.Spawning;
    protected float spawnTimer = 0;

    public override void _Ready()
    {
        base._Ready();

        hitbox = new CircleHitbox(this, Position, 12);

        health = 3;
    }

    public override Vector2 GetSpawnpoint()
    {
        FieldEdge spawnEdge;
        (spawnPoint, spawnEdge) = Utils.GenerateRandomEdgepoint(32, -40, FieldEdge.Left | FieldEdge.Right);

        facingAngle = spawnEdge == FieldEdge.Left ? 0 : Mathf.Pi;
        Rotation = facingAngle;
        spawnDirection = spawnEdge == FieldEdge.Left ? Vector2.Right : Vector2.Left;

        return spawnPoint;
    }

    private const float spawnDuration = 2;

    protected override void TickMainPlayer(float delta)
    {
        switch (state)
        {
        case ChaserState.Spawning: // Pop out of the wall
            TickSpawning(delta);
            break;
        case ChaserState.Chasing:
            TickChasing(delta);
            break;
        }
    }

    protected virtual void TickSpawning(float delta)
    {
        spawnTimer += delta;

        float spawnVal = Mathf.Min(spawnDuration, spawnTimer);

        float spawnOffset = (2 * spawnDuration - spawnVal) * spawnVal;

        GlobalPosition = spawnPoint + spawnOffset * spawnDirection * 25;
            
        if (spawnTimer >= spawnDuration + 1)
            state = ChaserState.Chasing;
    }

    protected virtual void TickChasing(float delta)
    {
        Vector2 offset = Player.Instance.GlobalPosition - GlobalPosition;
        facingAngle = Utils.MoveTowardsAngle(facingAngle, offset.Angle(), delta * turnSpeed).Item1;

        Rotation = facingAngle;

        Position += new Vector2(Mathf.Cos(facingAngle), Mathf.Sin(facingAngle)) * delta * moveSpeed;
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