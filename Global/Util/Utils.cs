using Godot;

public static class Utils
{
    /// <summary>
    /// Returns smallest positive solution to quadratic ax^2 + bx + c = 0
    /// If both (all) solutions are negative, returns either
    /// Assumes at least one solution exists
    /// </summary>
    public static float FirstPositiveQuadratic(float a, float b, float c)
    {
        if (a == 0)
            return -c / b;

        // Assume this exists
        float det = Mathf.Sqrt(b * b - 4 * a * c);

        float sol1 = (-b + det) / (2 * a);
        float sol2 = (-b - det) / (2 * a);

        if (sol1 < 0)
            return sol2;
        
        if (sol2 < 0)
            return sol1;
        
        return sol1 < sol2 ? sol1 : sol2;
        
    }
}