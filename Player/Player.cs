using Godot;
using System;

public class Player : Node2D
{

    // Movement
    private const float Bounciness = 0.75f;
    private const float MinVertSpeed = 300;
    private const float MinSpeed = 30;
    private const float HorControl = 100.0f;
    private Vector2 velocity;

    // Guns
    private FakeGun currentFakeGun;
    private Gun currentGun;
    private Gun nextGun;
    private Line2D previewLine;
    public override void _Ready()
    {
        GameManager.RegisterPlayerInstance(this);
    
        velocity = Vector2.Zero;

        previewLine = GetNode<Line2D>("Preview");
        previewLine.Points = new Vector2[] {Vector2.Zero, Vector2.Zero};

        currentFakeGun = GetNode<FakeGun>("FakeGun");
        currentGun = new Pistol();
    }

    public void Tick(float delta)
    {
        RunPhysics(delta);

        TickWeapon(delta);
    }

    public void ImpulseVelocity(Vector2 impulse)
    {
        velocity += impulse;
    }

    private void TickWeapon(float delta)
    {
        Field field = GameManager.GetField(); // Getting each frame is probably slow

        // Currently, just a straight line to the mouse
        Vector2 aimDirection = GetAimDirection();

        // Find first collision with walls
        // TODO: Move this into subfunction to allow for bounces
        (float firstHit, _) = CollisionManager.GetFirstRayhit(Position, aimDirection);
        // if (aimDirection.x > 0)
        //     firstHit = Mathf.Min(firstHit, (field.rightBound - Position.x) / aimDirection.x);
        // if (aimDirection.x < 0)
        //     firstHit = Mathf.Min(firstHit, (field.leftBound - Position.x) / aimDirection.x);
        // if (aimDirection.y > 0)
        //     firstHit = Mathf.Min(firstHit, (field.bottomBound - Position.y) / aimDirection.y);
        // if (aimDirection.y < 0)
        //     firstHit = Mathf.Min(firstHit, (field.topBound - Position.y) / aimDirection.y);

        previewLine.SetPointPosition(1, aimDirection * firstHit);
        
        currentGun.TickGun(this, aimDirection, currentFakeGun, delta);
        if (Input.IsActionJustPressed("key_fire"))
        {
            if (currentGun.CanFire())
            {
                Logger.Log("Fired Weapon", LogLevel.info);
                currentGun.Fire( this, aimDirection );
            }
        }
    }

    /// <summary>
    /// Returns normalized aim direction
    /// </summary>
    private Vector2 GetAimDirection()
    {
        // TODO: Cache this, so it's not running multiple times a frame
        return (GetGlobalMousePosition() - Position).Normalized();
    }

    private void RunPhysics(float delta)
    {
        if (delta <= 0)
        {
            Logger.Log("RunPhysics recieved a negative delta", LogLevel.error);
            return;
        }

        Field field = GameManager.GetField(); // Getting each frame is probably slow

        // Acceleration should be constant during a single runPhysics call
        Vector2 acceleration = Vector2.Down * Constants.Gravity;
        
        // acceleration += HorControl * Vector2.Right * Input.GetAxis("key_left", "key_right");
    
        // TODO: Air resistance

        // Add on any additional accelerations

        // Overcomplicated quadratic logic so end position will be the same regardless of framerate or hitches
        // Makes non-basic collision logic a lot harder though, so maybe just movement is through this, enemy collision still straightline?
        Vector2 testPosition = Position + (velocity + acceleration * delta / 2) * delta;

        float preDelta = Mathf.Inf;
        int collisionState = 0; // 0 - none, 1 - horizontal, 2 - bottom, 3 - top

        // TODO: If two happen at once, and combine these together; find the first hit and do that one
        if (testPosition.x > field.rightBound)
        {
            Logger.Log("Hit Right", LogLevel.info, LogChannel.Movement);

            // Find cross delta
            float dist = field.rightBound - Position.x;

            preDelta = Utils.FirstPositiveQuadratic(acceleration.x / 2, velocity.x, -dist);
            collisionState = 1;
        }
        else if (testPosition.x < field.leftBound)
        {
            Logger.Log("Hit Left", LogLevel.info, LogChannel.Movement);

            // Find cross delta
            float dist = field.leftBound - Position.x;

            preDelta = Utils.FirstPositiveQuadratic(acceleration.x / 2, velocity.x, -dist);
            collisionState = 1;
        }
        
        if (testPosition.y > field.bottomBound)
        {
            Logger.Log("Hit Bottom", LogLevel.info, LogChannel.Movement);

            // Find cross delta
            float dist = field.bottomBound - Position.y;
            // (vel.y + acc.y * alpha / 2) * alpha = dist
            // alpha^2 * acc.y / 2 + vel.y * alpha - dist = 0
            float newDelta = Utils.FirstPositiveQuadratic(acceleration.y / 2, velocity.y, -dist);
            if (newDelta < preDelta)
            {
                preDelta = newDelta;
                collisionState = 2;
            }
        }
        else if (testPosition.y < field.topBound)
        {
            Logger.Log("Hit Top", LogLevel.info, LogChannel.Movement);

            // Find cross delta
            float dist = field.topBound - Position.y;
            float newDelta = Utils.FirstPositiveQuadratic(acceleration.y / 2, velocity.y, -dist);
            if (newDelta < preDelta)
            {
                preDelta = newDelta;
                collisionState = 3;
            }
        }

        if (collisionState == 0)
        {
            Position = testPosition;
            velocity += acceleration * delta;
            return;
        }

        Position += (velocity + acceleration * preDelta / 2) * preDelta;
        velocity += acceleration * preDelta;

        // Collision bounces
        switch(collisionState)
        {
            case 1: // Sides
            velocity.x *= -1;

            // Dampen and keep at minimum speed
            velocity.x *= Bounciness;
            if (Mathf.Abs(velocity.x) < MinSpeed)
                velocity.x = Mathf.Sign(velocity.x) * MinSpeed;
            break;
            case 2: // Bottom
            velocity.y *= -1;

            velocity.y *= Bounciness;
            if (Mathf.Abs(velocity.y) < MinVertSpeed)
                velocity.y = MinVertSpeed * Mathf.Sign(velocity.y);
            break;
            case 3: // Top
            velocity.y *= -1;

            // No minspeed
            velocity.y *= Bounciness;
            break;
        }
        
        RunPhysics(delta - preDelta);
    }
}