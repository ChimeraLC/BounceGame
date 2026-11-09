using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using Godot;

public enum HitboxOwnerType
{
    Enemy,
    Player,
    Any,
    Invalid
}

public interface HitboxOwner
{
    // Called before any hitbox calculations, so they aren't updated every frame
    void UpdateHitbox();
    Hitbox GetHitbox();
    HitboxOwnerType GetHitboxOwnerType();
}

// Handles normals in some edge cases
public interface CompoundHitboxOwner : HitboxOwner
{
    Vector2 GetNormal(Vector2 testPoint);
}

public abstract class Hitbox
{
    public int lastUpdatedHitboxFrame = -1;
    public bool enabled {get; private set;} = true;

    protected HitboxOwner hitboxOwner;
    /// <summary> Returns distance a given ray intersects this hitbox at. -1 if no hit </summary>
    public abstract (float, HitboxOwner) GetFirstRayHit(Vector2 start, Vector2 direction);
    /// <summary> Returns if the given spot lies within the hitbox </summary>
    public abstract (bool, HitboxOwner) GetContainsHit(Vector2 testPosition);
    /// <summary> Gets normal of hitbox at given point </summary>
    public abstract Vector2 GetNormal(Vector2 testPoint);
    /// <summary> Updates position of hitbox </summary>
    public void UpdateHitbox() { 
        if (lastUpdatedHitboxFrame != GameManager.CurrentFrame)
        {
            hitboxOwner.UpdateHitbox(); 
            lastUpdatedHitboxFrame = GameManager.CurrentFrame;
        }
    }
    public HitboxOwnerType GetHitboxOwnerType()
    {
        if (hitboxOwner == null)
        {
            Logger.Log($"Hitbox {this} is missing a hitbox owner", LogLevel.error);
            return HitboxOwnerType.Invalid;
        }

        return hitboxOwner.GetHitboxOwnerType();
    }

    public HitboxOwner GetHitboxOwner() { return hitboxOwner; }

    public void SetEnabled(bool isEnabled) { enabled = isEnabled; }

    public abstract void DrawHitbox( Control drawOwner );


    /// <summary> Returns if this hitbox overlaps the other given hibox </summary>
    public bool OverlapsHitbox(Hitbox otherHitbox) { return HitboxesOverlap(this, otherHitbox); }
    public static bool HitboxesOverlap(Hitbox hitbox1, Hitbox hitbox2)
    {
        // Make sure both hitboxes are active
        if (!hitbox1.enabled || !hitbox2.enabled)
            return false;
        
        // TODO: Cleanup; It's a bit messy
        // Compound cases
        if (hitbox1 is CompoundHitbox compoundHitbox)
        {
            foreach (Hitbox subHitbox in compoundHitbox.GetHitboxes())
            {
                if (HitboxesOverlap(subHitbox, hitbox2))
                    return true;
            }
            return false;
        }
        if (hitbox2 is CompoundHitbox compoundHitbox2)
        {
            foreach (Hitbox subHitbox in compoundHitbox2.GetHitboxes())
            {
                if (HitboxesOverlap(hitbox1, subHitbox))
                {
                    return true;
                }
            }
            return false;
        }
        
        if (hitbox1 is RectHitbox rectHitbox)
        {
            if (hitbox2 is RectHitbox rectHitbox2)
            {
                return RectRectOverlap(rectHitbox, rectHitbox2);
            }
            else if (hitbox2 is CircleHitbox circHitbox)
            {
                return RectCircOverlap(rectHitbox, circHitbox);
            }
        }
        else if (hitbox1 is CircleHitbox circHitbox)
        {
            if (hitbox2 is RectHitbox rectHitbox2)
            {
                return RectCircOverlap(rectHitbox2, circHitbox);
            }
            else if (hitbox2 is CircleHitbox circHitbox2)
            {
                return CircCircOverlap(circHitbox, circHitbox2);
            }
        }

        Logger.Log("Recieved unsuported hitbox types in HitboxesOverlap()", LogLevel.error);
        return false;
    }

