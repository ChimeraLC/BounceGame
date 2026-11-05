using System.Collections.Generic;
using Godot;

public partial class DebugManager: Control
{
    public static DebugManager Instance { get; private set; }
    private RichTextLabel leftTopLabel;
    private RichTextLabel leftMiddleLabel;
    private RichTextLabel rightTopLabel;
    private Label centerLabel;
    private TextEdit console;
    
    public static bool Ghosting;
    public override void _Ready()
    {   
        Instance = this;

        centerLabel = GetNode<Label>("CenterLabel");
        leftTopLabel = GetNode<RichTextLabel>("LeftTopLabel");
        leftMiddleLabel = GetNode<RichTextLabel>("LeftMiddleLabel");
        rightTopLabel = GetNode<RichTextLabel>("RightTopLabel");
        console = GetNode<TextEdit>("Console");
    }

    public static void Tick(float delta)
    {
#if DEBUG
        if (Instance != null)
            Instance.Update();
#endif // #if DEBUG
    
    }

#if DEBUG
    // Console stuff should work whenever; separate from tick
    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("debug_console"))
        {
            if (console.Visible)
            {
                console.ReleaseFocus();
                console.Visible = false;
            }
            else
            {
                console.Text = "";
                console.Visible = true;
                console.GrabFocus();
            }
        }

        if (Input.IsActionJustPressed("debug_enter"))
        {
            if (console.Visible)
            {
                RunCommand(console.Text);
                console.ReleaseFocus();
                console.Visible = false;
                console.Text = "";
            }
        }
    }

    private void RunCommand(string command)
    {
        Logger.Log($"Running command: {command}", LogLevel.info);

        command = command.ToLower();

        string[] parsed = command.StripEdges().Split(" ");
        if (parsed.Length == 0)
            Logger.Log("Recieved empty command", LogLevel.warn);

        switch (parsed[0])
        {
            case "showupgrades":
                UpgradeManager.Instance.ShowUpgrades();
            break;
        }
    }
#endif // #if DEBUG

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
