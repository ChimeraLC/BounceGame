using Godot;
using System;

// Basic class for nodes with a single static color
public class ColorNode : Polygon2D
{
    [Export]
    public ColorType colorType; 
    public override void _Ready()
    {
       Color = Constants.GetColor(colorType);
    }

//  public override void _Process(float delta)
//  {
//
//  }
}