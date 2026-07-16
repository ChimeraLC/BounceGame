using Godot;

/// <summary>
/// Level of displayed message
/// </summary>
public enum LogLevel
{
    debug,
    info,
    warn,
    error,
    fatal
}

/// <summary>
/// Log channels to have individual log level cutoffs
/// </summary>
public enum LogChannel
{
    Default = LogLevel.debug, // Default will always display log level
    Movement = LogLevel.warn,
}

public static class Logger
{
    public static void Log(string message, LogLevel level, LogChannel channel = LogChannel.Default)
    {
        if (level >= (LogLevel) channel)
        {
            if (level >= LogLevel.error) 
            {
                GD.PrintErr(message);
                GD.PrintErr(System.Environment.StackTrace);
            }
            else GD.Print(message);
        }

        if (OS.IsDebugBuild())
        {
            if (level >= LogLevel.fatal)
            {
                GameManager.EndGame();
            }
        }
    }
}
