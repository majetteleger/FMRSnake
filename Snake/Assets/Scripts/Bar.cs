using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TEST
[CreateAssetMenu(fileName = "New Bar", menuName = "Bar")]
public class Bar : ScriptableObject
{
    public List<Beat> Beats;
}

[System.Serializable]
public struct Beat
{
    public bool IsHigh;
    public float Delay;
}
