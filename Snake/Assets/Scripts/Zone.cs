using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zone : MonoBehaviour
{
    public GridCell[] GridCells { get; set; }
    public List<Food> FoodObjects { get; set; }

    public void Initialize(GridCell[] gridCells)
    {
        GridCells = gridCells;
        FoodObjects = new List<Food>();
    }
    
    public void TryClear()
    {
        if (FoodObjects.Count == 0)
        {
            foreach (var cell in GridCells)
            {
                cell.ZoneModifier = MainManager.Instance.GridPlayground.NoneZoneModifier;
                cell.Modify();

                MainManager.Instance.GridPlayground.ZonesSpawned--;

                GiveScore();
            }
        }
    }

    public void GiveScore()
    {
        Debug.Log("Implement scoring");
    }
}
