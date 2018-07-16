using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float LifeTime;
    
	void Update ()
    {
		if (LifeTime > 0)
        {
            LifeTime -= Time.deltaTime;

            if (LifeTime <= 0)
            {
                GridPlayground.Instance.ObstaclesSpawned--;
                Destroy(gameObject);

                // SHOULD WE ALSO NOTIFY THE CELL THAT ITS CONTENT IS DESTROYED?
            }
        }
	}
}
