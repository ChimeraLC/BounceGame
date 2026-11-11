using System;
using System.Security.Cryptography.X509Certificates;
using Godot;

public abstract class Enemy : Node2D, HitboxOwner
{
    // In play
    private bool isAlive = true;
    private bool inPlay = true;
    protected float health = 5;

    // Leaving play
    float hurtTimer = 0;
    public int xpWorth = 0; // Set through enemyManager

    EnemyManager enemyManager;

    protected static Random rand = new Random();

    public virtual void Tick(float delta)
    {
        // Generic !isAlive -> !inPlay setup
        TickHurt(delta);

        TickMain(delta);
        if (Player.Instance != null)
            TickMainPlayer(delta);

        TickAnimation(delta);
    }

    /// <summary> Only runs in ticks where Player.Instance is valid </summary>
    protected virtual void TickMainPlayer(float delta) {}
    /// <summary> Always runs every frame, regardless of if player is active or not </summary>
    protected virtual void TickMain(float delta) {}
    protected virtual void TickAnimation(float delta) {}

    protected virtual void TickHurt(float delta)
    {
        hurtTimer = Mathf.Max(0, hurtTimer - delta);
        if (!isAlive && hurtTimer <= 0)
        {
            inPlay = false;
        }
    }

    public override void _Ready()
    {
        isAlive = true;
        inPlay = true;

        // TODO: Should be handled by enemy manager spawning code
        enemyManager = GameManager.Instance.enemyManager;
        enemyManager.RegisterEnemy(this);        
    }

    public bool InPlay() { return inPlay; }

    // Damage
    public virtual void TakeDamage( float damage, DamageType damageType = DamageType.Ballistic,
        bool continuousDamage = false)
    {
        health -= damage;
        if (health <= 0)
        {
            isAlive = false;
            hurtTimer = 0.2f;
            OnDeath();
        }
        else if (!continuousDamage)
            hurtTimer = 0.2f;
    }

    public virtual bool IsAlive() {return isAlive;}

    protected virtual void Suicide()
    {
        if (isAlive)
            TakeDamage(1000);
    }

    // Called when a player is hit by this
    public virtual int DealContactDamage( Player player ) { return 1; }

    // Hitbox Owner interface
    public abstract void UpdateHitbox();
    public abstract Hitbox GetHitbox();
    public HitboxOwnerType GetHitboxOwnerType() { return HitboxOwnerType.Enemy;}

    // Spawning behavior

    // Gets position enemy should be placed at by enemy manager
    public virtual Vector2 GetSpawnpoint() { return GlobalPosition; }

    // Private stuff?
    protected virtual void OnDeath()
    {
        if (Player.Instance != null)
            Player.Instance.AddXP(xpWorth);
    
        CollisionManager.RemoveHitbox(GetHitbox());
    }
}