using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    public Sprite[] Sprites;

    public Zone Zone { get; set; }

    private void Start()
    {
        GetComponent<SpriteRenderer>().sprite = Sprites[Random.Range(0, Sprites.Length)];
    }
}
