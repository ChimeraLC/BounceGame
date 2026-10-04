using Godot;
using System;

public class MainUI : Control
{
    private Label centerLabel;
    private float centerLabelFade = 0;
    private ColorRect healthDisplay;
    public override void _Ready()
    {
        GameManager.RegisterUIInstance(this);
    
        healthDisplay = GetNode<ColorRect>("HealthDisplay");
        healthDisplay.Color = Consts.GetColor(ColorType.Dark);
        
        centerLabel = GetNode<Label>("CenterLabel");

        SetSize(new Vector2(Consts.ScreenWidth, Consts.ScreenHeight));
    }

    public void UpdateHealth(float healthAmount)
    {
        healthDisplay.RectSize = new Vector2(healthAmount * 200, 40);
    }

    public override void _Process(float delta)
    {
        // TODO: Only update when weapon or ammo changes
        Update();

        if (centerLabelFade > 0)
        {
            centerLabelFade = Mathf.Max(0, centerLabelFade - delta);
            centerLabel.Modulate = new Color(1, 1, 1, centerLabelFade);
        }
    }

    public override void _Draw()
    {
        GameManager.GetPlayer().GetCurrentGun().DisplayAmmo( this, new Vector2(850, 120));
    }

    public void DisplayCenterText(string text, float duration = 5)
    {
        centerLabel.Modulate = new Color(1, 1, 1, 1);
        centerLabel.Text = text;

        centerLabelFade = duration;

    }
}