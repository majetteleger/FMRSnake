using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float LifeTime;

    public GridCell Cell { get; set; }
    
    void Update ()
    {
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
        GridPlayground.Instance.ObstaclesSpawned--;
        Cell.Content = null;

        foreach (var cellTileSection in Cell.TileSections)
        {
            cellTileSection.Renderer.enabled = false;
        }

        Destroy(gameObject);
    }
}
