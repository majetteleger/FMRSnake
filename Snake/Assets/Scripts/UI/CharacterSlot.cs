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

    public void ResetSlot()
    {
        StopAllCoroutines();

        UpArrow.color = NormalColor;
        DownArrow.color = NormalColor;
    }

    public void Toggle(bool toggle)
    {
        Outline.color = toggle ? HighlightedColor : NormalColor;
    }

    public void PressUp()
    {
        StartCoroutine(DoPress(UpArrow));
    }

    public void PressDown()
    {
        StartCoroutine(DoPress(DownArrow));
    }

    private IEnumerator DoPress(Image image)
    {
        image.color = HighlightedColor;

        yield return new WaitForSeconds(0.05f);

        image.color = NormalColor;
    }
}
