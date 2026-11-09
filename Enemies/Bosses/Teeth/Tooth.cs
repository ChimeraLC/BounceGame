using Godot;


public class Tooth : Enemy
{
    public Sprite sprite;
    public RectHitbox hitbox;

    public override void _Ready()
    {
        // Skips enemy register stuff, since it has no logic
    }

    public override void UpdateHitbox() {} // Doesn't move

    public override Hitbox GetHitbox()
    {
        return hitbox;
    }
}