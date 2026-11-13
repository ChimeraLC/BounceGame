// Small chasing enemy that doesn't block bullets and dies touching the player
using Godot;

public class TickEnemy : Enemy
{
    CircleHitbox hitbox;

    protected float moveSpeed = 50;
    protected float turnSpeed = Mathf.Pi / 3;

    public override void _Ready()
    {
        base._Ready();

        hitbox = new CircleHitbox(this, Position, 16);

        health = 1;
        blocksBullets = false;
    }

    protected override void TickMainPlayer(float delta)
    {
        Vector2 offset = Player.Instance.GlobalPosition - GlobalPosition;
        Rotation = Utils.MoveTowardsAngle(Rotation, offset.Angle(), delta * turnSpeed).Item1;
    
        Position += Utils.AngleToVec(Rotation) * delta * moveSpeed;
    }

    public override int DealContactDamage( Player player )
    {
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