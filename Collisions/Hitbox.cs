using System.Runtime.Remoting.Messaging;
using Godot;

public enum HitboxOwnerType
{
    Enemy,
    Player,
    Any
}

public interface HitboxOwner
{
    // Called before any hitbox calculations, so they aren't updated every frame
    void UpdateHitbox();
    Hitbox GetHitbox();
    HitboxOwnerType GetHitboxOwnerType();
}

public abstract class Hitbox
{
    public int lastUpdatedHitboxFrame = -1;

    public HitboxOwner hitboxOwner;
    public abstract float GetFirstRayHit(Vector2 start, Vector2 direction);
}

// TODO: Checks for if ray starts already within hitbox?

// Rectable hitbox defined by its center and width/height
public class RectHitbox : Hitbox
{
    public Vector2 center;
    public Vector2 dimensions;
    public RectHitbox(HitboxOwner inOwner, Vector2 inCenter, Vector2 inDimensions)
    {
        hitboxOwner = inOwner;
        center = inCenter;
        dimensions = inDimensions;
    }

    public override float GetFirstRayHit(Vector2 start, Vector2 direction)
    {
        // Set center of ray to be at 0, 0
        Vector2 offsetCenter = center - start;

        // TODO: Could this ever 'slide' through a corner

        // Only need to check closer 2 sides
        if (!Mathf.IsZeroApprox(direction.x))
        {
            float xHit = offsetCenter.x + (direction.x > 0 ? -1 : 1) * dimensions.x / 2;
            float hitTime = xHit / direction.x;
            // Valid hit that's within the bounds
            if (hitTime > 0 && Mathf.Abs(direction.y * hitTime - offsetCenter.y) <= dimensions.y / 2)
                return hitTime;
        }

        if (!Mathf.IsZeroApprox(direction.y))
        {
            float yHit = offsetCenter.y + (direction.y > 0 ? -1 : 1) * dimensions.y / 2;
            float hitTime = yHit / direction.y;

            if (hitTime > 0 && Mathf.Abs(direction.x * hitTime - offsetCenter.x) <= dimensions.x / 2)
                return hitTime;
        }
        
        return -1;
    }
}

// Circle hitbox defined by its center and radius
public class CircleHitbox : Hitbox
{
    public Vector2 center;
    public float radius;
    public CircleHitbox(HitboxOwner inOwner, Vector2 inCenter, float inRadius)
    {
        hitboxOwner = inOwner;
        center = inCenter;
        radius = inRadius;
    }

    public override float GetFirstRayHit(Vector2 start, Vector2 direction)
    {
        // Set center of ray to be at 0, 0
        Vector2 offsetCenter = center - start;

        // (tx - a)^2 + (ty - b)^2 = r^2
        // (x^2+y^2)t^2 - (2xa + 2yb)t + (a^2 + b^2 - r^2) = 0
        // This only works since direction is normalized
        return Utils.PositiveQuadraticIfPossible(direction.x * direction.x + direction.y * direction.y,
            -2 * direction.x * offsetCenter.x - 2 * direction.y * offsetCenter.y,
            offsetCenter.x * offsetCenter.x + offsetCenter.y * offsetCenter.y - radius * radius);
    }
}