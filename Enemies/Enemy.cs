using System.Security.Cryptography.X509Certificates;
using Godot;

public abstract class Enemy : Node2D, HitboxOwner
{
    // In play
    private bool isAlive = true;
    private bool inPlay = true;
    private float health = 5;

    // Leaving play
    float hurtTimer = 0;

    EnemyManager enemyManager;

    public virtual void Tick(float delta)
    {
        // Generic !isAlive -> !inPlay setup
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
        GameManager.Instance.enemyManager.RegisterEnemy(this);        
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

    // Called when a player is hit by this
    public virtual int DealDamage( Player player ) { return 1; }

    // Hitbox Owner interface
    public abstract void UpdateHitbox();
    public abstract Hitbox GetHitbox();
    public HitboxOwnerType GetHitboxOwnerType() { return HitboxOwnerType.Enemy;}

    // Private stuff?
    protected abstract void OnDeath();
}