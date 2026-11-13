using Godot;

public static partial class Utils
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

    /// <summary>
    /// Returns smallest positive solution if it exists; returns -1 in all other cases
    /// </summary>
    /// <returns></returns>
    public static float PositiveQuadraticIfPossible(float a, float b, float c)
    {
        if (a == 0)
            return -c / b;

        float det = b * b - 4 * a * c;
        if (det < 0)
            return -1;
        det = Mathf.Sqrt(det);
        
        float sol1 = (-b + det) / (2 * a);
        float sol2 = (-b - det) / (2 * a);

        if (sol1 < 0)
            return sol2;
        
        if (sol2 < 0)
            return sol1;
        
        return sol1 < sol2 ? sol1 : sol2;
    }

    public static (int, float, float) SolveQuadratic(float a, float b, float c)
    {
        if (a == 0)
            return (1, -c / b, 0);

        float det = b * b - 4 * a * c;
        if (det < 0)
            return (0, 0, 0);
        det = Mathf.Sqrt(det);
        
        float sol1 = (-b + det) / (2 * a);
        float sol2 = (-b - det) / (2 * a);

        return (2, sol1, sol2);
    }

    // Returns two random ints between low and high, and least spacing apart
    public static (int, int) RandomSpaced(int low, int high, int spacing)
    {
        // Edge cases
        if (high < low)
            return (high, low);

        if (spacing > high - low)
            return (low, high);
    
        int lower = RandRange(low, high-spacing);
        int higher = RandRange(lower + spacing, high);
        return (lower, higher);
    }

    // Returns two random ints between low and high, between minSpacing and maxSpacing apart
    public static (int, int) RandomSpacedMaxed(int low, int high, int minSpacing, int maxSpacing)
    {
        // Assumes values are all legal
        int lower = RandRange(low, high-minSpacing);
        int higher = RandRange(lower + minSpacing, low + maxSpacing);
        return (lower, higher);
    }

    /// <summary> Returns random value between low (inclusive) and high (non-inclusive) </summary>
    public static int RandRange(int low, int high)
    {
        return rand.Next() % (high - low) + low;
    }

    public static float RandRange(float low, float high)
    {
        return rand.Next()  % (high - low) + low;
    }
}