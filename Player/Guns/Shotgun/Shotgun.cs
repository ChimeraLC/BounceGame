using Godot;

public class Shotgun : Gun
{
    public Shotgun()
    {
        remainingAmmo = 2;
    }

    public override string GetName() { return "Shotgun"; }

    public override void Fire(Player firingPlayer, Vector2 aimDirection)
    {
        firingPlayer.ImpulseVelocity(aimDirection * -400);

        remainingAmmo -= 1;
        reloadTime = 1.5f;
        if (remainingAmmo <= 0)
        {
            
        }

        // Deal damage
        foreach (Vector2 direction in GetShotgunDirections(aimDirection))
        {
            (_, HitboxOwner hitOwner) = CollisionManager.GetFirstHitboxRayhit(
                firingPlayer.Position, direction, HitboxOwnerType.Enemy
            );
            if (hitOwner is Enemy hitEnemy)
            {
                hitEnemy.TakeDamage(1);
            }
        }
    }

    public override void DrawPreview( Node2D owner, Vector2 aimDirection )
    {
        if (CanFire())
        {
            foreach (Vector2 direction in GetShotgunDirections(aimDirection))
            {
                DrawPreviewLine( owner, owner.Position, direction);
            }
        }
    }

    private Vector2[] GetShotgunDirections( Vector2 aimDirection )
    {
        return new Vector2[]
        {
            aimDirection,
            aimDirection.Rotated(Mathf.Pi / 12),
            aimDirection.Rotated(-Mathf.Pi / 12)
        };
    }
}