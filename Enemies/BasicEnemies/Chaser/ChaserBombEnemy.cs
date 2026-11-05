using Godot;

// Version of the chaser enemy that explodes once within distance
public class ChaserBombEnemy : ChaserEnemy
{
    protected float explosionRadius = 200;
    protected float triggerRadius = 80;

    private Node2D explosionDisplay;

    public override void _Ready()
    {
        base._Ready();

        explosionDisplay = GetNode<Node2D>("ExplosionDisplay");
        explosionDisplay.Scale = new Vector2(triggerRadius, triggerRadius);
        explosionDisplay.Modulate = new Color(1, 1, 1, 0.3f);
    }

    protected override void TickMainPlayer(float delta)
    {
        switch (state)
        {
        case ChaserState.Spawning: // Pop out of the wall
            TickSpawning(delta);
            break;
        case ChaserState.Chasing:
            TickChasing(delta);
            break;
        case ChaserState.Exploding:
            TickExploding(delta);
            break;
        }
    }
    protected override  void TickChasing(float delta)
    {
        if (Player.Instance != null)
        {
            Vector2 offset = Player.Instance.GlobalPosition - GlobalPosition;
            facingAngle = Utils.MoveTowardsAngle(facingAngle, offset.Angle(), delta * turnSpeed).Item1;

            Rotation = facingAngle;

            Position += new Vector2(Mathf.Cos(facingAngle), Mathf.Sin(facingAngle)) * delta * moveSpeed;

            if (offset.LengthSquared() < triggerRadius * triggerRadius)
            {
                spawnTimer = 0;
                state = ChaserState.Exploding;
                
                explosionDisplay.Scale = new Vector2(explosionRadius, explosionRadius);
                explosionDisplay.Modulate = new Color(1, 1, 1, 1f);
            }
        }
    }

    protected virtual void TickExploding(float delta)
    {
        spawnTimer += delta;

        Modulate = new Color(1, 1, 1, spawnTimer % 0.4f > 0.2f ? 1 : 0);

        if (spawnTimer >= 3)
        {
            Suicide();
            if (Player.Instance != null)
            {
                Vector2 playerLocation = Player.Instance.GlobalPosition;

                if ((playerLocation - GlobalPosition).LengthSquared() < explosionRadius * explosionRadius)
                {
                    Player.Instance.TakeDamage(1);
                }

                state = ChaserState.None;
            }
        }
    }

    // TODO: Display trigger and explosion radiuses
}