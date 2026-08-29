using Godot;
using System;
using System.Collections.Generic;

public class Intestines : Boss
{

    // Tracking hitbox set
    CompoundHitbox compoundHitbox;
    HashSet<Hitbox> hitboxSet;


    // Tracking all intestine bodies
    PackedScene bodyScene = GD.Load<PackedScene>("res://Enemies/Bosses/Intestines/IntestinesBody.tscn");
    List<IntestinesBody> bodies = new List<IntestinesBody>();

    // Current position along 'track'
    float bodyOffset = 0;

    public override void _Ready()
    {
        base._Ready();

        hitboxSet = new HashSet<Hitbox>();
        compoundHitbox = new CompoundHitbox(ref hitboxSet);

        CollisionManager.AddHitbox(compoundHitbox);

        // Create bodies
        for (int i = 0; i < 8; i++)
        {
            CreateBody();
        }
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
        }
        if (Input.IsActionPressed("debug_regress"))
        {
            bodyOffset -= delta * 400;
        }
        if (Input.IsActionJustPressed("debug_action"))
        {
            DestroyBody(2);
        }
#endif   

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

    // Returns the body position of a given offset
    private Vector2 GetBodyPosition(float offset)
    {
        float radius = 150;

        return new Vector2(
            512 + Mathf.Cos(offset / radius) * radius,
            300 + Mathf.Sin(offset / radius) * radius
        );
    }
    
    private void DestroyBody(int index)
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