using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummySegment : MonoBehaviour
{
    public void UpdatePosition(Segment frontSegment, Segment backSegment, int intermediateIndex, int intermediateCount)
    {
        var frontSegmentPosition = frontSegment.transform.position;
        var backSegmentPosition = backSegment.transform.position;
        
        // Find a way to make it do the corner turn

        var position = backSegmentPosition + (((frontSegmentPosition - backSegmentPosition) / (float)(intermediateCount + 1)) * (intermediateIndex + 1));

        transform.position = position;
    }
}
