using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zone : MonoBehaviour
{
    public GridCell[] GridCells { get; set; }
    public List<Food> FoodObjects { get; set; }
    public ZoneModifier ZoneModifier { get; set; }

    public void Initialize(GridCell[] gridCells, ZoneModifier zoneModifier)
    {
        GridCells = gridCells;
        ZoneModifier = zoneModifier;
        FoodObjects = new List<Food>();
    }
    
    public void TryClear()
    {
        if (FoodObjects.Count == 0)
        {
            Clear();
        }
    }

    public void Clear()
    {
        foreach (var cell in GridCells)
        {
            cell.ZoneModifier = MainManager.Instance.GridPlayground.NoneZoneModifier;
            cell.Modify();
        }

        MainManager.Instance.GridPlayground.ZonesSpawned--;

        MainManager.Instance.Player.GiveScore(ZoneModifier.ClearBaseScore, true, true);
    }
}
