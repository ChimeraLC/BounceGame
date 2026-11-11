using System;
using System.Collections.Generic;
using Godot;

public enum EnemyType
{
    TumorStatic,
    TumorShooter,
    ChaserBase,
    ChaserExplode,
    BurrowerBasic,
    CrawlerBasic,
    UpgradeOrb,
}

public class EnemyManager
{
    public static EnemyManager Instance {get; private set;}
    private HashSet<Enemy> enemies = new HashSet<Enemy>();
    private HashSet<Enemy> queuedEnemies = new HashSet<Enemy>();
    private HashSet<Enemy> garbageEnemies = new HashSet<Enemy>();
    private Dictionary<EnemyType, PackedScene> enemyPrefabs = new Dictionary<EnemyType, PackedScene>();
    private static Dictionary<EnemyType, string> prefabPaths = new Dictionary<EnemyType, string>()
    {
        [EnemyType.ChaserBase] = "res://Enemies/BasicEnemies/Chaser/ChaserEnemy.tscn",
        [EnemyType.ChaserExplode] = "res://Enemies/BasicEnemies/Chaser/ChaserBombEnemy.tscn",
        [EnemyType.TumorStatic] = "res://Enemies/BasicEnemies/Tumor/TumorStaticEnemy.tscn",
        [EnemyType.TumorShooter] = "res://Enemies/BasicEnemies/Tumor/TumorShooterEnemy.tscn",
        [EnemyType.UpgradeOrb] = "res://Player/Upgrades/UpgradeOrb.tscn",
        [EnemyType.BurrowerBasic] = "res://Enemies/BasicEnemies/Burrower/BurrowerEnemy.tscn",
        [EnemyType.CrawlerBasic] = "res://Enemies/BasicEnemies/Crawler/CrawlerEnemy.tscn",
    };

    public EnemyManager()
    {
        Instance = this;
    }

    private static Dictionary<EnemyType, int> xpValues = new Dictionary<EnemyType, int>()
    {
        [EnemyType.ChaserBase] = 2,
        [EnemyType.ChaserExplode] = 1,
        [EnemyType.TumorStatic] = 1,
        [EnemyType.TumorShooter] = 2,
        [EnemyType.UpgradeOrb] = 0,
        [EnemyType.BurrowerBasic] = 2,
        [EnemyType.CrawlerBasic] = 3,
    };

    public void RegisterEnemy(Enemy newEnemy)
    {
        // Queues the enemy registering in case one is spawned during a tick
        queuedEnemies.Add(newEnemy);
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
            SpawnEnemy(EnemyType.CrawlerBasic);
            
        if (Input.IsActionJustPressed("debug_3"))
            SpawnEnemy(EnemyType.TumorStatic);

        if (Input.IsActionJustPressed("debug_4"))
            SpawnEnemy(EnemyType.BurrowerBasic);
    }

    private void TickEnemies(float delta)
    {
        // Add in queued enemies
        enemies.UnionWith(queuedEnemies);
        queuedEnemies.Clear();
        
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

    // TODO: Have some legality checks
    public static void RequestEnemy(EnemyType enemyType)
    {
        if (Instance != null)
            Instance.SpawnEnemy(enemyType);
    }

    private void SpawnEnemy(EnemyType enemyType)
    {
        PackedScene scene = GetPrefab(enemyType);
        if (scene != null)
        {
            Enemy newEnemy = scene.Instance<Enemy>();
            newEnemy.GlobalPosition = newEnemy.GetSpawnpoint();
            newEnemy.xpWorth = GetXPWorth(enemyType);
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
                enemyPrefabs[enemyType] = scene;
                return scene;
            }
        }

        Logger.Log($"Enemy type {enemyType} has no set prefabs", LogLevel.error);
        return null;
    }

    private int GetXPWorth(EnemyType enemyType)
    {
        if (xpValues.ContainsKey(enemyType))
            return xpValues[enemyType];

        Logger.Log($"Enemy type {enemyType} has no xp data", LogLevel.error);
        return 0;
    }

    // Checks that xp and prefabs are all valid
    public void VerifyData()
    {
        foreach (EnemyType enemyType in Enum.GetValues(typeof(EnemyType)))
        {
            GetPrefab(enemyType);

            if (!xpValues.ContainsKey(enemyType))
                Logger.Log($"Enemy type {enemyType} missing xp value.", LogLevel.error);
        }
    }
}