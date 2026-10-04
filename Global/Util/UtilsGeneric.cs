using Godot;

public static partial class Utils
{
    public static (Vector2, bool) MoveTowards(Vector2 start, Vector2 goal, float distance, 
        float padding = 0)
    {
        float curDistance = (goal-start).Length();

        // Easy out
        if (padding <= 0 && curDistance <= distance)
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
            return (start + curDistance * Mathf.Sign(start - end), false);
        }
        else
        {
            return (start + curDistance * Mathf.Sign(end - start), false);   
        }
    }
}