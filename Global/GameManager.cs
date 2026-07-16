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

    public override void _Ready()
    {
        Instance = this;

        Logger.Log("Game Manager Start", LogLevel.info);

        paused = false;
        timeDilation = 1;
        mouseK = true;

        // Create collision manager
        Logger.Log("Creating Collsion Manager", LogLevel.info);
        collisionManager = new CollisionManager();
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
        }
    }

    public static void EndGame()
    {
		Instance.GetTree().Quit();
    }


    public void Tick(float delta)
    {
        if (playerInstance != null)
            playerInstance.Tick(delta);
    }
}