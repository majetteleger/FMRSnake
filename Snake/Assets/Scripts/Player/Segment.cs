using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Segment : MonoBehaviour
{
    public Transform Shape;
    public SpriteRenderer Center;
    public MeshFilter Quad;

    public Segment PreviouSegment { get; set; }
    public Segment NextSegment { get; set; }
    public GridCell CurrentCell { get; set; }
    public DummySegment[] FrontDummySegments { get; set; }
    
    private void Start()
    {
        Shape.GetComponent<SpriteRenderer>().size = new Vector2(GridPlayground.Instance.CellSize, GridPlayground.Instance.CellSize);
    }

    private void Update()
    {
        if (transform.hasChanged && FrontDummySegments != null)
        {
            for (var i = 0; i < FrontDummySegments.Length; i++)
            {
                var frontDummySegment = FrontDummySegments[i];

                frontDummySegment.UpdatePosition(
                    PreviouSegment.transform.position, 
                    transform.position, 
                    i, 
                    MainManager.Instance.Player.IntermediateSegments
                );
            }

            transform.hasChanged = false;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        var gridCell = other.GetComponent<GridCell>();

        if (gridCell != null)
        {
            CurrentCell = gridCell;
            CurrentCell.Content = gameObject;
        }
    }

    public bool TryShowCenter(float probability)
    {
        if (PreviouSegment == null || !PreviouSegment.Center.enabled && UnityEngine.Random.Range(0f, 1f) < probability)
        {
            Center.enabled = true;
        }

        return Center.enabled;
    }

    public void Move(Vector3 destination, float time, bool mainMenu = true)
    {
        transform.DOMove(destination, time).SetEase(MainManager.Instance.Player.MoveCurve);
        
        if (NextSegment != null)
        {
            NextSegment.Move(transform.position, time);
        }
    }
}
