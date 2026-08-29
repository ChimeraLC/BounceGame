using Godot;
using System;

/// <summary>
/// Global manager for all game logic. Tick is run through this
/// </summary>
public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    public float timeDilation { get; private set; }

    public bool paused { get; private set; }

    public static bool mouseK { get; private set; }

    private CollisionManager collisionManager;
    public EnemyManager enemyManager;
    private int currentFrame;
    private float gameTime;

    public override void _Ready()
    {
        Instance = this;

        Logger.Log("Game Manager Start", LogLevel.info);

        paused = false;
        timeDilation = 1;
        mouseK = true;
        currentFrame = 0;
        gameTime = 0;

        // Create collision manager
        Logger.Log("Creating Collsion Manager", LogLevel.info);
        collisionManager = new CollisionManager();

        Logger.Log("Creating Enemy Manager", LogLevel.info);
        enemyManager = new EnemyManager();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (paused)
        {
            
        }
        else
        {
            float trueTime = delta * timeDilation;
            Tick(trueTime);
            gameTime += trueTime;
            currentFrame++;
        }

#if DEBUG
        TickDebug(delta);
#endif
    }

    public static int GetCurrentFrame()
    {
        return Instance.currentFrame;
    }

    public static float GetGameTime()
    {
        return Instance.gameTime;
    }

    public static void EndGame()
    {
		Instance.GetTree().Quit();
    }


    private void Tick(float delta)
    {
        if (playerInstance != null)
            playerInstance.Tick(delta);

        enemyManager.Tick(delta);
        DebugManager.Tick(delta);
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
            DebugManager.ToggleHitboxDebug();
        }
    }


}