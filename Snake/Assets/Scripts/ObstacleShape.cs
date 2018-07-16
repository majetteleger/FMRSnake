using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Obstacle Shape", menuName = "Obstacle Shape")]
public class ObstacleShape : ScriptableObject
{
    public enum Direction
    {
        Up,
        Right,
        Down,
        Left
    }

    public Direction[] ShapeDirections;
}
