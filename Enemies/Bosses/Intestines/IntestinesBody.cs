using Godot;

public class IntestinesBody : Enemy
{
    private CircleHitbox hitbox;

    public Intestines owner;
    public int index;
    // When the body in front of them is destroyed, don't immediately close the gap
    public float offsetBump;
    public override void _Ready()
    {
        base._Ready();
    }

    public void Initialize(Intestines owner)
    {
        this.owner = owner;
        hitbox = new CircleHitbox(this, Position, 32);
    }

    public override void UpdateHitbox()
    {
        hitbox.center = Position;
    }

    public override Hitbox GetHitbox()
    {
        return hitbox;
    }

    public override void TakeDamage(float damage, DamageType damageType = DamageType.Ballistic, bool continuousDamage = false)
    {
        owner.DestroyBody(index);
    }

    protected override void OnDeath()
    {
        // Hitbox is contained in main intestines compound hitbox
    }
}