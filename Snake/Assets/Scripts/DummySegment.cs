using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DummySegment : MonoBehaviour
{
    public enum UpdateType
    {
        Normal,
        HorizontalToVertical,
        VerticalToHorizontal
    }

    private Segment _frontSegment;
    private Segment _backSegment;
    private Vector3 _direction;
    private Vector3 _midPoint;
    private Vector3 _previousFrontSegmentPosition;
    private Vector3 _previousVectorFromMidPoint;
    private bool _directionUpdated;
    private bool _initialized;

    public void Initialize(Segment frontSegment, Segment backSegment, int intermediateIndex, int intermediateCount, float moveTime)
    {
        _frontSegment = frontSegment;
        _backSegment = backSegment;
        
        if (MainManager.Instance.CurrentState != MainManager.GameState.Play)
        {
            var frontSegmentPosition = _frontSegment.transform.position + Vector3.right * MainManager.Instance.GridPlayground.MoveDistance;
            var backSegmentPosition = _backSegment.transform.position;

            var computedPosition = backSegmentPosition + (((frontSegmentPosition - backSegmentPosition) / (float)(intermediateCount + 1)) * (intermediateIndex + 1));

            var movement = transform.DOMove(computedPosition, moveTime);
            movement.onComplete += () =>
            {
                _initialized = true;
            };
        }
    }

    public void UpdatePosition(Vector3 frontSegmentPosition, Vector3 backSegmentPosition, int intermediateIndex, int intermediateCount, UpdateType updateType)
    {
        if (!_initialized)
        {
            return;
        }
        
        var distanceTraveledByFrontSegment = (_frontSegment.transform.position - _previousFrontSegmentPosition).magnitude;
        _previousFrontSegmentPosition = _frontSegment.transform.position;

        transform.position += _direction * distanceTraveledByFrontSegment;

        var vectorFromMidPoint = (_midPoint - transform.position).normalized;

        if (!_directionUpdated && vectorFromMidPoint != _previousVectorFromMidPoint)
        {
            transform.position = _midPoint;

            _direction = Vector3.Normalize(frontSegmentPosition - transform.position);
            _directionUpdated = true;
        }
        else
        {
            _previousVectorFromMidPoint = vectorFromMidPoint;
        }
    }

    public void InitializeMove()
    {
        _initialized = true;

        _direction = Vector3.Normalize(_frontSegment.transform.position - transform.position);
        _midPoint = _frontSegment.transform.position;
        _previousFrontSegmentPosition = _frontSegment.transform.position;
        _previousVectorFromMidPoint = (_midPoint - transform.position).normalized;
    }

    public void SetForNextMove(int intermediateIndex, int intermediateCount)
    {
        _initialized = true;

        var frontSegmentPosition = _frontSegment.transform.position;
        var backSegmentPosition = _backSegment.transform.position;

        transform.position = backSegmentPosition + (((frontSegmentPosition - backSegmentPosition) / (float)(intermediateCount + 1)) * (intermediateIndex + 1));

        _directionUpdated = false;
    }
}
