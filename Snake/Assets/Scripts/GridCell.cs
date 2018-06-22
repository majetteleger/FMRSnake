using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell : MonoBehaviour
{
    public List<ZoneModifier> ZoneModifiers;

    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        ZoneModifiers = new List<ZoneModifier>();

        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Modify(ZoneModifier zoneModifier)
    {
        _spriteRenderer.color = zoneModifier.Color;
    }
}
