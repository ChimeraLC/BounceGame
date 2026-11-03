using Godot;
using System;

public class Field : Node2D
{
    public float leftBound { get; private set; }
    public float rightBound { get; private set; }
    public float bottomBound { get; private set; }
    public float topBound { get; private set; }
    public override void _Ready()
    { 
        leftBound = Consts.FieldLeft;
        rightBound = Consts.FieldRight;
        bottomBound = Consts.FieldBottom;
        topBound = Consts.FieldTop;

        // Set regions
        UpdateBounds();

        GameManager.RegisterFieldInstance(this);

        // Create collision obstacles that bound the playfield
        CollisionManager.AddCollisionObstacle(new VertAxisCollision(leftBound));
        CollisionManager.AddCollisionObstacle(new VertAxisCollision(rightBound));
        CollisionManager.AddCollisionObstacle(new HorAxisCollision(topBound));
        CollisionManager.AddCollisionObstacle(new HorAxisCollision(bottomBound));
    }

    private void UpdateBounds()
    {
        // TODO: Resize polygon bounds to match bounds
    }

}