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
        leftBound = 300;
        rightBound = 724;
        bottomBound = 550;
        topBound = 0;

        GameManager.RegisterFieldInstance(this);

        // Create collision obstacles
        CollisionManager.AddCollisionObstacle(new VertAxisCollision(leftBound));
        CollisionManager.AddCollisionObstacle(new VertAxisCollision(rightBound));
        CollisionManager.AddCollisionObstacle(new HorAxisCollision(topBound));
        CollisionManager.AddCollisionObstacle(new HorAxisCollision(bottomBound));
    }

}