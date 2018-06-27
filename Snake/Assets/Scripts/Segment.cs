using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Segment : MonoBehaviour
{
    public float ScaleUpTime;
    public Transform Shape;
    
    public Segment PreviouSegment { get; set; }
    public Segment NextSegment { get; set; }
    public GridCell CurrentCell { get; set; }

    private void Start()
    {
        Shape.GetComponent<SpriteRenderer>().size = new Vector2(GridPlayground.Instance.CellSize, GridPlayground.Instance.CellSize);
        transform.DOScale(Vector3.one, ScaleUpTime).SetEase(Ease.OutBack, 2);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var gridCell = other.GetComponent<GridCell>();

        if (gridCell != null)
        {
            CurrentCell = gridCell;
        }
    }
    
    public void Move(Vector3 destination, bool mainMenu = true)
    {
        transform.DOMove(destination, MainManager.Instance.Player.MoveTime);

        if (NextSegment != null)
        {
            NextSegment.Move(transform.position);
        }
    }
    
    private GridCell GetPreviousSegmentCell()
    {
        return PreviouSegment.CurrentCell;
    }
}
