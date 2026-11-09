using System.Collections.Generic;
using Godot;


public partial class TeethBoss : Boss, CompoundHitboxOwner
{

    [Export]
    public PackedScene ToothScene;
    private CompoundHitbox hitbox;
    private HashSet<Hitbox> compoundBoxes = new HashSet<Hitbox>();

    private List<Tooth> Teeth = new List<Tooth>();
    private FieldEdge currentEdge;
    private const int TeethCount = 16;
    private const int TeethSpacing = 40;
    private const int TeethVariance = 10;
    private const int MinWidth = 300;
    private const int MaxWidth = 600;

    public override void _Ready()
    {
        base._Ready();

        for (int i = 0; i < TeethCount; i++)
        {
            Tooth newTooth = ToothScene.Instance<Tooth>();
            RectHitbox newHitbox = new RectHitbox(newTooth, Position, new Vector2(20, 20), true);
            newTooth.hitbox = newHitbox;
            compoundBoxes.Add(newHitbox);
            Teeth.Add(newTooth);
            AddChild(newTooth);
        }

        hitbox = new CompoundHitbox(this, ref compoundBoxes);

        health = 20;
    }

    protected override void TickMainPlayer(float delta)
    {
        if (Input.IsActionJustPressed("debug_action"))
        {
            PlaceTeeth();
        }
    }

    // Places teeth on one of the walls
    private void PlaceTeeth()
    {
    currentEdge = Utils.GenerateRandomEdge(FieldEdge.Left | FieldEdge.Bottom | FieldEdge.Right);

        // Reset teeth position
        foreach (Tooth tooth in Teeth)
        {
            tooth.Position = Vector2.Zero;
            tooth.hitbox.center = Position;
        }

        // Always covers entirety of edges, only covers part of bottom
        switch (currentEdge)
        {
            case FieldEdge.Left:
                Vector2 topEnd = new Vector2(Consts.FieldLeft, Consts.FieldTop) + Vector2.Right * 10;
                for (int i = Consts.FieldTop; i <= Consts.FieldBottom / 40 + 1; i++)
                {
                    Teeth[i].GlobalPosition = topEnd + (TeethSpacing / 2 + i * TeethSpacing) * Vector2.Down
                    + Vector2.Right * Utils.RandRange(-TeethVariance, TeethVariance);

                    Teeth[i].Rotation = Mathf.Pi / 2;
                    Teeth[i].hitbox.center = Teeth[i].GlobalPosition;
                }

            break;
            case FieldEdge.Right:
                topEnd = new Vector2(Consts.FieldRight, Consts.FieldTop) + Vector2.Left * 10;
                for (int i = Consts.FieldTop; i <= Consts.FieldBottom / 40 + 1; i++)
                {
                    Teeth[i].GlobalPosition = topEnd + (TeethSpacing / 2 + i * TeethSpacing) * Vector2.Down
                    + Vector2.Right * Utils.RandRange(-TeethVariance, TeethVariance);

                    Teeth[i].Rotation = -Mathf.Pi / 2;
                    Teeth[i].hitbox.center = Teeth[i].GlobalPosition;
                }
            break;
            case FieldEdge.Bottom:
                (int left, int right) = Utils.RandomSpacedMaxed(Consts.FieldLeft, Consts.FieldRight, MinWidth, MaxWidth);

                int width = right - left;
                right += TeethSpacing - (width % TeethSpacing);
                if (right >= Consts.FieldRight)
                    right -= TeethSpacing;

                Vector2 leftEnd = new Vector2(left, Consts.FieldBottom - 10);
                for (int i = 0; i < (right - left) / 40; i++)
                {
                    Teeth[i].GlobalPosition = leftEnd + (TeethSpacing / 2 + i * TeethSpacing) * Vector2.Right
                    + Vector2.Up * Utils.RandRange(-TeethVariance, TeethVariance);

                    Teeth[i].Rotation = 0;
                    Teeth[i].hitbox.center = Teeth[i].GlobalPosition;
                }
            break;
        }
    }

    public override Vector2 GetSpawnpoint()
    {
        // Hidden offscreen
        return Consts.offScreen;
    }

    public override Hitbox GetHitbox()
    {
        return hitbox;   
    }

    public override void UpdateHitbox()
    {
        // Hitboxes positions are updated internally;
    }

    public Vector2 GetNormal(Vector2 testPoint)
    {
        switch (currentEdge)
        {
            case FieldEdge.Left:
                return Vector2.Right;
            case FieldEdge.Right:
                return Vector2.Left;
            case FieldEdge.Bottom:
                return Vector2.Up;
        }
        return Vector2.Up;
    }
}