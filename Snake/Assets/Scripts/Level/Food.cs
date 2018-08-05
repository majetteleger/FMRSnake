using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    public Sprite[] Sprites;
    public SpriteRenderer SpriteRenderer;

    public Zone Zone { get; set; }
    public bool DontPulse { get; set; }

    private void Start()
    {
        SpriteRenderer.sprite = Sprites[Random.Range(0, Sprites.Length)];
        DontPulse = true;
    }
}
