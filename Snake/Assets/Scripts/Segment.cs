using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Segment : MonoBehaviour
{
    public float ScaleUpTime;
    public Transform Shape;

    public GridCell CurrentCell { get; set; }
    public Player Player { get; set; }
    public Vector3 LastDirection { get; set; }

    private void Start()
    {
        transform.DOScale(Vector3.one, ScaleUpTime).SetEase(Ease.OutBack, 2);

        Shape.localPosition = new Vector3(-((GridPlayground.Instance.CellSize + GridPlayground.Instance.CellSpacing) / 2f), 0f, 0f);
        Shape.GetComponent<SpriteRenderer>().size = new Vector2(GridPlayground.Instance.CellSize * 2 + GridPlayground.Instance.CellSpacing, GridPlayground.Instance.CellSize);

        LastDirection = Vector3.right;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var gridCell = other.GetComponent<GridCell>();

        if (gridCell != null)
        {
            CurrentCell = gridCell;
        }
    }
    
    public void Move(bool mainMenu = true)
    {
        var direction = Vector3.zero;

        if (transform == transform.parent.GetChild(0))
        {
            direction = Player.transform.position - transform.position;
            transform.DOMove(Player.transform.position, Player.MoveTime );
        }
        else
        {
            direction = GetPreviousSegmentCell().transform.position - transform.position;
            transform.DOMove(GetPreviousSegmentCell().transform.position, Player.MoveTime);
        }

        TryRotate(direction);

        LastDirection = direction;
    }

    public void TryRotate(Vector3 direction)
    {
        // Horizontal to vertical
        if (Mathf.Abs(LastDirection.x) > Mathf.Abs(LastDirection.y) && Mathf.Abs(direction.x) < Mathf.Abs(direction.y))
        {
            if (direction.y > 0)
            {
                transform.DORotate(new Vector3(0f, 0f, 90f), Player.MoveTime);
            }
            else if (direction.y < 0)
            {
                transform.DORotate(new Vector3(0f, 0f, -90f), Player.MoveTime);
            }
        }
        // Vertical to horizontal
        else if (Mathf.Abs(LastDirection.x) < Mathf.Abs(LastDirection.y) && Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0)
            {
                transform.DORotate(new Vector3(0f, 0f, 0f), Player.MoveTime);
            }
            else if (direction.x < 0)
            {
                transform.DORotate(new Vector3(0f, 0f, 180f), Player.MoveTime);
            }
        }
    }

    private GridCell GetPreviousSegmentCell()
    {
        GridCell previousGridCell;

        int index = transform.GetSiblingIndex();
        previousGridCell = transform.parent.GetChild(index - 1).GetComponent<Segment>().CurrentCell;

        return previousGridCell;
    }
}
