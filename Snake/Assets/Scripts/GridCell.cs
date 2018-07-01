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

    public void Modify(ZoneModifier zoneModifier)
    {
        _spriteRenderer.color = zoneModifier.Color;
    }
}
