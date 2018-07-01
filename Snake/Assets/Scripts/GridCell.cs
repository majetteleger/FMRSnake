using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell : MonoBehaviour
{
    public ZoneModifier ZoneModifier;
    public GameObject Content;

    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == Content)
        {
            Content = null;
        }
    }

    public void Modify()
    {
        if (ZoneModifier != null)
        {
            _spriteRenderer.color = ZoneModifier.Color;
        }
    }
}