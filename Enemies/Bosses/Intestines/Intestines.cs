using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography;

public class Intestines : Boss
{

    Random rand = new Random();

    // Tracking hitbox set
    CompoundHitbox compoundHitbox;
    HashSet<Hitbox> hitboxSet;


    // Tracking all intestine bodies
    PackedScene bodyScene = GD.Load<PackedScene>("res://Enemies/Bosses/Intestines/IntestinesBody.tscn");
    List<IntestinesBody> bodies = new List<IntestinesBody>();

    // Current position along 'track'
    float bodyOffset = 0;
    float travelSpeed = 300;
    float endOffset; // offset at which it should generate a new path

    Vector2 offsetCenter = new Vector2(500, 312);
    float radius = 150;
    int direction = 1;

    // Wait time between tunnels
    const int waitLow = 3;
    const int waitHigh = 5;
    float waitTimer = 0;
    public Node2D warnIndicator;

    public override void _Ready()
    {
        base._Ready();

        warnIndicator = GetNode<Node2D>("WarnIndicator");

        hitboxSet = new HashSet<Hitbox>();
        compoundHitbox = new CompoundHitbox(this, ref hitboxSet);

        CollisionManager.AddHitbox(compoundHitbox);

        // Create bodies
        for (int i = 0; i < 10; i++)
        {
            CreateBody();
        }
        
        GenBodyPosition();
        waitTimer = 5;
    }

    private void CreateBody()
    {
        IntestinesBody newBody = bodyScene.Instance<IntestinesBody>();
        AddChild(newBody);
        newBody.Initialize(this);

        newBody.index = bodies.Count;
        bodies.Add(newBody);
        hitboxSet.Add(newBody.GetHitbox());
    }

    public override void Tick(float delta)
    {
#if DEBUG
        if (Input.IsActionPressed("debug_progress"))
        {
            bodyOffset += delta * 400;
            if (bodyOffset > endOffset)
                GenBodyPosition();
        }
        if (Input.IsActionPressed("debug_regress"))
        {
            bodyOffset -= delta * 400;
        }
        if (Input.IsActionJustPressed("debug_action"))
        {
            GenBodyPosition();
            //DestroyBody(2);
        }
#endif   

        warnIndicator.Visible = waitTimer > 0;

        if (waitTimer < 0)
        {
            bodyOffset += delta * travelSpeed;
            if (bodyOffset > endOffset)
            {
                GenBodyPosition();
                waitTimer = (float) rand.NextDouble() * (waitHigh - waitLow) + waitLow;
            }
        }
        else
        {
            waitTimer -= delta;
        }

        TickBodies(delta);
    }

    private float bodySpacing = 56;

    private void TickBodies(float delta)
    {
        for (int i = 0; i < bodies.Count; i++)
        {
            IntestinesBody currentBody = bodies[i];
            currentBody.Position = GetBodyPosition(bodyOffset - bodySpacing * i - currentBody.offsetBump);

            currentBody.offsetBump = Mathf.Max(0, currentBody.offsetBump - delta * 100);
        }
    }

    private void GenBodyPosition()
    {
        // Select two points along different sides, and then the arc connecting them
        int skipped = rand.Next(3);

        int side1 = skipped == 0 ? 1 : 0;
        int side2 = skipped < 2 ? 2 : 1;

        Vector2 point1 = GenSidePoint(side1);
        Vector2 point2 = GenSidePoint(side2);

        // Preventing small corner paths
        float middle = (Constants.FieldLeft + Constants.FieldRight) / 2;
        if (skipped == 2 && point2.x < middle - 75)
            point2.x = 2 * middle - point2.x; // Mirror to longer path

        if (skipped == 0 && point1.x > middle + 75)
            point1.x = 2 * middle - point1.x;

        // Find valid center
        Vector2 midPoint = (point1 + point2) / 2;
        Vector2 perp = point1 - midPoint;
        perp = new Vector2(perp.y, -perp.x).Normalized();
        if (perp.y < 0)
            perp = - perp;

        offsetCenter = midPoint + perp * 500;
        radius = (point1 - offsetCenter).Length();

        // Choose direction
        direction = rand.Next(2) * 2 - 1; // 1 or -1
        
        // Place offset at the right point
        Vector2 startPoint = direction == 1 ? point1 : point2;
        int startSide = direction == 1 ? side1 : side2;
        Vector2 endPoint = direction == 1 ? point2 : point1;
        Vector2 offset = startPoint - offsetCenter;

        // tan(offset / radius * direction) = offset.y/x
        bodyOffset = Mathf.Atan2(offset.y, offset.x) * radius / direction;

        offset = endPoint - offsetCenter;
        endOffset = Mathf.Atan2(offset.y, offset.x) * radius / direction + bodySpacing * bodies.Count;

        // Place warning indicator
        warnIndicator.Position = startPoint;
        switch (startSide)
        {
            case 0:
            warnIndicator.Position += 100 * Vector2.Right;
                break;
            case 1:
            warnIndicator.Position += 100 * Vector2.Up;
                break;
            case 2:
            warnIndicator.Position += 100 * Vector2.Left;
                break;
        }
    }

    private const int padding = 150;

    private Vector2 GenSidePoint(int side)
    {
        switch (side)
        {
            case 0:
                return new Vector2(Constants.FieldLeft - padding / 3,
                    rand.Next(Constants.FieldTop + padding * 2, Constants.FieldBottom - padding));
            case 1:
                return new Vector2(rand.Next(Constants.FieldLeft + padding, Constants.FieldRight - padding),
                    Constants.FieldBottom + padding / 3);
            case 2:
                return new Vector2(Constants.FieldRight + padding / 3,
                    rand.Next(Constants.FieldTop + padding, Constants.FieldBottom - padding));
        }

        return Vector2.Zero;
    }

    // Returns the body position of a given offset
    private Vector2 GetBodyPosition(float offset)
    {
        return offsetCenter + new Vector2(
            Mathf.Cos(offset / radius * direction) * radius,
            Mathf.Sin(offset / radius * direction) * radius
        );
    }

    public void DestroyBody(int index)
    {
        if (bodies.Count > index)
        {
            // Remove body, and remove body from hitbox set
            IntestinesBody removedBody = bodies[index];
            bodies.RemoveAt(index);

            if (!hitboxSet.Remove(removedBody.GetHitbox()))
            {
                Logger.Log($"Failed to remove hitbox from intestine body {removedBody}", LogLevel.warn,
                    LogChannel.BossBehavior);
            }

            // Hide body
            removedBody.Position = new Vector2(-100, -100);

            // Add bumps for remaining bodies
            for (int i = index; i < bodies.Count; i++)
            {
                bodies[i].offsetBump += bodySpacing;
                bodies[i].index -= 1;
            }
        }
    }

    public override void UpdateHitbox()
    {
        foreach (IntestinesBody body in bodies)
        {
            body.UpdateHitbox();
        }
    }

    public override Hitbox GetHitbox()
    {
        return compoundHitbox;
    }

    protected override void OnDeath()
    {
        CollisionManager.RemoveHitbox(compoundHitbox);
    }
}