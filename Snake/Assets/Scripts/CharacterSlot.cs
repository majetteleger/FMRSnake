using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSlot : MonoBehaviour
{
    public bool IsConfirm;
    public Image UpArrow;
    public Image DownArrow;
    public Text Character;
    public Image Outline;
    
    public void Toggle(bool toggle)
    {
        Outline.enabled = toggle;
    }
}
