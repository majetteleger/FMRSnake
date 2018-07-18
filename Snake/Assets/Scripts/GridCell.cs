using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridCell : MonoBehaviour
{
    public Sprite ObstacleSprite;
    public float ColorAlpha;
    public bool IsObstacle;
    public int ObstacleTime;
    public ZoneModifier ZoneModifier { get; set; }
    public GameObject Content { get; set; }

    private SpriteRenderer _spriteRenderer;
    private Sprite _blankSprite;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _blankSprite = _spriteRenderer.sprite;
        ZoneModifier = MainManager.Instance.GridPlayground.NoneZoneModifier;
        Modify();
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == Content)
        {
            Content = null;
        }
    }

    public void Modify()
    {
        if (ZoneModifier != null)
        {
            _spriteRenderer.drawMode = SpriteDrawMode.Tiled;
            _spriteRenderer.sprite = _blankSprite;
            _spriteRenderer.color = new Color(ZoneModifier.Color.r, ZoneModifier.Color.g, ZoneModifier.Color.b, ZoneModifier.Color.a);
        }

        if (IsObstacle)
        {
            _spriteRenderer.drawMode = SpriteDrawMode.Simple;
            _spriteRenderer.sprite = ObstacleSprite;
        }
    }

    public GridCell GetAdjacentCell(ObstacleShape.Direction direction)
    {
        var adjacentColliders = Physics2D.OverlapCircleAll(transform.position, GridPlayground.Instance.CellSize * 1.5f);

        foreach (var adjacentCollider in adjacentColliders)
        {
            var adjacentCell = adjacentCollider.gameObject.GetComponent<GridCell>();

            if (adjacentCell == null || adjacentCell == this)
            {
                continue;
            }

            switch (direction)
            {
                case ObstacleShape.Direction.Up:

                    if (adjacentCell.transform.position.y > transform.position.y && Math.Abs(adjacentCell.transform.position.x - transform.position.x) < 0.1f)
                    {
                        return adjacentCell;
                    }

                    continue;

                case ObstacleShape.Direction.Right:

                    if (adjacentCell.transform.position.x > transform.position.x && Math.Abs(adjacentCell.transform.position.y - transform.position.y) < 0.1f)
                    {
                        return adjacentCell;
                    }

                    continue;

                case ObstacleShape.Direction.Down:

                    if (adjacentCell.transform.position.y < transform.position.y && Math.Abs(adjacentCell.transform.position.x - transform.position.x) < 0.1f)
                    {
                        return adjacentCell;
                    }

                    continue;

                case ObstacleShape.Direction.Left:

                    if (adjacentCell.transform.position.x < transform.position.x && Math.Abs(adjacentCell.transform.position.y - transform.position.y) < 0.1f)
                    {
                        return adjacentCell;
                    }

                    continue;
            }
        }

        return null;
    }
}