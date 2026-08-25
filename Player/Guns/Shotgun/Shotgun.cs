using Godot;

public class Shotgun : Gun
{
    private int shotCount = 4;
    private float shotSpacing = Mathf.Pi / 24;

    public Shotgun()
    {
        remainingAmmo = 2;
    }

    public override string GetName() { return "Shotgun"; }

    public override void Fire(Player firingPlayer, Vector2 aimDirection, float delta)
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
        Vector2[] returnVector = new Vector2[shotCount];
        float startAngle = -shotSpacing * (
            (shotCount % 2 == 1) ? (shotCount - 1) / 2 : shotCount / 2.0f);

        for (int i = 0; i < shotCount; i++)
        {
            returnVector[i] = aimDirection.Rotated(startAngle + i * shotSpacing);
        }

        return returnVector;
    }

    public override void Reload()
    {
        remainingAmmo = Mathf.Min(remainingAmmo + 1, 2);
    }

    public override void DisplayAmmo(Control owner, Vector2 centerPosition)
    {
        for (int i = 0; i < remainingAmmo; i++)
        {
            float angle = i * Mathf.Pi;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 30;

            owner.DrawRect(new Rect2(centerPosition + offset, new Vector2(40, 80)), Constants.colorDark);
        }
    }
}