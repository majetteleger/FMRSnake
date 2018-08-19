using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class GridCell : MonoBehaviour
{
    public enum TileCorner
    {
        TopLeft,
        TopRight,
        BottomRight,
        BottomLeft,
        None
    }

    [Serializable]
    public class TileSection
    {
        public TileCorner Corner;
        public SpriteRenderer Renderer;
        public Sprite RoundedSprite;
    }
    
    public float ColorAlpha;
    public Color ObstacleColor;
    public TileSection[] TileSections;
    public Sprite BlankTileSectionSprite;

    public ZoneModifier ZoneModifier { get; set; }
    public GameObject Content;/* { get; set; }*/
    public bool IsBorder { get; set; }

    private void Start()
    {
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
        if (ZoneModifier != MainManager.Instance.GridPlayground.NoneZoneModifier)
        {
            var zoneModifierUp = GetAdjacentCell(ObstacleShape.Direction.Up) != null
                && GetAdjacentCell(ObstacleShape.Direction.Up).ZoneModifier != null
                && GetAdjacentCell(ObstacleShape.Direction.Up).ZoneModifier != MainManager.Instance.GridPlayground.NoneZoneModifier;

            var zoneModifierRight = GetAdjacentCell(ObstacleShape.Direction.Right) != null
                && GetAdjacentCell(ObstacleShape.Direction.Right).ZoneModifier != null
                && GetAdjacentCell(ObstacleShape.Direction.Right).ZoneModifier != MainManager.Instance.GridPlayground.NoneZoneModifier;

            var zoneModifierDown = GetAdjacentCell(ObstacleShape.Direction.Down) != null
                && GetAdjacentCell(ObstacleShape.Direction.Down).ZoneModifier != null
                && GetAdjacentCell(ObstacleShape.Direction.Down).ZoneModifier != MainManager.Instance.GridPlayground.NoneZoneModifier;

            var zoneModifierLeft = GetAdjacentCell(ObstacleShape.Direction.Left) != null
                && GetAdjacentCell(ObstacleShape.Direction.Left).ZoneModifier != null
                && GetAdjacentCell(ObstacleShape.Direction.Left).ZoneModifier != MainManager.Instance.GridPlayground.NoneZoneModifier;

            foreach (var tileSection in TileSections)
            {
                tileSection.Renderer.color = new Color(ZoneModifier.Color.r, ZoneModifier.Color.g, ZoneModifier.Color.b, 0f);
                var sharpTile = false;

                switch (tileSection.Corner)
                {
                    case TileCorner.TopLeft:

                        sharpTile = zoneModifierUp || zoneModifierLeft;
                        break;

                    case TileCorner.TopRight:

                        sharpTile = zoneModifierUp || zoneModifierRight;
                        break;

                    case TileCorner.BottomRight:

                        sharpTile = zoneModifierDown || zoneModifierRight;
                        break;

                    case TileCorner.BottomLeft:

                        sharpTile = zoneModifierDown || zoneModifierLeft;
                        break;
                }

                tileSection.Renderer.sprite = sharpTile ? BlankTileSectionSprite : tileSection.RoundedSprite;

                tileSection.Renderer.enabled = true;
                tileSection.Renderer.sortingOrder = -1;
                tileSection.Renderer.DOFade(ColorAlpha, MainManager.Instance.PulseTime);
            }
        }
        else if(Content == null || Content.GetComponent<Obstacle>() == null)
        {
            foreach (var tileSection in TileSections)
            {
                tileSection.Renderer.DOFade(0f, 0.1f).OnComplete(() => tileSection.Renderer.enabled = false);
            }
        }
    }
    
    public void OutlineObstacle()
    {
        var obstacleUp = CheckAdjacentContent<Obstacle>(ObstacleShape.Direction.Up);
        var obstacleRight = CheckAdjacentContent<Obstacle>(ObstacleShape.Direction.Right);
        var obstacleDown = CheckAdjacentContent<Obstacle>(ObstacleShape.Direction.Down);
        var obstacleLeft = CheckAdjacentContent<Obstacle>(ObstacleShape.Direction.Left);

        foreach (var tileSection in TileSections)
        {
            tileSection.Renderer.color = new Color(ObstacleColor.r, ObstacleColor.g, ObstacleColor.b, 0f);
            
            var sharpTile = false;

            switch (tileSection.Corner)
            {
                case TileCorner.TopLeft:

                    sharpTile = obstacleUp || obstacleLeft;
                    break;

                case TileCorner.TopRight:

                    sharpTile = obstacleUp || obstacleRight;
                    break;

                case TileCorner.BottomRight:

                    sharpTile = obstacleDown || obstacleRight;
                    break;

                case TileCorner.BottomLeft:

                    sharpTile = obstacleDown || obstacleLeft;
                    break;
            }

            tileSection.Renderer.sprite = sharpTile ? BlankTileSectionSprite : tileSection.RoundedSprite;

            tileSection.Renderer.enabled = true;
            tileSection.Renderer.sortingOrder = 0;
            tileSection.Renderer.DOFade(ColorAlpha, MainManager.Instance.PulseTime);
        }
    }
    
    public void ClearZone()
    {
        foreach (var tileSection in TileSections)
        {
            tileSection.Renderer.DOFade(0f, MainManager.Instance.PulseTime).OnComplete(() => tileSection.Renderer.enabled = false);
        }
    }

    public GridCell GetAdjacentCell(ObstacleShape.Direction direction)
    {
        var adjacentColliders = Physics2D.OverlapCircleAll(transform.position, GridPlayground.Instance.CellSize * 1.15f);

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

    private bool CheckAdjacentContent<T>(ObstacleShape.Direction direction)
    {
        return GetAdjacentCell(direction) != null
            && GetAdjacentCell(direction).Content != null
            && GetAdjacentCell(direction).Content.GetComponent<T>() != null;
    }
}