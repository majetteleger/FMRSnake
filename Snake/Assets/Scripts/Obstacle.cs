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
                GridPlayground.Instance.ObstaclesSpawned--;
                Cell.Content = null;
                Destroy(gameObject);
            }
        }
	}
}
