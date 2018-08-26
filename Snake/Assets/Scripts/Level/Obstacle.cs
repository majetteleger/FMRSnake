using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float LifeTime;

    public bool Permanent { get; set; }
    public GridCell Cell { get; set; }
    public bool DontPulse { get; set; }

    private void Start()
    {
        DontPulse = true;
    }

    void Update ()
    {
        if (Permanent)
        {
            return;
        }

		if (LifeTime > 0)
        {
            LifeTime -= Time.deltaTime;

            if (LifeTime <= 0)
            {
                Clear();
            }
        }

        if (!Cell.TileSections[0].Renderer.enabled)
        {
            Clear();
        }
	}

    public void Clear()
    {
        if (Permanent)
        {
            return;
        }

        DontPulse = true;

        GridPlayground.Instance.ObstaclesSpawned--;
        GridPlayground.Instance.Obstacles.Remove(this);

        Cell.Content = null;

        foreach (var cellTileSection in Cell.TileSections)
        {
            if (Cell.ZoneModifier == MainManager.Instance.GridPlayground.NoneZoneModifier)
            {
                cellTileSection.Renderer.DOFade(0f, MainManager.Instance.PulseTime).OnComplete(() => cellTileSection.Renderer.enabled = false);
            }
            else
            {
                cellTileSection.Renderer.DOColor(new Color(Cell.ZoneModifier.Color.r, Cell.ZoneModifier.Color.g, Cell.ZoneModifier.Color.b, Cell.ColorAlpha), MainManager.Instance.PulseTime);
            }
        }
        
        transform.DOScale(0f, MainManager.Instance.PulseTime).OnComplete(() => Destroy(gameObject));
    }
}