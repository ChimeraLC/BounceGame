using Godot;
using System;

public enum DamageType
{
    Ballistic
}

public abstract class Gun
{
    protected int remainingAmmo;
    protected float reloadTime;
    public virtual bool CanFire() {return reloadTime <= 0;}

    public abstract void Fire( Player firingPlayer, Vector2 aimDirection );

    public virtual void TickGun( Player firingPlayer, Vector2 aimDirection, FakeGun fakeGun, float delta)
    {
        reloadTime = Mathf.Max(0, reloadTime - delta);
    }
}
