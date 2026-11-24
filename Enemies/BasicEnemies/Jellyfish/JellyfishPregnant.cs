using Godot;

// Version of the jellyfish that spawns ticks on death
public class JellyfishPregnant : Jellyfish
{
    protected override void OnDeath()
    {
        base.OnDeath();

        int tickCount = Utils.RandRange(3, 5);

        float divideAngle = 2 * Mathf.Pi / tickCount;
        for (int i = 0; i < tickCount; i++)
        {
            Enemy newEnemy = EnemyManager.RequestEnemy(EnemyType.TickEnemy);
            if (newEnemy is TickEnemy newTick)
            {
                newTick.Initialize(Position, Utils.AngleToVec(
                    divideAngle * i + Utils.RandRange(0, Mathf.Pi)
                ));
            }
        }
    }
}