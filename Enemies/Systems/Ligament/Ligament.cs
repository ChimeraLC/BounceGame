using Godot;
using System;
using System.Runtime.Serialization;

public static class Ligament
{
    // Returns positions for a two-segment arm of lengths length1 and length2, going from start to end
    // forward indicates which direction the arm should bend
    // Returns global angles of segments, and base position of segment2
    public static (float firstAngle, float secondAngle, Vector2 secondPosition) GetPositions(Vector2 start, Vector2 end, float length1, float length2, bool clockwise = true)
    {
        float totalLength = (end - start).Length();
        float baseAngle = (end-start).Angle();
        
        // Check edge cases
        // Invalid triangle
        if (totalLength < Mathf.Abs(length1 - length2))
        {
            return (baseAngle, baseAngle + Mathf.Pi, start + (end - start) / totalLength * length1 );
        }

        // Too far
        if (totalLength >= length1 + length2)
        {
            // Prioritize keeping ligaments together over reaching target
            return (baseAngle, baseAngle, start + (end - start) / totalLength * length1);
        }

        float cosTheta = (length1 * length1 + totalLength * totalLength - length2 * length2) / (2 * length1 * totalLength);
        float theta = Mathf.Acos(cosTheta);
        float omega = Mathf.Asin(totalLength * Mathf.Sin(theta) / length2);
        if (length1 / totalLength > cosTheta)
            omega = Mathf.Pi - omega;

        float angle1 = baseAngle + theta * (clockwise ? -1 : 1);
        float angle2 = angle1 + omega * (clockwise ? 1 : -1);
        return (angle1, angle2, start + length1 * Utils.AngleToVec(angle1));
    }
}
