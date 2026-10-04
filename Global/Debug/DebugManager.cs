using Godot;

public class DebugManager: Control
{
    public static DebugManager Instance { get; private set; }
    private RichTextLabel leftTopLabel;
    private RichTextLabel leftMiddleLabel;
    private RichTextLabel rightTopLabel;
    
    public static bool Ghosting;
    public static bool Hitboxes;
    public override void _Ready()
    {   
        Instance = this;

        leftTopLabel = GetNode<RichTextLabel>("LeftTopLabel");
        leftMiddleLabel = GetNode<RichTextLabel>("LeftMiddleLabel");
        rightTopLabel = GetNode<RichTextLabel>("RightTopLabel");
    }

    public static void Tick(float delta)
    {
        if (Instance != null && Hitboxes)
        {
            Instance.Update();
        }
    }

    public static void DebugStringTop(string message)
    {
        Instance.leftTopLabel.Text = message;
    }

    public static void DebugStringMiddle(string message)
    {
        Instance.leftMiddleLabel.Text = message;
    }

    public static void DebugStringRight(string message)
    {
        Instance.rightTopLabel.Text = message;
    }

    public static void ToggleHitboxDebug()
    {
        Hitboxes = !Hitboxes;
        Instance.Update();
    }

    public override void _Draw()
    {
        if (Hitboxes)
        {
            CollisionManager.DrawHitboxes(this);
        }
    }
}
