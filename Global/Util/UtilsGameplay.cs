using Godot;
using System;

public enum FieldEdge
{
    None = 0,
    Left = 1 << 0,
    Bottom = 1 << 1,
    Right = 1 << 2,
    Top = 1 << 3,
}

public static partial class Utils
{
    private static Random rand = new Random();

    /* Generates a random point around the edges of the field. Corner padding is minimum
    *  distance from a corner. Side padding is distance from sides.
    *  Edge is bitmask of all allowed sides.
    */
    public static (Vector2, FieldEdge) GenerateRandomEdgepoint( int cornerPadding,
        int sidePadding, FieldEdge edges = FieldEdge.Left | FieldEdge.Right | FieldEdge.Top | FieldEdge.Bottom)
    {
        int values = 0;
        foreach (FieldEdge edge in Enum.GetValues(typeof(FieldEdge)))
        {
            if ((edges & edge) == edge)
                values++;
        }
    
        if (values == 0)
        {
            Logger.Log("GenerateRandomEdgepoint() recieved no sides", LogLevel.error);
            return (Vector2.Zero, FieldEdge.None);
        }

        int chosenSide = rand.Next(values);
        foreach (FieldEdge edge in Enum.GetValues(typeof(FieldEdge)))
        {
            if ((edges & edge) == edge)
            {
                chosenSide--;
                if (chosenSide < 0)
                {
                    return (GenerateEdgepoint(cornerPadding, sidePadding, edge), edge);
                }
            }
        }

        // This codepath should never happen
        return (Vector2.Zero, FieldEdge.None);
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
            return new Vector2( edge == FieldEdge.Top ? Consts.FieldTop + sidePadding :
                Consts.FieldBottom - sidePadding, xPos);
        }

        Logger.Log("GenerateEdgepoint() given an invalid side", LogLevel.error);
        return Vector2.Zero;
    }
}