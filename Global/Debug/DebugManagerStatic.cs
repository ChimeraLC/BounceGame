// Static methods for the debug manager

using System.Collections.Generic;
using Godot;


public enum DebugFlag
{
    DrawUnitHitboxes
}

public partial class DebugManager: Control
{
    static Dictionary<DebugFlag, int> debugFlags = new Dictionary<DebugFlag, int>();

    public static int GetFlag(DebugFlag flag)
    {
        if (debugFlags.ContainsKey(flag))
            return debugFlags[flag];
        return 0;
    }
    public static bool CheckFlag(DebugFlag flag)
    {
        return GetFlag(flag) != 0;
    }


    public static void SetFlag(DebugFlag flag, int value)
    {
        debugFlags[flag] = value;
        Logger.Log($"Flag {flag} set to {value}");
    }

    public static void ToggleFlag(DebugFlag flag)
    {
        SetFlag(flag, CheckFlag(flag) ? 0 : 1);
    }
}