    public static bool RectRectOverlap(RectHitbox hitbox1, RectHitbox hitbox2)
    {
        return Mathf.Abs(hitbox1.center.x - hitbox2.center.x) <= hitbox1.halfDimensions.x + hitbox2.halfDimensions.x
        && Mathf.Abs(hitbox1.center.y - hitbox2.center.y) <= hitbox1.halfDimensions.y + hitbox2.halfDimensions.y;
    }

    public static bool RectCircOverlap(RectHitbox hitbox1, CircleHitbox hitbox2)
    {
        Vector2 closest = new Vector2(
            Mathf.Min(hitbox1.center.x + hitbox1.halfDimensions.x, Mathf.Max(hitbox1.center.x - hitbox1.halfDimensions.x, hitbox2.center.x)),
            Mathf.Min(hitbox1.center.y + hitbox1.halfDimensions.y, Mathf.Max(hitbox1.center.y - hitbox1.halfDimensions.y, hitbox2.center.y))
        );

        return (closest - hitbox2.center).LengthSquared() <= hitbox2.radius * hitbox2.radius;
    }

    public static bool CircCircOverlap(CircleHitbox hitbox1, CircleHitbox hitbox2)
    {
        return (hitbox1.center - hitbox2.center).LengthSquared() 
            < (hitbox1.radius + hitbox2.radius) * (hitbox1.radius + hitbox2.radius);
    }
}

// TODO: Checks for if ray starts already within hitbox?

// Rectable hitbox defined by its center and width/height
public class RectHitbox : Hitbox
{
    public Vector2 center;
    public Vector2 halfDimensions;
    public RectHitbox(HitboxOwner inOwner, Vector2 inCenter, Vector2 inHalfDimensions, bool compound = false)
    {
        hitboxOwner = inOwner;
        center = inCenter;
        halfDimensions = inHalfDimensions;

        if (!compound)
            CollisionManager.AddHitbox(this);
    }

    public override (float, HitboxOwner) GetFirstRayHit(Vector2 start, Vector2 direction)
    {
        // Set center of ray to be at 0, 0
        Vector2 offsetCenter = center - start;

        // TODO: Could this ever 'slide' through a corner

        // Only need to check closer 2 sides
        if (!Mathf.IsZeroApprox(direction.x))
        {
            float xHit = offsetCenter.x + (direction.x > 0 ? -1 : 1) * halfDimensions.x;
            float hitTime = xHit / direction.x;
            // Valid hit that's within the bounds
            if (hitTime > 0 && Mathf.Abs(direction.y * hitTime - offsetCenter.y) <= halfDimensions.y)
                return (hitTime, hitboxOwner);
        }

        if (!Mathf.IsZeroApprox(direction.y))
        {
            float yHit = offsetCenter.y + (direction.y > 0 ? -1 : 1) * halfDimensions.y;
            float hitTime = yHit / direction.y;

            if (hitTime > 0 && Mathf.Abs(direction.x * hitTime - offsetCenter.x) <= halfDimensions.x)
                return (hitTime, hitboxOwner);
        }
        
        return (-1, hitboxOwner);
    }

    public override (bool, HitboxOwner) GetContainsHit(Vector2 testPosition)
    {
        Vector2 offsetCenter = testPosition - center;
        return (Mathf.Abs(offsetCenter.x) < halfDimensions.x && Mathf.Abs(offsetCenter.y) < halfDimensions.y,
            hitboxOwner);
    }

    public override Vector2 GetNormal(Vector2 testPoint)
    {
        Vector2 offset = testPoint - center;
        if (Mathf.Abs(offset.x) > Mathf.Abs(offset.y))
        {
            return Mathf.Sign(offset.x) * Vector2.Right;
        }
        else
        {
            return Mathf.Sign(offset.y) * Vector2.Down;
        }
    }

    public override void DrawHitbox(Control drawOwner)
    {
        drawOwner.DrawRect(new Rect2(center - halfDimensions, halfDimensions * 2), enabled ? Colors.Purple : Colors.LightBlue, false);
    }
}

