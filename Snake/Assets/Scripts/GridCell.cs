using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell : MonoBehaviour
{
    public float ColorAlpha;

    public ZoneModifier ZoneModifier;// { get; set; }
    public GameObject Content { get; set; }

    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        ZoneModifier = MainManager.Instance.GridPlayground.NoneZoneModifier;
        Modify();
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
            _spriteRenderer.color = new Color(ZoneModifier.Color.r, ZoneModifier.Color.g, ZoneModifier.Color.b, ZoneModifier.Color.a);
        }
    }
}