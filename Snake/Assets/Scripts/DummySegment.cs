using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummySegment : MonoBehaviour
{
    public enum UpdateType
    {
        Normal,
        HorizontalToVertical,
        VerticalToHorizontal
    }

    public void UpdatePosition(Vector3 frontSegmentPosition, Vector3 backSegmentPosition, int intermediateIndex, int intermediateCount, UpdateType updateType)
    {
        // Find a way to make it do the corner turn
    
        var computedPosition = backSegmentPosition + (((frontSegmentPosition - backSegmentPosition) / (float)(intermediateCount + 1)) * (intermediateIndex + 1));
        transform.position = computedPosition;

        /*if (updateType == UpdateType.VerticalToHorizontal)
        {
            if (!Mathf.Approximately(computedPosition.y, frontSegmentPosition.y))
            { 
                newPosition.y = computedPosition.y;
            }
            else
            {
                newPosition.x = computedPosition.x;
            }
        }
        else if(updateType == UpdateType.HorizontalToVertical)
        {
            if (!Mathf.Approximately(computedPosition.x, frontSegmentPosition.x))
            {
                newPosition.x = computedPosition.x;
            }
            else
            {
                newPosition.y = computedPosition.y;
            }
        }
        else if(updateType == UpdateType.Normal)
        {
            newPosition = computedPosition;
        }*/
    }
}
