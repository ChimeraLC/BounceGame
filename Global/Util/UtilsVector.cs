using Godot;

public partial class Utils
{
    public static Vector2 AngleToVec(float angle)
    {
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    public static Vector2 RandVectorInRect(float width, float height)
    {
        return new Vector2(RandRange(-width/2, width/2), RandRange(-height/2, height/2));
    }
}