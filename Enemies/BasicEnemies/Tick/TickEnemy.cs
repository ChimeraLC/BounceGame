// Small chasing enemy that doesn't block bullets and dies touching the player
using Godot;

// Absolutely dreadful name to use, given TickEnemy() should also be a function 
public class TickEnemy : Enemy
{
    CircleHitbox hitbox;

    protected float moveSpeed = 30;
    protected float turnSpeed = Mathf.Pi / 3;

    protected float spawnTimer = 0;
    protected const float spawnDuration = 1f;

    public override void _Ready()
    {
        base._Ready();

        hitbox = new CircleHitbox(this, Position, 16);

        health = 1;
        blocksBullets = false;
    }

    protected override void TickMainPlayer(float delta)
    {
        float actualTurnSpeed = turnSpeed;
        float actualMoveSpeed = moveSpeed;

        if (spawnTimer > 0)
        {
            actualMoveSpeed += 40 * spawnTimer * spawnTimer;
            actualTurnSpeed *= (spawnDuration - spawnTimer) / spawnDuration;
            spawnTimer -= delta;
        }
        Vector2 offset = Player.Instance.GlobalPosition - GlobalPosition;
        Rotation = Utils.MoveTowardsAngle(Rotation, offset.Angle(), delta * actualTurnSpeed).Item1;
    
        Position += Utils.AngleToVec(Rotation) * delta * actualMoveSpeed;
    }

    public void Initialize(Vector2 position, Vector2 direction)
    {
        GlobalPosition = position;
        Rotation = direction.Angle();
        spawnTimer = spawnDuration;
    }

    public override int DealContactDamage( Player player )
    {
        if (spawnTimer > 0)
            return 0;
            
        Suicide();
        return 1;
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