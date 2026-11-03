using System.Collections.Generic;
using Godot;

public partial class DebugManager: Control
{
    public static DebugManager Instance { get; private set; }
    private RichTextLabel leftTopLabel;
    private RichTextLabel leftMiddleLabel;
    private RichTextLabel rightTopLabel;
    private Label centerLabel;
    
    public static bool Ghosting;
    public override void _Ready()
    {   
        Instance = this;

        centerLabel = GetNode<Label>("CenterLabel");
        leftTopLabel = GetNode<RichTextLabel>("LeftTopLabel");
        leftMiddleLabel = GetNode<RichTextLabel>("LeftMiddleLabel");
        rightTopLabel = GetNode<RichTextLabel>("RightTopLabel");
    }

    public static void Tick(float delta)
    {
#if DEBUG
        if (Instance != null)
            Instance.Update();
#endif // #if DEBUG
    
    }

    public static void DebugStringCenter(string message)
    {
        Instance.centerLabel.Text = message;
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

    public override void _Draw()
    {
        if (CheckFlag(DebugFlag.DrawUnitHitboxes))
        {
            CollisionManager.DrawHitboxes(this);
        }
    }
}
