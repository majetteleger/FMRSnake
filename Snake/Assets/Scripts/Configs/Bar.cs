using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Bar", menuName = "Bar")]
public class Bar : ScriptableObject
{
    public bool[] Beats = new bool[8];
}

[System.Serializable]
public struct Beat
{
    
}
