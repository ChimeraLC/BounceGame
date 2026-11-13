using Godot;
using System;

// Similar to the chaser, but moves in bursts
public class Jellyfish : Enemy
{
    CircleHitbox hitbox;
    private enum JellyState
    {
        Idle, // Between Dashes
        Dashing, // Dashing
    }

    int dashCount = 0;
    float stateTimer = 2;
    const float idleLength = 4;
    const float dashLength = 2;
    const float betweenLength = 0.5f;
    const float rotateRate = 0.25f;
    Vector2 facingDirection;
    float rotationDirection = 0;


    private JellyState currentState = JellyState.Idle;

    public override void _Ready()
    {
        base._Ready();

        hitbox = new CircleHitbox(this, Position, 18);

        health = 4;
    }
    
    public override Vector2 GetSpawnpoint()
    {
        (Vector2 spawnPoint, FieldEdge spawnEdge) = Utils.GenerateRandomEdgepoint(32, -40, 
            FieldEdge.Left | FieldEdge.Right | FieldEdge.Top | FieldEdge.Bottom);

        Rotation = Utils.GetEdgeNormal(spawnEdge).Angle();

        return spawnPoint;
    }

    protected override void TickMain(float delta)
    {

        stateTimer -= delta;

        Rotation += rotationDirection * delta * rotateRate *
            (currentState == JellyState.Idle ? 1 : 0.4f);

        switch (currentState)
        {

            case JellyState.Idle:
            if (stateTimer <= 0)
            {
                if (dashCount == 0)
                    dashCount = Utils.RandRange(1, 4); // Between 1 and 3 dashes
    
                StartDash();
            }
            break;

            case JellyState.Dashing:
            // This isn't precise physics
            Position += facingDirection * delta * stateTimer * stateTimer * 40;
            
            if (stateTimer <= 0)
            {
                dashCount--;
                currentState = JellyState.Idle;
                if (dashCount <= 0)
                {
                    stateTimer = idleLength;
                }
                else
                {
                    // Shorter idles during a dash chain
                    stateTimer = betweenLength;
                }
            }
            break;
        }
    }

    private void StartDash()
    {
        currentState = JellyState.Dashing;
        stateTimer = dashLength;
        facingDirection = Utils.AngleToVec(Rotation);

        if (Player.Instance != null)
            rotationDirection = Utils.MoveTowardsAngleDirection(Rotation, (Player.Instance.Position - Position).Angle());
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
