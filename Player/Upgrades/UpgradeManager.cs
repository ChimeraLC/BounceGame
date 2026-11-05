using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class UpgradeManager : Control
{
    [Export]
    public PackedScene UpgradeCardPrefab;
    private Random rand = new Random();

    public static UpgradeManager Instance {get; private set;}
    private Control upgradeCenter;

    private HashSet<UpgradeCard> activeCards;
    private bool chosenComplete = false;


    public override void _Ready()
    {
        upgradeCenter = GetNode<Control>("UpgradeCenter");
        activeCards = new HashSet<UpgradeCard>();

        Instance = this;
    }

    public virtual void Tick(float delta)
    {
        foreach (UpgradeCard card in activeCards)
            card.Tick(delta);

        if (chosenComplete)
        {    
            // Despawn all cards TODO: Cool animation here probably
            foreach (UpgradeCard card in activeCards)
            {
                card.QueueFree();
            }

            activeCards.Clear();

            chosenComplete = false;
        }
    }

    public static bool IsActive()
    {
        return Instance != null && Instance.activeCards.Count > 0;
    }

    // TODO: Choose upgrades at random depending on prereqs and stuff like that
    // TODO: Should block other input
    public void ShowUpgrades()
    {
        HashSet<Upgrade> possibleUpgrades = new HashSet<Upgrade>();

        // TODO: Does this need to check that player exists? Should only run while they're alive
        foreach (Upgrade upgrade in Enum.GetValues(typeof(Upgrade)))
        {
            if (UpgradeData.CanGiveUpgrade(upgrade, Player.Instance.upgradeTracker))
            {
                possibleUpgrades.Add(upgrade);
            }
        }

        // For now, default to 3
        int upgradeNum = Math.Min(possibleUpgrades.Count, 3);
        Upgrade[] shownUpgrades = new Upgrade[upgradeNum];
        for (int i = 0; i < upgradeNum; i++)
        {
            int index = rand.Next() % possibleUpgrades.Count;
            Upgrade chosenUpgrade = possibleUpgrades.ElementAt(index);

            possibleUpgrades.Remove(chosenUpgrade);
            shownUpgrades[i] = chosenUpgrade;
        }

        // Choose 3 random upgrades that are possible TODO: Logic can probably be streamlined
        DisplayUpgrades(shownUpgrades);
        chosenComplete = false;
    }

    public void DisplayUpgrades(Upgrade[] upgrades)
    {
        Logger.Log($"Showing upgrades: {string.Join(", ", upgrades)}", LogLevel.info);
        Vector2[] positions = {};
        switch (upgrades.Length)
        {
            case 0:
            Logger.Log("DisplayUpgrades() called with no upgrades", LogLevel.error);
            return;
            case 1:
            positions = new Vector2[]{Vector2.Zero};
            break;
            case 2:
            positions = new Vector2[]{Vector2.Left * 150, Vector2.Right * 150};
            break;
            case 3:
            positions = new Vector2[]{Vector2.Left * 300, Vector2.Zero, Vector2.Right * 300};
            break;
            case 4:
            positions = new Vector2[]{Vector2.Left * 450, Vector2.Left * 150, Vector2.Right * 150, Vector2.Right * 450};
            break;
            default:
            Logger.Log($"DisplayUpgrades() not supported for {upgrades.Length} upgrades", LogLevel.error);
            break;
        }

        for (int i = 0; i < positions.Length; i++)
        {
            PlaceUpgradeCard(positions[i], upgrades[i]);
        }
    }

    private void PlaceUpgradeCard(Vector2 offset, Upgrade upgrade)
    {
        UpgradeCard newCard = UpgradeCardPrefab.Instance<UpgradeCard>();

        newCard.SetPosition(offset);
        newCard.Initialize(upgrade);
        upgradeCenter.AddChild(newCard);

        activeCards.Add(newCard);
    }

    public void SelectCard(Upgrade upgrade)
    {
        GiveUpgrade(upgrade);
        chosenComplete = true; // Clear cards on next tick
    }

    public void GiveUpgrade(Upgrade upgrade)
    {
        Logger.Log($"Giving upgrade: {upgrade}", LogLevel.info);
        // Edge cases
        // switch (upgrade)
        // {
        // }
        
        Player.Instance.upgradeTracker.AddUpgrade(upgrade);
    }
}