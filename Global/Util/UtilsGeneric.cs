using Godot;

public static partial class Utils
{
    public static (Vector2, bool) MoveTowards(Vector2 start, Vector2 goal, float distance, 
        float padding = 0, bool isWithin = false) // isWithin: just wants to be within padding; otherwise, will also move away
    {
        float curDistance = (goal-start).Length();

        // Easy out
        if (padding <= 0 && curDistance <= distance)
            return (goal, true);
        if (isWithin && curDistance <= padding)
            return (start, true);

        (float goalDistance, bool reached) = MoveTowards(curDistance, padding, distance);

        Vector2 direction = curDistance == 0 ? Vector2.Left : (start - goal) / curDistance;
        
        return (goal + direction * goalDistance, reached);
    }

    
    public static (Vector2, bool) MoveTowardsWithin(Vector2 start, Vector2 goal, float distance, 
        float padding = 0)
    {
        float curDistance = (goal-start).Length();

        // Easy out
        if (curDistance <= distance)
            return (goal, true);

        (float goalDistance, bool reached) = MoveTowards(curDistance, padding, distance);

        Vector2 direction = curDistance == 0 ? Vector2.Left : (start - goal) / curDistance;
        
        return (goal + direction * goalDistance, reached);
    }

    public static (float, bool) MoveTowards(float start, float end, float distance, float padding = 0)
    {
        float curDistance = Mathf.Abs(start - end);
        
        // Close enough to goal position
        if (Mathf.Abs(curDistance - padding) < distance)
            return (end + padding * Mathf.Sign(start - end), true);

        if (curDistance < padding) 
        {
            return (start + distance * Mathf.Sign(start - end), false);
        }
        else
        {
            return (start + distance * Mathf.Sign(end - start), false);   
        }
    }

    public static (float, bool) MoveTowardsNearestMultiple(float start, float value, float distance)
    {
        if (value <= 0)
            return (start, true);

        float overshot = start % value;
        
        // Closer to lower multiple
        if (overshot < value / 2)
        {
            if (overshot < distance)
                return (start - overshot, true);
            return (start - distance, false);
        }
        else
        {
            if (value - overshot < distance)
                return (start - overshot + value, true);
            return (start + distance, false);
        }
    }

    // Moves towards a given angle. In radians
    public static (float, bool) MoveTowardsAngle(float start, float end, float distance)
    {
        float difference = (end - start) % (2 * Mathf.Pi);
        difference = (2 * difference) % (2 * Mathf.Pi) - difference;
        if (Mathf.Abs(difference) < distance)
            return (end, true);
        return (start + Mathf.Sign(difference) * distance, false);
    }
}