using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DummySegment : MonoBehaviour
{
    public MeshFilter Quad;
    
    public void Initialize(Segment frontSegment, Segment backSegment, int intermediateIndex, int intermediateCount, float moveTime, AnimationCurve moveCurve)
    {
        if (MainManager.Instance.CurrentState != MainManager.GameState.Play || MainManager.Instance.CurrentState != MainManager.GameState.BuildYourSnake)
        {
            transform.position = frontSegment.transform.position;
        }
    }

    public void UpdatePosition(Vector3 frontSegmentPosition, Vector3 backSegmentPosition, int intermediateIndex, int intermediateCount)
    {
        var computedPosition = backSegmentPosition + (((frontSegmentPosition - backSegmentPosition) / (float)(intermediateCount + 1)) * (intermediateIndex));
        transform.position = computedPosition;
    }
}
