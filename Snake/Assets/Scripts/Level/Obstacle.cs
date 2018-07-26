using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float LifeTime;

    public bool Permanent { get; set; }
    public GridCell Cell { get; set; }
    
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

        GridPlayground.Instance.ObstaclesSpawned--;
        Cell.Content = null;

        foreach (var cellTileSection in Cell.TileSections)
        {
            cellTileSection.Renderer.enabled = false;
        }

        Destroy(gameObject);
    }
}
