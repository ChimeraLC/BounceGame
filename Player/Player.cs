using Godot;
using System;
using System.Collections.Generic;

public class Player : Node2D
{
    // Health
    private int health = 3;
    private const float InvulTimer = 3;
    private float hitTimer = 0;
    private const float MinHitSpeed = 300;

    // Movement
    private const int playerRadius = 12;
    private const float Bounciness = 0.75f;
    private const float MinVertSpeed = 300;
    private const float MinSpeed = 30;
    private const float HorControl = 100.0f;
    private Vector2 velocity;
    private Vector2 preframe; // Position at start of frame

    // Guns
    private List<Gun> guns = new List<Gun>();
    private Gun currentGun;
    private FakeGun fakeGun;
    private int currentGunIndex;
    public override void _Ready()
    {
        GameManager.RegisterPlayerInstance(this);
    
        velocity = Vector2.Zero;

        fakeGun = GetNode<FakeGun>("FakeGun");

        currentGunIndex = 0;

        guns.Add(new Pistol());
        guns.Add(new Shotgun());
        guns.Add(new Laser());

        currentGun = guns[0];
    }

    public void Tick(float delta)
    {
#if DEBUG
        if (DebugManager.Ghosting)
        {
            RunGhosting(delta);
        }
        else
#endif
        {
            RunPhysics(delta);
            RunCollisions(delta);
        }
        
        TickWeapon(delta);

        Update();
    }

    public int GetHP() { return health; }

    public void ImpulseVelocity(Vector2 impulse)
    {
        velocity += impulse;
    }

    private void TickWeapon(float delta)
    {
        Field field = GameManager.GetField(); // Getting each frame is probably slow

        // Currently, just a straight line to the mouse
        Vector2 aimDirection = GetAimDirection();

        if (Input.IsActionJustPressed("key_swapUp"))
            SwapGun(true);

        if (Input.IsActionJustPressed("key_swapDown"))
            SwapGun(false);

        for (int i = 0; i < guns.Count; i++)
        {
            if (i == currentGunIndex)
            {
                guns[i].TickGun(this, aimDirection, fakeGun, delta);
            }
            else
            {
                guns[i].TickGunPassive(delta);
            }
        }

        if (Input.IsActionJustPressed("key_fire") ||
            (currentGun.heldWeapon && Input.IsActionPressed("key_fire")))
        {
            if (currentGun.CanFire())
            {
                currentGun.Fire( this, aimDirection, delta );
            }
        }

        if (Input.IsActionJustReleased("key_fire"))
        {
            currentGun.ReleaseFire();
        }

        DebugManager.DebugStringRight($"{currentGun.GetName()}: {currentGun.CanFire()}");
    }

    public Gun GetCurrentGun()
    {
        return currentGun;
    }

    private void SwapGun(bool forward)
    {
        currentGunIndex += forward ? 1 : -1;
        if (currentGunIndex < 0)
            currentGunIndex += guns.Count;

        currentGunIndex %= guns.Count;
    
        currentGun = guns[currentGunIndex];
    }

    public override void _Draw()
    {
        Vector2 aimDirection = GetAimDirection();
        currentGun.DrawPreview(this, aimDirection);
    }

    /// <summary>
    /// Returns normalized aim direction
    /// </summary>
    private Vector2 GetAimDirection()
    {
        // TODO: Cache this, so it's not running multiple times a frame
        return (GetGlobalMousePosition() - Position).Normalized();
    }

    // Whenever player collides with bottom or an enemy
    private void OnBounce()
    {
        // For now, just reload on touch
        currentGun.Reload();
    }

    private void RunPhysics(float delta)
    {
        preframe = Position;

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

        if (testPosition.x > field.rightBound - playerRadius)
        {
            Logger.Log("Hit Right", LogLevel.info, LogChannel.Movement);

            // Find cross delta
            float dist = field.rightBound - playerRadius - Position.x;

            preDelta = Utils.FirstPositiveQuadratic(acceleration.x / 2, velocity.x, -dist);
            collisionState = 1;
        }
        else if (testPosition.x < field.leftBound + playerRadius)
        {
            Logger.Log("Hit Left", LogLevel.info, LogChannel.Movement);

            // Find cross delta
            float dist = field.leftBound + playerRadius - Position.x;

            preDelta = Utils.FirstPositiveQuadratic(acceleration.x / 2, velocity.x, -dist);
            collisionState = 1;
        }
        
        if (testPosition.y > field.bottomBound - playerRadius)
        {
            Logger.Log("Hit Bottom", LogLevel.info, LogChannel.Movement);

            // Find cross delta
            float dist = field.bottomBound - playerRadius - Position.y;
            // (vel.y + acc.y * alpha / 2) * alpha = dist
            // alpha^2 * acc.y / 2 + vel.y * alpha - dist = 0
            float newDelta = Utils.FirstPositiveQuadratic(acceleration.y / 2, velocity.y, -dist);
            if (newDelta < preDelta)
            {
                preDelta = newDelta;
                collisionState = 2;
            }
        }
        else if (testPosition.y < field.topBound + playerRadius)
        {
            Logger.Log("Hit Top", LogLevel.info, LogChannel.Movement);

            // Find cross delta
            float dist = field.topBound + playerRadius - Position.y;
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
            
            OnBounce();

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

    // Collisions with enemies don't have to be as exact
    private void RunCollisions(float delta)
    {
        hitTimer = Mathf.Max(0, hitTimer - delta);

        if (hitTimer <= 0)
        {
            (bool hit, HitboxOwner hitboxOwner) = CollisionManager.GetContainsHit(
                Position, HitboxOwnerType.Enemy
            );

            if (hit && hitboxOwner is Enemy hitEnemy)
            {
                hitTimer = InvulTimer;

                // Temp bounce code; uses preframe to prevent going too deep into hitbox TODO: Actuall collision math? Imagine
                Vector2 normal = hitboxOwner.GetHitbox().GetNormal(preframe);
                Vector2 projection = velocity.Dot(normal) * normal;
                velocity -= 2 * projection;

                // Maintaining a knockback speed
                float currentSpeed = velocity.Length();
                if (currentSpeed < MinHitSpeed)
                {
                    if (Mathf.IsZeroApprox(currentSpeed))
                    {
                        velocity = normal * MinHitSpeed;
                    }
                    else
                    {
                        velocity = velocity / currentSpeed * MinHitSpeed;
                    }
                }

                int damage = hitEnemy.DealDamage(this);

                // TODO: Move this into OnHit() function
                health -= damage;
                GameManager.GetUI().UpdateHealth(health / 3.0f);

                OnBounce();
            }
        }
    }

    private void RunGhosting(float delta)
    {
        Vector2 movementInput = new Vector2(Input.GetAxis("key_left", "key_right"),
            Input.GetAxis("key_up", "key_down"));
    
        movementInput = movementInput.Normalized();

        Position += movementInput * delta * 250;
    }
}