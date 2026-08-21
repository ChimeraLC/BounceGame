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

    public virtual string GetName() { return "Invalid"; }

    public virtual void TickGun( Player firingPlayer, Vector2 aimDirection, FakeGun fakeGun, float delta)
    {
        reloadTime = Mathf.Max(0, reloadTime - delta);

        if (aimDirection.x > 0)
        {
            fakeGun.Scale = Constants.scaleNormal;
            fakeGun.Rotation = Mathf.Atan(aimDirection.y / aimDirection.x);
        }
        else
        {
            fakeGun.Scale = Constants.scaleFlipX;
            fakeGun.Rotation = Mathf.Atan(aimDirection.y / aimDirection.x);
        }
    }

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

    protected virtual void DrawPreviewLine( Node2D owner, Vector2 position, Vector2 aimDirection)
    {
        (float firstHit, _) = CollisionManager.GetFirstCollisionRayhit(position, aimDirection);
        (float secondHit, _) = CollisionManager.GetFirstHitboxRayhit(position, aimDirection, HitboxOwnerType.Enemy);
        if (secondHit >= 0)
            firstHit = Mathf.Min(firstHit, secondHit);
        owner.DrawLine(Vector2.Zero, aimDirection * firstHit, Colors.White, 10);        
    }
}
