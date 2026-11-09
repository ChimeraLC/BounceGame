using Godot;
using System;

// Orb that just gives upgrades when destroyed
public class UpgradeOrb : Enemy
{
    CircleHitbox hitbox;

    protected int minimumSize = 30;
    protected int maximumSize = 100;
    protected int chosenSize;

    public override void _Ready()
    {
        base._Ready();

        hitbox = new CircleHitbox(this, Position, 20);

        // TODO: Maybe vary health with size?
        health = 1;
    }

    public override Vector2 GetSpawnpoint()
    {
        return Utils.GenerateRandomEdgepoint(48, 10, FieldEdge.Top).Item1;
    }

    public override int DealContactDamage(Player player)
    {
        // This doesn't deal contact damage
        return 0;
    }


    public override void UpdateHitbox() {} // Doesn't move

    public override Hitbox GetHitbox()
    {
        return hitbox;
    }
    protected override void OnDeath()
    {
        base.OnDeath();

        // Shows upgrades when destroyed
        UpgradeManager.Instance.ShowUpgrades();
    }
}
