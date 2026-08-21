using Godot;
using System;

public class Pistol : Gun
{
    public Pistol()
    {
        remainingAmmo = 6;
    }
    public override string GetName() { return "Pistol"; }

    public override void Fire( Player firingPlayer, Vector2 aimDirection )
    {
        firingPlayer.ImpulseVelocity(aimDirection * -250);

        remainingAmmo -= 1;
        reloadTime = 0.75f;
        if (remainingAmmo <= 0)
        {
            
        }

        // Deal damage
        (float hitDist, HitboxOwner hitOwner) = CollisionManager.GetFirstHitboxRayhit(
            firingPlayer.Position, aimDirection, HitboxOwnerType.Enemy
        );
        if (hitOwner is Enemy hitEnemy)
        {
            hitEnemy.TakeDamage(1);
        }
    }
}