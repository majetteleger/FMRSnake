using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New ZoneModifier", menuName = "ZoneModifier")]
public class ZoneModifier : ScriptableObject
{
    public Color Color;
    public float Radius;
    public int FoodSpawnCount;
    public int ClearBaseScore;
    public int FoodScore;
    public Bar Bar;
    public AudioClip Ambiance;
}
