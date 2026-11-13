using Godot;
using System;
using System.Collections.Generic;
using System.Data.Odbc;

public class CrawlerLeg : Node2D
{
    private Node2D ligament1;
    private Node2D ligament2;
    
    private Vector2 currentPosition;
    public Vector2 goalPosition {get; private set;}
    private const float moveSpeed = 200;
    private const float legLength = 64;
    bool reached = true;
    bool clockwise;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        ligament1 = GetNode<Node2D>("Lig1");
        ligament2 = GetNode<Node2D>("Lig2");

        currentPosition = GlobalPosition;
        clockwise = Position.x >= 0;
    }

    public void Tick(float delta, bool moving)
    {
        // Move legs
        if (moving || !reached)
        {
            (float angle1, float angle2, Vector2 position) = Ligament.GetPositions(GlobalPosition, currentPosition,
                legLength, legLength, clockwise);
            ligament2.GlobalPosition = position;
            ligament1.Rotation = angle1;
            ligament2.Rotation = angle2;
        }

        if (!reached)
        {
            (currentPosition, reached) = Utils.MoveTowards(currentPosition, goalPosition, delta * moveSpeed);
        }
    }

    public void SetGoal(Vector2 newGoal)
    {
        reached = false;
        goalPosition = newGoal;
    }
}
