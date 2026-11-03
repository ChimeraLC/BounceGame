// Basic enemy that just latches onto the wall and damages if you hit it

using Godot;

public partial class TumorStaticEnemy : Enemy
{
    CircleHitbox hitbox;
    Node2D tumorBody;

    protected int minimumSize = 30;
    protected int maximumSize = 100;
    protected int chosenSize;

    public override void _Ready()
    {
        base._Ready();

        chosenSize = rand.Next() % (maximumSize - minimumSize) + minimumSize;    
        hitbox = new CircleHitbox(this, Position, chosenSize);

        tumorBody = GetNode<Node2D>("TumorBody");
        tumorBody.Scale = new Vector2(chosenSize, chosenSize);

        // TODO: Maybe vary health with size?
        health = 2;
    }

    public override Vector2 GetSpawnpoint()
    {
        return Utils.GenerateRandomEdgepoint(48, -5, FieldEdge.Left | FieldEdge.Right | FieldEdge.Bottom).Item1;
    }

    public override void UpdateHitbox() {} // Doesn't move

    public override Hitbox GetHitbox()
    {
        return hitbox;
    }

    protected override void OnDeath()
    {
        CollisionManager.RemoveHitbox(hitbox);
    }
}