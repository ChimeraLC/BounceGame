using Godot;
using System;
using System.Runtime.CompilerServices;

/// <summary>
/// Global manager for all game logic. Tick is run through this
/// </summary>
public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    public bool paused { get; private set; }

    public static bool mouseK { get; private set; }

    private CollisionManager collisionManager;
    public EnemyManager enemyManager;
    public static int CurrentFrame;
    public static Node2D WorldRoot;
    private float gameTime;

    public override void _Ready()
    {
        Instance = this;

        // Read config file
        SetupDebugConfigs();

        Logger.Log("Game Manager Start", LogLevel.info);

        // Set initial values
        paused = false;
        mouseK = true;
        CurrentFrame = 0;
        gameTime = 0;

        WorldRoot = GetTree().Root.GetNode<Node2D>("World");
        
        // Create collision manager
        Logger.Log("Creating Collsion Manager", LogLevel.info);
        collisionManager = new CollisionManager();

        Logger.Log("Creating Enemy Manager", LogLevel.info);
        enemyManager = new EnemyManager();

#if DEBUG
        enemyManager.VerifyData();
#endif // #if DEBUG
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (paused)
        {
            
        }
        else
        {
            float trueTime = delta * GetTimeDilation();
            
            UITick(delta); // UI Should always tick at normal speed
            
            // Only run tick if not paused
            if (trueTime > 0)
            {
                GameTick(trueTime);
                gameTime += trueTime;
                CurrentFrame++;
            }
        }

#if DEBUG
        TickDebug(delta);
#endif
    }

    public static float GetGameTime()
    {
        return Instance.gameTime;
    }

    private float GetTimeDilation()
    {
        float baseDilation = 1;

        // Pause while upgrades shown
        if (UpgradeManager.IsActive())
            baseDilation = 0;

        // If player has time effecting upgrades
        if (Player.Instance != null)
            baseDilation *= Player.Instance.GetTimeDilation();

        return baseDilation;
    }

    public static void EndGame()
    {
		Instance.GetTree().Quit();
    }

    // Tick for game related logic; Is affected by time dilations
    private void GameTick(float delta)
    {
        if (Player.Instance != null)
            Player.Instance.Tick(delta);

        enemyManager.Tick(delta);
    }

    // Tick for ui logic; always ticks at the same speed
    private void UITick(float delta)
    {
        DebugManager.Tick(delta);

        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.Tick(delta);
    }

    private void TickDebug(float delta)
    {
        if (Input.IsActionJustPressed("debug_ghost"))
        {
            DebugManager.Ghosting = !DebugManager.Ghosting;
            DebugManager.DebugStringMiddle(DebugManager.Ghosting ?
                "GHOSTING" : "");
        }

        if (Input.IsActionJustPressed("debug_hitbox"))
        {
            DebugManager.ToggleFlag(DebugFlag.DrawUnitHitboxes);
        }

        if (Input.IsActionJustPressed("debug_pause"))
        {
            SetPaused(!paused);
        }
    }

    private void SetPaused(bool isPaused)
    {
        paused = isPaused;
        DebugManager.DebugStringCenter(paused ? "PAUSED" : ""); 
    }

    private static void SetupDebugConfigs()
    {
        File fileReader = new File();
        if (fileReader.Open("res://configs.txt", File.ModeFlags.Read) == Error.Ok)
        {
            string content = fileReader.GetLine();
            while (content != "")
            {
                // Set debugs
                string[] splits = content.Split(" ");
                bool valid = false;
                if (splits.Length == 2 && splits[0].Length > 0)
                {
                    if (int.TryParse(splits[1], out int value))
                    {
                        valid = true;
                        DebugFlag outflag;
                        if (Enum.TryParse<DebugFlag>(splits[0], out outflag))
                        {
                            DebugManager.SetFlag(outflag, value);   
                        }
                        else
                        {
                            Logger.Log($"Configs has invalid flag: \"{splits[0]}\"", LogLevel.warn);
                        }
                    }                    
                }

                if (!valid)
                    Logger.Log($"Configs has invalid line: \"{content}\"", LogLevel.warn);

                content = fileReader.GetLine();
            }
        }
    }
}