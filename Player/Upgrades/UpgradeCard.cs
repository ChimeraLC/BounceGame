using Godot;

public abstract class UpgradeCard : Control
{
    private Upgrade upgrade;
    private Control cardRect;
    public void Initialize(Upgrade upgrade)
    {
        (string name, string description) = UpgradeData.GetData(upgrade);
        GetNode<Label>("Upgrade Title").Text = name;
        GetNode<Label>("Upgrade Description").Text = description;

        cardRect = GetNode<Control>("CardBack");

        this.upgrade = upgrade;
    }

    public void Tick(float delta)
    {
        // Detect mouseovers and stuff
        if (Input.IsActionJustPressed("key_fire"))
        {
            if (cardRect.GetGlobalRect().HasPoint(GetGlobalMousePosition()))
            {
                UpgradeManager.Instance.SelectCard(upgrade);
            }
        }
    }
}