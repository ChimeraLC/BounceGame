using System.Collections.Generic;

public class EnemyManager
{
    private HashSet<Enemy> enemies = new HashSet<Enemy>();
    private HashSet<Enemy> garbageEnemies = new HashSet<Enemy>();

    public void RegisterEnemy(Enemy newEnemy)
    {
        enemies.Add(newEnemy);
    }

    public void Tick(float delta)
    {
        foreach (Enemy enemy in enemies)
        {
            enemy.Tick(delta);
        }

        // Cleanup of dead enemies
        garbageEnemies.Clear();
        foreach (Enemy enemy in enemies)
        {
            if (!enemy.InPlay())
            {
                garbageEnemies.Add(enemy);
            }
        }
        foreach (Enemy garbageEnemy in garbageEnemies)
        {
            enemies.Remove(garbageEnemy);
            garbageEnemy.QueueFree();
        }
    }
}