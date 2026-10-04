using Godot;

public class Laser : Gun
{
    float remainingCharge;
    float minCharge = 0.5f;
    float maxCharge = 5;
    bool isFiring = false;
    public Laser()
    {
        remainingCharge = maxCharge;
        heldWeapon = true;
    }

    public override string GetName() { return "Laser"; }

    // public override void TickGunPassive(float delta)
    // {
    //     base.TickGunPassive(delta);
        
    //     remainingCharge = Mathf.Min(remainingCharge + delta, maxCharge);
    // }

    // public override void TickGun(Player firingPlayer, Vector2 aimDirection, FakeGun fakeGun, float delta)
    // {
    //     base.TickGun(firingPlayer, aimDirection, fakeGun, delta);
             
    //     if (!isFiring)
    //         remainingCharge = Mathf.Min(remainingCharge + delta, maxCharge);
    // }

    public override void Reload()
    {
        remainingCharge = Mathf.Min(remainingCharge + maxCharge / 2, maxCharge);
    }
    public override bool CanFire()
    {
        return remainingCharge > 0; //(isFiring && remainingCharge > 0) || remainingCharge > minCharge;
    }

    public override void Fire(Player firingPlayer, Vector2 aimDirection, float delta)
    {
        isFiring = true;

        
        firingPlayer.ImpulseVelocity(aimDirection * -250 * delta);

        remainingCharge = Mathf.Max(0, remainingCharge - delta);
        
        (float hitDist, HitboxOwner hitOwner) = CollisionManager.GetFirstHitboxRayhit(
            firingPlayer.Position, aimDirection, HitboxOwnerType.Enemy
        );
        if (hitOwner is Enemy hitEnemy)
        {
            hitEnemy.TakeDamage(1 * delta);
        }
    }
    public override void ReleaseFire()
    {
        isFiring = false;
    }

    public override void DisplayAmmo(Control owner, Vector2 centerPosition)
    {
        owner.DrawRect(new Rect2(centerPosition, new Vector2(60, 80 * remainingCharge / maxCharge)), Consts.colorDark);
    }
}