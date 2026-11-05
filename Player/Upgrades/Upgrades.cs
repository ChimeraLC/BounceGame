using System.Collections.Generic;
using Godot;

public enum Upgrade
{
    DamageUp, // Increases damage, repeatable
    JetControls, // Can use WASD to slightly control movement
    TimeSlow, // Can hold rmb to slow time
}

public struct UpgradeRequirement
{
    public bool unique;

    public UpgradeRequirement(bool inUnique)
    {
        unique = inUnique;
    }
}

public class UpgradeData
{
    public static Dictionary<Upgrade, (string name, string description)> Data =
        new Dictionary<Upgrade, (string name, string description)>
    {
        [Upgrade.DamageUp] = ("Damage Up", "Increases damage by 1"),
        [Upgrade.JetControls] = ("Jet Controls", "Use A and D to control in-air movement slightly."),
        [Upgrade.TimeSlow] = ("Time Slow", "Hold RMB to slow down time"),
    };

    public static Dictionary<Upgrade, UpgradeRequirement> UpgradeReqs = 
        new Dictionary<Upgrade, UpgradeRequirement>
    {
        [Upgrade.DamageUp] = new UpgradeRequirement(false),
        [Upgrade.JetControls] = new UpgradeRequirement(true),
        [Upgrade.TimeSlow] = new UpgradeRequirement(true),  
    };

    public static (string name, string description) GetData(Upgrade upgrade)
    {
        if (Data.ContainsKey(upgrade))
            return Data[upgrade];

        Logger.Log($"No upgrade data found for {upgrade}", LogLevel.error);
        return ("Missing Upgrade", "Missing Description");
    }

    // Returns if a given upgrade can be given TODO: Make this formulaic
    public static bool CanGiveUpgrade(Upgrade upgrade, UpgradeTracker tracker)
    {
        if (UpgradeReqs.ContainsKey(upgrade))
        {
            UpgradeRequirement requirement = UpgradeReqs[upgrade];
            // Unique requirement
            if (requirement.unique && tracker.HasUpgrade(upgrade))
            {
                return false;
            }

            return true;
        }
        else
        {
            Logger.Log($"Upgrade {upgrade} has no requirement data", LogLevel.error);
            return false;
        }
    }
}

public class UpgradeTracker
{
    Dictionary<Upgrade, int> ownedUpgrades;

    public UpgradeTracker()
    {
        ownedUpgrades = new Dictionary<Upgrade, int>();
    }

    public bool HasUpgrade(Upgrade upgrade)
    {
        return ownedUpgrades.ContainsKey(upgrade) && ownedUpgrades[upgrade] > 0;
    }

    public int GetUpgradeCount(Upgrade upgrade)
    {
        if (ownedUpgrades.ContainsKey(upgrade))
            return ownedUpgrades[upgrade];
        return 0;
    }

    public bool AddUpgrade(Upgrade upgrade)
    {
        if (HasUpgrade(upgrade))
        {
            ownedUpgrades[upgrade]++;
            return false;
        }

        ownedUpgrades[upgrade] = 1;
        return true;
    }
}