// Circle hitbox defined by its center and radius
public class CircleHitbox : Hitbox
{
    public Vector2 center;
    public float radius;
    // Compound - if it's in a compound hitbox and shouldn't be individually registered
    public CircleHitbox(HitboxOwner inOwner, Vector2 inCenter, float inRadius, bool compound = false)
    {
        hitboxOwner = inOwner;
        center = inCenter;
        radius = inRadius;
    
        if (!compound)
            CollisionManager.AddHitbox(this);
    }

    public override (float, HitboxOwner) GetFirstRayHit(Vector2 start, Vector2 direction)
    {
        // Set center of ray to be at 0, 0
        Vector2 offsetCenter = center - start;

        // (tx - a)^2 + (ty - b)^2 = r^2
        // (x^2+y^2)t^2 - (2xa + 2yb)t + (a^2 + b^2 - r^2) = 0
        // This only works since direction is normalized
        return (Utils.PositiveQuadraticIfPossible(direction.x * direction.x + direction.y * direction.y,
            -2 * direction.x * offsetCenter.x - 2 * direction.y * offsetCenter.y,
            offsetCenter.x * offsetCenter.x + offsetCenter.y * offsetCenter.y - radius * radius), hitboxOwner);
    }

    public override (bool, HitboxOwner) GetContainsHit(Vector2 testPosition)
    {
        
        Vector2 offsetCenter = testPosition - center;
        // Initial check to avoid length queries
        if (Mathf.Abs(offsetCenter.x) < radius && Mathf.Abs(offsetCenter.y) < radius)
        {
            return (offsetCenter.LengthSquared() < radius * radius, hitboxOwner);
        }
        return (false, hitboxOwner);
    }
    public override Vector2 GetNormal(Vector2 testPoint)
    {
        return (testPoint - center).Normalized();
    }
    
    public override void DrawHitbox(Control drawOwner)
    {
        drawOwner.DrawArc(center, radius, 0, Mathf.Pi * 2, 32, enabled ? Colors.Purple : Colors.LightBlue);
    }
}

// Hitbox consisting of multiple hitboxes
public class CompoundHitbox : Hitbox
{
    protected HashSet<Hitbox> hitboxes;

    public CompoundHitbox( HitboxOwner owner, ref HashSet<Hitbox> hitboxes )
    {
        this.hitboxes = hitboxes;
        this.hitboxOwner = owner;
        CollisionManager.AddHitbox(this);
    }

    public HashSet<Hitbox> GetHitboxes() { return hitboxes; }

    public override (float, HitboxOwner) GetFirstRayHit(Vector2 start, Vector2 direction)
    {
        float firstHit = -1;
        HitboxOwner outOwner = hitboxOwner;

        foreach (Hitbox hitbox in hitboxes)
        {
            (float potentialHit, HitboxOwner potentialOwner) = hitbox.GetFirstRayHit(start, direction);
            if (potentialHit > 0)
            {
                if (firstHit < 0 || potentialHit < firstHit)
                {
                    firstHit = potentialHit;
                    outOwner = potentialOwner;
                }
            }
        }

        return (firstHit, outOwner);
    }

    public override (bool, HitboxOwner) GetContainsHit(Vector2 testPosition)
    {
        foreach (Hitbox hitbox in hitboxes)
        {
            (bool hit, HitboxOwner potentialOwner) = hitbox.GetContainsHit(testPosition);
            if (hit)
            {
                return (true, potentialOwner);
            }
        }
        return (false, hitboxOwner);
    }

    public override Vector2 GetNormal(Vector2 testPoint)
    {
        if (hitboxOwner is CompoundHitboxOwner compoundOwner)
            return compoundOwner.GetNormal(testPoint);
            
        // TODO: This kind of sucks, assumes GetNormal() is right after any GetHits()
        Logger.Log("Compound hitbox was directly queried for normal", LogLevel.error);
        return Vector2.Up;
    }
    public override void DrawHitbox(Control drawOwner)
    {
        foreach (Hitbox hitbox in hitboxes)
        {
            hitbox.UpdateHitbox();
            hitbox.DrawHitbox(drawOwner);
        }
    }
}