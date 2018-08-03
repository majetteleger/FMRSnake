using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zone : MonoBehaviour
{
    public GameObject IndicatorPrefab;

    public GridCell[] GridCells { get; set; }
    public List<Food> FoodObjects { get; set; }
    public ZoneModifier ZoneModifier { get; set; }

    private ZoneIndicator _indicator;

    public void Initialize(GridCell[] gridCells, ZoneModifier zoneModifier)
    {
        GridCells = gridCells;
        ZoneModifier = zoneModifier;
        FoodObjects = new List<Food>();

        _indicator = Instantiate(IndicatorPrefab, MainPanel.Instance.transform).GetComponent<ZoneIndicator>();
        _indicator.Initialize(this);
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
            //if (cell.Content == MainManager.Instance.Player)
            //{
            //    MainPanel.Instance.BeatIndicator.UpdateBarAtNextBeat = true;
            //}

            cell.ZoneModifier = MainManager.Instance.GridPlayground.NoneZoneModifier;
            cell.Modify();
        }

        MainPanel.Instance.BeatIndicator.UpdateBarAtNextBeat = true;

        MainManager.Instance.GridPlayground.ZonesSpawned--;
        MainManager.Instance.Player.GiveScore(ZoneModifier.ClearBaseScore, true, true);

        Destroy(_indicator);
        _indicator = null;
    }

    public bool IsVisibleOnScreen()
    {
        foreach (var cell in GridCells)
        {
            var screenPoint = Camera.main.WorldToViewportPoint(cell.transform.position);

            if (screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1 && screenPoint.z > 0)
            {
                return true;
            }
        }

        return false;
    }
}
