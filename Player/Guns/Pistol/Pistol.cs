using Godot;
using System;

public class Pistol : Gun
{
    public Pistol()
    {
        remainingAmmo = 6;
    }
    public override void Fire( Player firingPlayer, Vector2 aimDirection )
    {
        firingPlayer.ImpulseVelocity(aimDirection * -250);

        remainingAmmo -= 1;
        reloadTime = 0.75f;
        if (remainingAmmo <= 0)
        {
            
        }
    }

    public override void TickGun(Player firingPlayer, Vector2 aimDirection, FakeGun fakeGun, float delta)
    {
        base.TickGun(firingPlayer, aimDirection, fakeGun, delta);

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
}