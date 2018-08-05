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
            cellTileSection.Renderer.DOFade(0f, MainManager.Instance.PulseTime).OnComplete(() => cellTileSection.Renderer.enabled = false);
        }

        transform.DOScale(0f, MainManager.Instance.PulseTime).OnComplete(() => Destroy(gameObject));
    }
}