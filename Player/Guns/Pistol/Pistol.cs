using Godot;
using System;

public class Pistol : Gun
{
    public Pistol()
    {
        remainingAmmo = 6;
    }
    public override string GetName() { return "Pistol"; }

    public override void Fire( Player firingPlayer, Vector2 aimDirection, float delta )
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

    public override void Reload()
    {
        remainingAmmo = Mathf.Min(remainingAmmo + 2, 6);
    }

    public override void DisplayAmmo(Control owner, Vector2 centerPosition)
    {
        for (int i = 0; i < remainingAmmo; i++)
        {
            float angle = i * Mathf.Pi / 3;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 80;

            owner.DrawRect(new Rect2(centerPosition + offset, new Vector2(40, 40)), Constants.colorDark);
        }
    }
}