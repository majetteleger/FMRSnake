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
    
    public bool TryClear()
    {
        if (FoodObjects.Count == 0)
        {
            Clear();
            return true;
        }
        return false;
    }

    public void Clear()
    {
        foreach (var cell in GridCells)
        {
            cell.ZoneModifier = MainManager.Instance.GridPlayground.NoneZoneModifier;
            cell.Modify();
        }

        MainPanel.Instance.BeatIndicator.UpdateBarAtNextBeat = true;

        MainManager.Instance.GridPlayground.ZonesSpawned--;
        MainManager.Instance.Player.GiveScore(ZoneModifier.ClearBaseScore, true, true);

        Destroy(_indicator);
        _indicator = null;

        Destroy(gameObject);
    }

    public void Delete()
    {
        foreach (var cell in GridCells)
        {
            cell.ZoneModifier = MainManager.Instance.GridPlayground.NoneZoneModifier;
            cell.Modify();
        }

        MainManager.Instance.GridPlayground.ZonesSpawned--;

        Destroy(_indicator);
        _indicator = null;

        Destroy(gameObject);
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
