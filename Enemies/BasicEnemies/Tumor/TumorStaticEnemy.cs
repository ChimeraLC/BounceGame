// Basic enemy that just latches onto the wall and damages if you hit it

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class TumorStaticEnemy : Enemy
{
    [Export]
    public Texture tumorTexture;
    CircleHitbox hitbox;
    Node2D tumorBody;

    protected int minHealth = 2;
    protected int maxHealth = 5;
    protected int chosenSize;
    protected float spriteSize;
    protected float age;

    protected Vector2 spawnDirection;
    protected List<Sprite> tumorSprites = new List<Sprite>();
    protected List<float> tumorOffsets = new List<float>();

    public override void _Ready()
    {
        base._Ready();

        chosenSize = Utils.RandRange(minHealth, maxHealth);   

        spriteSize = GetHitboxSize(chosenSize);

        tumorBody = GetNode<Node2D>("TumorBody");
        tumorBody.Scale = new Vector2(spriteSize, spriteSize);
        hitbox = new CircleHitbox(this, Position, spriteSize);

        Vector2 perp = new Vector2(-spawnDirection.y, spawnDirection.x);

        // Place pulsating health indicators
        for (int i = 0; i < chosenSize; i++)
        {
            Sprite newSprite = new Sprite
            {
                Texture = tumorTexture
            };
            tumorSprites.Add(newSprite);
            AddChild(newSprite);

            float horizontalPosition = -spriteSize + 2 * spriteSize * (i+1) / (chosenSize + 1);
            horizontalPosition += Utils.RandRange(-5, 5);
            float verticalPosition = Utils.RandRange(0, Utils.TriangleLeg(horizontalPosition, spriteSize) - 10);
            newSprite.Position = spawnDirection * verticalPosition + perp * horizontalPosition;

            tumorOffsets.Add(Utils.RandRange(0, 2 * Mathf.Pi));
        }

        // TODO: Maybe vary health with size?
        health = chosenSize + 1;
    }

    protected override void TickAnimation(float delta)
    {
        age += delta * 2;
        for (int i = 0; i < tumorSprites.Count; i++)
        {
            tumorSprites[i].Scale = (0.4f + Mathf.Max(0, Mathf.Cos(age + tumorOffsets[i]) - 0.9f)) * Vector2.One;
        }
        tumorBody.Scale = spriteSize * (1 + Mathf.Max(0, Mathf.Cos(age) - 0.9f) / 2) * Vector2.One;
    }

    private float GetHitboxSize(int size)
    {
        return size * 10 + 30;
    }

    public override Vector2 GetSpawnpoint()
    {
        (Vector2 position, FieldEdge edge) = Utils.GenerateRandomEdgepoint(48, -5, FieldEdge.Left | FieldEdge.Right | FieldEdge.Bottom);
        spawnDirection = Utils.GetEdgeNormal(edge);
        return position;
    }

    protected override void OnTakeDamage(float damage, DamageType damageType = DamageType.Ballistic)
    {
        while (tumorSprites.Count > health -1 && tumorSprites.Count > 0)
        {
            Sprite removedSprite = tumorSprites.Last();
            tumorSprites.RemoveAt(tumorSprites.Count - 1);
            removedSprite.QueueFree();
        }
    }

    public override void UpdateHitbox() {} // Doesn't move

    public override Hitbox GetHitbox()
    {
        return hitbox;
    }
}