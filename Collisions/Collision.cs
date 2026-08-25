using System.Collections.Generic;
using Godot;

/// <summary>
/// Generic handler for collision logic
/// </summary>
public partial class CollisionManager
{
    public static CollisionManager Instance { get; private set; }
    private HashSet<CollisionObstacle> collisionObstacles;
    private HashSet<Hitbox> hitboxes;
    public CollisionManager()
    {
        Instance = this;
        collisionObstacles = new HashSet<CollisionObstacle>();
        hitboxes = new HashSet<Hitbox>();
    }

    /// <summary>
    /// Returns distance to first straight line rayhit in a given (normalized) direction
    /// </summary>
    public static (float, CollisionObstacle) GetFirstCollisionRayhit(Vector2 start, Vector2 direction)
    {
        float firstHit = -1;
        CollisionObstacle hitObstacle = null;

        float potentialHit;
        foreach ( CollisionObstacle obstacle in Instance.collisionObstacles)
        {
            potentialHit = obstacle.GetFirstRayhit(start, direction);
            if (potentialHit > 0 && (firstHit < 0 || potentialHit < firstHit))
            {
                firstHit = potentialHit;
                hitObstacle = obstacle;
            }
        }

        return (firstHit, hitObstacle);
    }

    public static void AddCollisionObstacle(CollisionObstacle inObstacle)
    {
        Logger.Log($"Adding collision obstacle {inObstacle}", LogLevel.info);
        Instance.collisionObstacles.Add(inObstacle);
    }

    public static void RemoveCollisionObstacle(CollisionObstacle outObstacle)
    {
        Logger.Log($"Removing collsion obstacle {outObstacle}", LogLevel.info);
        Instance.collisionObstacles.Remove(outObstacle);
    }

    public static (float, HitboxOwner) GetFirstHitboxRayhit(Vector2 start, Vector2 direction, HitboxOwnerType queryType = HitboxOwnerType.Any)
    {
        float firstHit = -1;
        HitboxOwner hitOwner = null;

        float potentialHit;
        HitboxOwner potentialOwner;
        int currentFrame = GameManager.GetCurrentFrame();
        foreach ( Hitbox hitbox in Instance.hitboxes)
        {
            if (queryType != HitboxOwnerType.Any && hitbox.GetHitboxOwnerType() != queryType)
            {
                continue;
            }

            // Update hitbox position if necessary
            if (hitbox.lastUpdatedHitboxFrame != currentFrame)
            {
                hitbox.UpdateHitbox();
                hitbox.lastUpdatedHitboxFrame = currentFrame;
            }

            (potentialHit, potentialOwner) = hitbox.GetFirstRayHit(start, direction);
            if (potentialHit > 0 && (firstHit < 0 || potentialHit < firstHit))
            {
                firstHit = potentialHit;
                hitOwner = potentialOwner;
            }
        }

        return (firstHit, hitOwner);
    }

    // Returns a hitbox owner if it overlaps with the position; doesn't test closeness
    public static (bool, HitboxOwner) GetContainsHit(Vector2 testPosition, HitboxOwnerType queryType = HitboxOwnerType.Any)
    {
        int currentFrame = GameManager.GetCurrentFrame();
        foreach ( Hitbox hitbox in Instance.hitboxes)
        {
            if (queryType != HitboxOwnerType.Any && hitbox.GetHitboxOwnerType() != queryType)
            {
                continue;
            }

            // Update hitbox position if necessary
            if (hitbox.lastUpdatedHitboxFrame != currentFrame)
            {
                hitbox.UpdateHitbox();
                hitbox.lastUpdatedHitboxFrame = currentFrame;
            }

            (bool hit, HitboxOwner potentialOwner) = hitbox.GetContainsHit(testPosition);
            if (hit)
            {
                return (hit, potentialOwner);
            }
        }
        return (false, null);
    } 

    public static void AddHitbox(Hitbox inHitbox)
    {
        Logger.Log($"Adding collision obstacle {inHitbox}", LogLevel.info);
        Instance.hitboxes.Add(inHitbox);
    }

    public static void RemoveHitbox(Hitbox outHitbox)
    {
        Logger.Log($"Removing collsion hitbox {outHitbox}", LogLevel.info);
        Instance.hitboxes.Remove(outHitbox);
    }
}

public abstract class CollisionObstacle
{
    public abstract float GetFirstRayhit(Vector2 start, Vector2 direction);
}

// Basic collisions of vertical or horizontal axis
public partial class VertAxisCollision : CollisionObstacle
{
    private float axisPosition;
    public VertAxisCollision(float inAxisPosition)
    {
        axisPosition = inAxisPosition;
    }
    public override float GetFirstRayhit(Vector2 start, Vector2 direction)
    {
        return (axisPosition - start.x) / direction.x;
    }
}

public partial class HorAxisCollision : CollisionObstacle
{
    private float axisPosition;
    public HorAxisCollision(float inAxisPosition)
    {
        axisPosition = inAxisPosition;
    }

    public override float GetFirstRayhit(Vector2 start, Vector2 direction)
    {
        return (axisPosition - start.y) / direction.y;
    }
}