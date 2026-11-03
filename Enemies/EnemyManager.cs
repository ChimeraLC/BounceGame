using System.Collections.Generic;
using Godot;

public enum EnemyType
{
    TumorStatic,
    ChaserBase,
    ChaserExplode,
}

public class EnemyManager
{
    private HashSet<Enemy> enemies = new HashSet<Enemy>();
    private HashSet<Enemy> garbageEnemies = new HashSet<Enemy>();
    private Dictionary<EnemyType, PackedScene> enemyPrefabs = new Dictionary<EnemyType, PackedScene>();
    private static Dictionary<EnemyType, string> prefabPaths = new Dictionary<EnemyType, string>()
    {
        [EnemyType.ChaserBase] = "res://Enemies/BasicEnemies/Chaser/ChaserEnemy.tscn",
        [EnemyType.ChaserExplode] = "res://Enemies/BasicEnemies/Chaser/ChaserBombEnemy.tscn",
        [EnemyType.TumorStatic] = "res://Enemies/BasicEnemies/Tumor/TumorStaticEnemy.tscn",
    };

    public void RegisterEnemy(Enemy newEnemy)
    {
        enemies.Add(newEnemy);
    }

    public void Tick(float delta)
    {
        TickSpawning(delta);
        TickEnemies(delta);
    }

    private void TickSpawning(float delta)
    {
        if (Input.IsActionJustPressed("debug_1"))
            SpawnEnemy(EnemyType.ChaserBase);
    
    
        if (Input.IsActionJustPressed("debug_2"))
            SpawnEnemy(EnemyType.ChaserExplode);
            
        if (Input.IsActionJustPressed("debug_3"))
            SpawnEnemy(EnemyType.TumorStatic);
    }

    private void TickEnemies(float delta)
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

    private void SpawnEnemy(EnemyType enemyType)
    {
        PackedScene scene = GetPrefab(enemyType);
        if (scene != null)
        {
            Enemy newEnemy = scene.Instance<Enemy>();
            newEnemy.GlobalPosition = newEnemy.GetSpawnpoint();
            GameManager.WorldRoot.AddChild(newEnemy);
        }
    }

    private PackedScene GetPrefab(EnemyType enemyType)
    {
        if (enemyPrefabs.ContainsKey(enemyType))
            return enemyPrefabs[enemyType];

        if (prefabPaths.ContainsKey(enemyType))
        {
            PackedScene scene = GD.Load<PackedScene>(prefabPaths[enemyType]);
            if (scene != null)
            {
                enemyPrefabs[EnemyType.ChaserBase] = scene;
                return scene;
            }
        }

        Logger.Log($"Enemy type {enemyType} has no set prefabs", LogLevel.error);
        return null;
    }
}