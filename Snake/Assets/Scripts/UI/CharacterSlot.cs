using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSlot : MonoBehaviour
{
    public bool IsConfirm;
    public bool IsBack;
    public bool IsOn;
    public Image UpArrow;
    public Image DownArrow;
    public Text Character;
    public Image Outline;
    public Color HighlightedColor;
    public Color NormalColor;

    public void Toggle(bool toggle)
    {
        Outline.color = toggle ? HighlightedColor : NormalColor;
    }
}
