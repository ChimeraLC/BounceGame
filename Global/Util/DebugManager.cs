using Godot;

public class DebugManager: Control
{
    public static DebugManager Instance { get; private set; }
    private RichTextLabel topLabel;
    private RichTextLabel middleLabel;
    private RichTextLabel rightLabel;

    public override void _Ready()
    {   
        Instance = this;

        topLabel = GetNode<RichTextLabel>("TopLabel");
        middleLabel = GetNode<RichTextLabel>("MiddleLabel");
        rightLabel = GetNode<RichTextLabel>("RightLabel");
    }

    public static void DebugStringTop(string message)
    {
        Instance.topLabel.Text = message;
    }

    public static void DebugStringMiddle(string message)
    {
        Instance.middleLabel.Text = message;
    }

    public static void DebugStringRight(string message)
    {
        Instance.rightLabel.Text = message;
    }
}
