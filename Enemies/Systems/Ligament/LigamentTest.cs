using Godot;
using System;

public class LigamentTest : Node2D
{
    [Export]
    float length1 = 64;
    [Export]
    float length2 = 64;
    [Export]
    bool bendForwards = true;
    private Polygon2D ligament1;
    private Polygon2D ligament2;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        ligament1 = GetNode<Polygon2D>("Lig1");
        ligament2 = GetNode<Polygon2D>("Lig2");

        ligament1.Polygon = new Vector2[]{new Vector2(length1 + 5, 5), new Vector2(length1 + 5, -5), new Vector2(-5, -5), new Vector2(-5, 5)};
        ligament2.Polygon = new Vector2[]{new Vector2(length2 + 5, 5), new Vector2(length2 + 5, -5), new Vector2(-5, -5), new Vector2(-5, 5)};
    }
    public override void _Process(float delta)
    {
        (float angle1, float angle2, Vector2 position) = Ligament.GetPositions(Position, GetGlobalMousePosition(),
            length1, length2, bendForwards);
        ligament2.GlobalPosition = position;
        ligament1.Rotation = angle1;
        ligament2.Rotation = angle2;
    }
}
