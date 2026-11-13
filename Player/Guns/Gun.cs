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

    public bool heldWeapon {protected set; get; }
    public virtual bool CanFire() {return reloadTime <= 0 && remainingAmmo > 0;}

    public abstract void Fire( Player firingPlayer, Vector2 aimDirection, float delta );
    public virtual void ReleaseFire() {}

    public virtual string GetName() { return "Invalid"; }

    public virtual void TickGun( Player firingPlayer, Vector2 aimDirection, FakeGun fakeGun, float delta)
    {
        reloadTime = Mathf.Max(0, reloadTime - delta);

        if (aimDirection.x > 0)
        {
            fakeGun.Scale = Consts.scaleNormal;
            fakeGun.Rotation = Mathf.Atan(aimDirection.y / aimDirection.x);
        }
        else
        {
            fakeGun.Scale = Consts.scaleFlipX;
            fakeGun.Rotation = Mathf.Atan(aimDirection.y / aimDirection.x);
        }
    }

    public abstract void Reload();

    public virtual void TickGunPassive( float delta )
    {
        reloadTime = Mathf.Max(0, reloadTime - delta / 2);
    }

    public virtual void DrawPreview( Node2D owner, Vector2 aimDirection )
    {
        if (CanFire())
        {
            DrawPreviewLine( owner, owner.Position, aimDirection);
        }
    }

    public bool BulletBlocking(HitboxOwner owner)
    {
        if (owner is Enemy enemy)
            return enemy.blocksBullets;
        return true;
    }

    // Gets endpoint of fired shot
    protected virtual float GetShotEnd(Vector2 position, Vector2 aimDirection)
    {
        (float firstHit, _) = CollisionManager.GetFirstCollisionRayhit(position, aimDirection);
        (float secondHit, _) = CollisionManager.GetFirstHitboxRayhit(position, aimDirection, HitboxOwnerType.Enemy, BulletBlocking);
        if (secondHit >= 0)
            firstHit = Mathf.Min(firstHit, secondHit);

        return firstHit;
    }

    protected virtual void DrawPreviewLine( Node2D owner, Vector2 position, Vector2 aimDirection)
    {
        float shotEnd = GetShotEnd(position, aimDirection);
        owner.DrawLine(Vector2.Zero, aimDirection * shotEnd, Consts.colorLight, 4);        
    }

    public virtual void DisplayAmmo( Control owner, Vector2 centerPosition ) {}
}
