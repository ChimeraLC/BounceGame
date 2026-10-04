using System.Collections.Generic;
using Godot;

public enum ColorType
{
    Light,
    Dark,
    Danger,
    Enemy
}

public static class Consts
{
    public static float Gravity = 180;
    public static Vector2 scaleFlipX = new Vector2(-1, 1);
    public static Vector2 scaleNormal = new Vector2(1, 1);

    public static int ScreenHeight = 600;
    public static int ScreenWidth = 1024;

    // Field
    public static int FieldLeft = 300;
    public static int FieldRight = 724;
    public static int FieldTop = 0;
    public static int FieldBottom = 550;

    // Colors
    public static Color colorLight = Color.Color8(185, 185, 185);
    public static Color colorDark = Color.Color8(28, 28, 28);
    public static Color colorDanger = Color.Color8(90, 22, 22);
    public static Color colorEnemy = Color.Color8(178, 26, 26);

    public static Dictionary<ColorType, Color> colorMap = new Dictionary<ColorType, Color>
    {
        {ColorType.Light, colorLight},
        {ColorType.Dark, colorDark},
        {ColorType.Danger, colorDanger},
        {ColorType.Enemy, colorEnemy}  
    };

    public static Color GetColor(ColorType colorType)
    {
        return colorMap[colorType];
    }

    // Derived
    public static int FieldWidth = FieldRight - FieldLeft;
    public static int FieldHeight = FieldBottom - FieldTop;
}