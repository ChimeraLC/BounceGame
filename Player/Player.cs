using Godot;
using System;
using System.Collections.Generic;

public class Player : Node2D, HitboxOwner
{
    public static Player Instance {get; private set;}
    // Health
    private const int MaxHealth = 3;
    private int health = MaxHealth;
    private const float InvulTimer = 3;
    private float hitTimer = 0;
    private const float MinHitSpeed = 400;

    // Leveling up
    private int levelXp = 8; // This increases as time goes on
    private int currentXp = 0;

    // Movement
    private const int playerRadius = 12;
    private const float Bounciness = 0.75f;
    private const float MinVertSpeed = 300;
    private const float MinSpeed = 30;
    private const float HorControl = 100.0f;
    private Vector2 velocity;
    private Vector2 preframe; // Position at start of frame

    private RectHitbox hitbox;

    // Guns
    private List<Gun> guns = new List<Gun>();
    private Gun currentGun;
    private FakeGun fakeGun;
    private int currentGunIndex;

    // Upgrades
    public UpgradeTracker upgradeTracker;


    public override void _Ready()
    {
        Instance = this;

        velocity = Vector2.Zero;

        hitbox = new RectHitbox(this, GlobalPosition, new Vector2(12, 12));

        // Setup guns
        fakeGun = GetNode<FakeGun>("FakeGun");

        currentGunIndex = 0;

        guns.Add(new Pistol());
        // guns.Add(new Shotgun());
        // guns.Add(new Laser());

        currentGun = guns[0];

        // Setup upgrades
        upgradeTracker = new UpgradeTracker();

        // Reset UI elements
        OnHealthUpdate();
        OnXPUpdate();
    }

    public void Tick(float delta)
    {
#if DEBUG
        if (DebugManager.Ghosting)
        {
            TickGhosting(delta);
        }
        else
#endif
        {
            TickPhysics(delta);
            TickCollisions(delta);
        }
        
        TickWeapon(delta);
        TickAnimation(delta);

        Update();
    }

    public int GetHP() { return health; }

    public void UpdateHitbox() { hitbox.center = GlobalPosition; }
    public Hitbox GetHitbox() { return hitbox; }
    public HitboxOwnerType GetHitboxOwnerType() { return HitboxOwnerType.Player; }

// MARK: Weapon
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

    // Returns damage change (i.e. with buffs, debuffs)
    public float GetDamageModifier()
    {
        float baseModifier = 1;
        baseModifier += upgradeTracker.GetUpgradeCount(Upgrade.DamageUp) * 0.25f; // Each damageup gives +25% damage

        return baseModifier;
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

// MARK: Physics
    /// <summary> Apply an impulse to player movement /// </summary>
    public void ImpulseVelocity(Vector2 impulse)
    {
        velocity += impulse;
    }
    // Whenever player collides with bottom or an enemy
    private void OnBounce()
    {
        // For now, just reload on touch
        currentGun.Reload();
    }


    private void TickPhysics(float delta)
    {
        preframe = Position;

        if (delta <= 0)
        {
            Logger.Log("RunPhysics recieved a negative delta", LogLevel.error);
            return;
        }

        Field field = GameManager.GetField(); // Getting each frame is probably slow

        // Acceleration should be constant during a single runPhysics call
        Vector2 acceleration = Vector2.Down * Consts.Gravity;

        // Allowing for jet controls    
        if (upgradeTracker.HasUpgrade(Upgrade.JetControls))
            acceleration += HorControl * Vector2.Right * Input.GetAxis("key_left", "key_right");
    
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
        
        TickPhysics(delta - preDelta);
    }

    // Collision timings with enemies don't have to be as exact
    private void TickCollisions(float delta)
    {
        hitTimer = Mathf.Max(0, hitTimer - delta);

        if (hitTimer <= 0)
        {
            // (bool hit, HitboxOwner hitboxOwner) = CollisionManager.GetContainsHit(
            //     Position, HitboxOwnerType.Enemy
            // );

            // TODO: Dealing with hitting multiple at once?
            (bool hit, HitboxOwner hitboxOwner) = CollisionManager.GetOverlapsHit(hitbox, HitboxOwnerType.Enemy);

            if (hit && hitboxOwner is Enemy hitEnemy)
            {
                // Check if it does contact damage
                int damage = hitEnemy.DealContactDamage(this);
                if (damage >= 0)
                {
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

                    TakeDamage(damage);
                    
                    OnBounce();
                }
            }
        }
    }

// MARK: Health and xp
    public void TakeDamage(int damage)
    {
        if (hitTimer <= 0)
        {
            hitTimer = InvulTimer;
            health -= damage;
            OnHealthUpdate();
        }
    }

    public void AddXP(int xpAmount)
    {
        currentXp += xpAmount;
        if (currentXp >= levelXp)
        {
            currentXp -= levelXp; // Check case when multiple levels overlap
            levelXp += 4;

            // Spawn an upgrade orb
            EnemyManager.RequestEnemy(EnemyType.UpgradeOrb);
        }
        OnXPUpdate();
    }

    private void OnHealthUpdate()
    {
        GameManager.GetUI().UpdateHealth(health / (float) MaxHealth);
    }

    private void OnXPUpdate()
    {
        GameManager.GetUI().UpdateXP(currentXp / (float) levelXp);
    }

// MARK: Animation
    private void TickAnimation(float delta)
    {
        if (hitTimer > 0)
            Modulate = hitTimer % 0.4f > 0.2f ? Consts.colorInvis : Colors.White;
        else
            Modulate = Colors.White;
    }

// MARK: Other
    public float GetTimeDilation()
    {
        if (upgradeTracker.HasUpgrade(Upgrade.TimeSlow))
        {
            if (Input.IsActionPressed("key_rmb"))
            {
                return 0.5f;
            }
        }
        return 1.0f;
    }
    private void TickGhosting(float delta)
    {
        Vector2 movementInput = new Vector2(Input.GetAxis("key_left", "key_right"),
            Input.GetAxis("key_up", "key_down"));
    
        movementInput = movementInput.Normalized();

        Position += movementInput * delta * 250;
    }
}