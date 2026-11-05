using Godot;
using System;

public class TumorShooterEnemy : Enemy
{
    CircleHitbox hitbox;
    Node2D tumorBody;
    // Controls the projectile
    Node2D projectileBody;

    [Export]
    public PackedScene projectileScene;

    protected int minimumSize = 20;
    protected int maximumSize = 70;
    protected int chosenSize;
    protected Vector2 projectileLocation;
    protected Vector2 projectileDirection;

    protected float spawnTimer = 2;
    protected const float spawnSpacing = 8;

    public override void _Ready()
    {
        base._Ready();

        chosenSize = rand.Next() % (maximumSize - minimumSize) + minimumSize;    
        hitbox = new CircleHitbox(this, Position, chosenSize);

        tumorBody = GetNode<Node2D>("TumorBody");
        tumorBody.Scale = new Vector2(chosenSize, chosenSize);

        // TODO: Maybe vary health with size?
        health = 2;
    }

    protected override void TickMainPlayer(float delta)
    {
        spawnTimer -= delta;
        if (spawnTimer <= 0)
        {
            spawnTimer += spawnSpacing;
            Node2D projectile = projectileScene.Instance<Node2D>();
            projectile.GlobalPosition = GlobalPosition;
            GameManager.WorldRoot.AddChild(projectile);
        }
    }

    
    public override Vector2 GetSpawnpoint()
    {
        return Utils.GenerateRandomEdgepoint(48, -5, FieldEdge.Left | FieldEdge.Right | FieldEdge.Bottom).Item1;
    }

    public override void UpdateHitbox() {} // Doesn't move

    public override Hitbox GetHitbox()
    {
        return hitbox;
    }

    protected override void OnDeath()
    {
        CollisionManager.RemoveHitbox(hitbox);
    }
}
