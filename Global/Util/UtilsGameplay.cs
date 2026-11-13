using Godot;
using System;

public enum FieldEdge
{
    None = 1 << 0,
    Left = 1 << 1,
    Bottom = 1 << 2,
    Right = 1 << 3,
    Top = 1 << 4,
}

public static partial class Utils
{
    private static Random rand = new Random();

    /* Generates a random point around the edges of the field. Corner padding is minimum
    *  distance from a corner. Side padding is distance from sides.
    *  Edge is bitmask of all allowed sides.
    */
    // TODO: Version that equalizes probability across size
    public static (Vector2, FieldEdge) GenerateRandomEdgepoint( int cornerPadding,
        int sidePadding, FieldEdge edges = FieldEdge.Left | FieldEdge.Right | FieldEdge.Top | FieldEdge.Bottom)
    {
        FieldEdge chosenEdge = GenerateRandomEdge(edges);
        if (chosenEdge != FieldEdge.None)
            return (GenerateEdgepoint(cornerPadding, sidePadding, chosenEdge), chosenEdge);


        // This codepath should never happen
        return (Vector2.Zero, FieldEdge.None);
    }

    public static FieldEdge GenerateRandomEdge(FieldEdge edges = FieldEdge.Left | FieldEdge.Bottom | FieldEdge.Right)
    {
        int values = 0;
        foreach (FieldEdge edge in Enum.GetValues(typeof(FieldEdge)))
        {
            if ((edges & edge) == edge)
                values++;
        }
    
        if (values == 0)
        {
            Logger.Log("GenerateRandomEdge() recieved no sides", LogLevel.error);
            return FieldEdge.None;
        }

        int chosenSide = rand.Next(values);
        foreach (FieldEdge edge in Enum.GetValues(typeof(FieldEdge)))
        {
            if ((edges & edge) == edge)
            {
                chosenSide--;
                if (chosenSide < 0)
                {
                    return edge;
                }
            }
        }

        return FieldEdge.None;
    }

    public static Vector2 GenerateEdgepoint( int cornerPadding, int sidePadding, FieldEdge edge)
    {
        switch (edge)
        {
        // Horizontal edges
          case FieldEdge.Left:
          case FieldEdge.Right:
            // Make sure bounds remain valid
            cornerPadding = Math.Min(cornerPadding, Consts.FieldHeight / 2 - 1);
            int yPos = rand.Next(Consts.FieldTop + cornerPadding, Consts.FieldBottom - cornerPadding);
            return new Vector2( edge == FieldEdge.Left ? Consts.FieldLeft + sidePadding :
                Consts.FieldRight - sidePadding, yPos);

        // Vertical edges
          case FieldEdge.Top:
          case FieldEdge.Bottom:
            cornerPadding = Math.Min(cornerPadding, Consts.FieldWidth / 2 - 1);
            int xPos = rand.Next(Consts.FieldLeft + cornerPadding, Consts.FieldRight - cornerPadding);
            return new Vector2( xPos, edge == FieldEdge.Top ? Consts.FieldTop + sidePadding :
                Consts.FieldBottom - sidePadding);
        }

        Logger.Log("GenerateEdgepoint() given an invalid side", LogLevel.error);
        return Vector2.Zero;
    }

    /// <summary> Returns vector coresponding to facing inwards from the given edge </summary>
    public static Vector2 GetEdgeNormal( FieldEdge edge )
    {
        switch (edge)
        {
            case FieldEdge.Left:
            return Vector2.Right;
            case FieldEdge.Right:
            return Vector2.Left;
            case FieldEdge.Top:
            return Vector2.Down;
            case FieldEdge.Bottom:
            return Vector2.Up;
        }

        Logger.Log($"GetEdgeNormal() recieved invalid edge {edge}", LogLevel.error);
        return Vector2.Up;
    }
}