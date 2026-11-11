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
    // How long teeth spend rumbling
    private const float rumbleTime = 2;
    private float rumbleTimer = 0.2f;
    private const float minOutTime = 5;
    private const float maxOutTime = 8;
    private const float minInTime = 1;
    private const float maxInTime = 3;
    
    private Vector2 outDirection; 
    
    // How many teeth are currently in-use
    private int usedTeeth = 0;

    private enum ToothState
    {
        Hidden, // Teeth are not visible
        Rumbling, // Indicating where teeth will show up
        Rising, // Teeth coming out of the wall
        Out, //
        Setting
    }

    private ToothState currentState;
    private float stateTimer = 0;

    public override void _Ready()
    {
        base._Ready();

        usedTeeth = TeethCount;
        for (int i = 0; i < TeethCount; i++)
        {
            Tooth newTooth = ToothScene.Instance<Tooth>();
            RectHitbox newHitbox = new RectHitbox(newTooth, Position, new Vector2(20, 20), true);
            newTooth.hitbox = newHitbox;
            compoundBoxes.Add(newHitbox);
            Teeth.Add(newTooth);
            AddChild(newTooth);
        }

        HideTeeth(); // This is probably not necessary

        currentState = ToothState.Hidden;
        stateTimer = 3;

        hitbox = new CompoundHitbox(this, ref compoundBoxes);
        hitbox.SetEnabled(false);

        health = 20;
    }

    protected override void TickMainPlayer(float delta)
    {
        stateTimer -= delta;
        switch (currentState)
        {
            case ToothState.Hidden:
                if (stateTimer <= 0)
                {
                    stateTimer = rumbleTime;
                    PlaceTeeth();
                    currentState = ToothState.Rumbling;

                    rumbleTimer = 0;
                }
            break;
            case ToothState.Rumbling:
                // Rumbling
                rumbleTimer -= delta;
                if (rumbleTimer <= 0)
                {
                    rumbleTimer += 0.1f;
                    for (int i = 0; i < usedTeeth; i++)
                    {
                        Teeth[i].GlobalPosition = Teeth[i].BasePosition + outDirection * (Utils.RandRange(-TeethVariance, TeethVariance) - 15);
                    }
                }

                if (stateTimer <= 0)
                {
                    stateTimer = Utils.RandRange(minOutTime, maxOutTime);
                    EnableTeeth();
                    currentState = ToothState.Out;
                }
            break;
            case ToothState.Out:
                if (stateTimer <= 0)
                {
                    DisableTeeth();
                    stateTimer = 3f;
                    currentState = ToothState.Setting;
                }
            break;
            case ToothState.Setting:
                for (int i = 0; i < usedTeeth; i++)
                {
                    Teeth[i].GlobalPosition -= delta * outDirection * 40;
                }

                if (stateTimer <= 0)
                {
                    HideTeeth();
                    stateTimer = Utils.RandRange(minInTime, maxInTime);
                    currentState = ToothState.Hidden;
                }
            break;
        }
    }

    private void HideTeeth()
    {
        for (int i = 0; i < usedTeeth; i++)
        {
            Teeth[i].Position = Vector2.Zero;
        }
    }
    
    private void EnableTeeth()
    {
        for (int i = 0; i < usedTeeth; i++)
        {
            hitbox.SetEnabled(true);
            Teeth[i].GlobalPosition = Teeth[i].BasePosition + outDirection * Utils.RandRange(-TeethVariance, TeethVariance);
            Teeth[i].hitbox.center = Teeth[i].GlobalPosition;
        }
    }

    private void DisableTeeth()
    {
        for (int i = 0; i < usedTeeth; i++)
        {
            hitbox.SetEnabled(false);
        }
    }

    // Places teeth on one of the walls
    private void PlaceTeeth()
    {
        FieldEdge edges = FieldEdge.Left | FieldEdge.Bottom | FieldEdge.Right;
        if (currentEdge != FieldEdge.Bottom)
            edges &= ~currentEdge;
        currentEdge = Utils.GenerateRandomEdge(edges);

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
                    Teeth[i].BasePosition = topEnd + (TeethSpacing / 2 + i * TeethSpacing) * Vector2.Down;
                    outDirection = Vector2.Right;

                    Teeth[i].Rotation = Mathf.Pi / 2;

                    usedTeeth = i + 1;
                }

            break;
            case FieldEdge.Right:
                topEnd = new Vector2(Consts.FieldRight, Consts.FieldTop) + Vector2.Left * 10;
                for (int i = Consts.FieldTop; i <= Consts.FieldBottom / 40 + 1; i++)
                {
                    Teeth[i].BasePosition = topEnd + (TeethSpacing / 2 + i * TeethSpacing) * Vector2.Down;
                    outDirection = Vector2.Left;

                    Teeth[i].Rotation = -Mathf.Pi / 2;

                    usedTeeth = i + 1;
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
                    Teeth[i].BasePosition = leftEnd + (TeethSpacing / 2 + i * TeethSpacing) * Vector2.Right;
                    outDirection = Vector2.Up;

                    Teeth[i].Rotation = 0;
                    
                    usedTeeth = i + 1;
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