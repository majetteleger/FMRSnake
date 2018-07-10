using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneIndicatorShield : MonoBehaviour
{
    public Rect GetRect()   // lol
    {
        var rectTransform = GetComponent<RectTransform>();
        var size = Vector2.Scale(rectTransform.rect.size, rectTransform.lossyScale);

        return new Rect((Vector2) rectTransform.position - (size * 0.5f), size);
    }
}