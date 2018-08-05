using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;

public class GridPlayground : MonoBehaviour
{
    public static GridPlayground Instance;

    public GameObject CellPrefab;
	public GameObject PlayerPrefab;
	public GameObject FoodPrefab;
    public GameObject ObstaclePrefab;
	public GameObject ZoneCenterPrefab;
    public SpriteRenderer BackgroundRenderer;
    public float CellSize;
    public float CellSpacing;
	public float FoodSpawnTime;
    public ZoneModifier[] ZoneModifiers;
    public ZoneModifier NoneZoneModifier;
	public float ZoneSpawnTime;
	public int MaxZoneSpawned;
    public float ObstacleSpawnTime;
    public int MaxObstaclesSpawned;
    public float AroundPlayerThreshold;
    public float ClosePlayerThreshold;
    public ObstacleShape[] ObstacleShapes;
    public List<Food> Foods { get; set; }
    public List<Obstacle> Obstacles { get; set; }

    public float MoveDistance { get { return CellSize + CellSpacing; } }
    [SerializeField]
    public int ZonesSpawned { get; set; }
    public int ObstaclesSpawned { get; set; }

    private Transform _zonesParent;
    public List<Zone> _zones = new List<Zone>();
    private Player _player;
    private float _obstacleSpawnTimer;
    private float _gridHeight;
    private float _gridWidth;
    private float _zoneSpawnTimer;
    private GridCell[] _cells;
    
    private void Awake()
    {
        _zonesParent = new GameObject("Zones").transform;
        Instance = this;

        _gridWidth = BackgroundRenderer.sprite.rect.size.x / 95f;
        _gridHeight = BackgroundRenderer.sprite.rect.size.y / 95f;

        Obstacles = new List<Obstacle>();
        Foods = new List<Food>();

        for (var x = transform.position.x - _gridWidth / 2; x < _gridWidth / 2; x += (CellSize + CellSpacing))
        {
            var firstColumn = Math.Abs(x - (transform.position.x - _gridWidth / 2)) < 0.1f;
            var lastColumn = x + (CellSize + CellSpacing) >= _gridWidth / 2;

            for (var y = transform.position.y - _gridHeight / 2; y < _gridHeight / 2; y += (CellSize + CellSpacing))
            {
                var firstRow = Math.Abs(y - (transform.position.y - _gridHeight / 2)) < 0.1f;
                var lastRow = y + (CellSize + CellSpacing) >= _gridHeight / 2;

                var newGridCell = Instantiate(CellPrefab, new Vector3(x, y, 0f), Quaternion.identity).GetComponent<GridCell>();
                newGridCell.GetComponent<SpriteRenderer>().size = Vector2.one * CellSize;
                newGridCell.GetComponent<BoxCollider2D>().size = Vector2.one * CellSize;
                newGridCell.transform.SetParent(transform);
                newGridCell.IsBorder = firstRow || lastRow || firstColumn || lastColumn;
                
                if (newGridCell.IsBorder)
                {
                    SpawnObstacle(newGridCell, true);

                    foreach (var section in newGridCell.TileSections)
                    {
                        section.Renderer.sprite = newGridCell.BlankTileSectionSprite;
                        section.Renderer.color = new Color(newGridCell.ObstacleColor.r, newGridCell.ObstacleColor.g, newGridCell.ObstacleColor.b, newGridCell.ColorAlpha);
                        section.Renderer.enabled = true;
                        section.Renderer.DOFade(newGridCell.ColorAlpha, MainManager.Instance.PulseTime);
                    }
                }
            }
        }

        _cells = GetComponentsInChildren<GridCell>();
    }

    public void ResetZones()
    {
        for (int i = 0; i < _zones.Count; i++)
        {
            Destroy(_zones[i].gameObject);
        }
        _zones.Clear();
    }

    private void Start()
	{
	    _zoneSpawnTimer = ZoneSpawnTime;

        _obstacleSpawnTimer = ObstacleSpawnTime;
    }
	
	private void Update()
	{
	    if (MainManager.Instance.CurrentState != MainManager.GameState.Play)
	    {
	        return;
	    }
        
	    if (_zoneSpawnTimer > 0f)
	    {
	        _zoneSpawnTimer -= Time.deltaTime;

	        if (_zoneSpawnTimer < 0)
	        {
	            TrySpawnZone();
	            _zoneSpawnTimer = ZoneSpawnTime;
	        }
	    }

        if (_obstacleSpawnTimer > 0)
        {
            _obstacleSpawnTimer -= Time.deltaTime;

            if (_obstacleSpawnTimer < 0)
            {
                var obstacles = TrySpawnObstacle();

                foreach (var obstacle in obstacles)
                {
                    obstacle.Cell.OutlineObstacle();
                }

                _obstacleSpawnTimer = ObstacleSpawnTime;
            }
        }
    }
	
    private Obstacle SpawnObstacle(GridCell cell, bool permanent = false)
    {
        var newObstacle = Instantiate(ObstaclePrefab, cell.transform).GetComponent<Obstacle>();
        cell.Content = newObstacle.gameObject;
        newObstacle.Cell = cell;
        newObstacle.Permanent = permanent;
        Obstacles.Add(newObstacle);

        newObstacle.transform.DOScale(1f, MainManager.Instance.PulseTime).OnComplete(() => newObstacle.DontPulse = false);

        if (!permanent)
        {
            ObstaclesSpawned++;
        }
        
        return newObstacle;
    }
    
    private Food SpawnFood(GridCell cell)
	{
        var newFood = Instantiate(FoodPrefab, cell.transform).GetComponent<Food>();
        Foods.Add(newFood);
        cell.Content = newFood.gameObject;

	    newFood.transform.DOScale(0.9f, MainManager.Instance.PulseTime).OnComplete(() => newFood.DontPulse = false);

        return newFood;
    }

    private GridCell GetRandomEmptyCell(GridCell[] sourceCells)
    {
        var emptyCells = sourceCells.Where(cell => cell.Content == null).ToArray();

        if (emptyCells.Length == 0)
        {
            return null;
        }

        return emptyCells[UnityEngine.Random.Range(0, emptyCells.Length)];
    }

    private Obstacle[] TrySpawnObstacle()
    {
        if (ObstaclesSpawned > MaxObstaclesSpawned)
        {
            return null;
        }
        
        var aroundPlayer = Physics2D.OverlapCircleAll(MainManager.Instance.Player.transform.position, AroundPlayerThreshold)
            .Where(x => x.GetComponent<GridCell>() != null && x.GetComponent<GridCell>().Content == null && x.GetComponent<GridCell>().ZoneModifier == MainManager.Instance.GridPlayground.NoneZoneModifier)
            .Where(x => Vector2.Distance(x.transform.position, MainManager.Instance.Player.transform.position) >= ClosePlayerThreshold)
            .ToList();
        
        var cell = aroundPlayer[UnityEngine.Random.Range(0, aroundPlayer.Count)].GetComponent<GridCell>();
        
        var shape = ObstacleShapes[Random.Range(0, ObstacleShapes.Length)];
        var newCell = cell;
        var directionIndex = 0;
        var obstacleList = new List<Obstacle>();

        do
        {
            var newObstacle = SpawnObstacle(newCell);
            obstacleList.Add(newObstacle);

            if (directionIndex >= shape.ShapeDirections.Length || Vector2.Distance(newCell.transform.position, MainManager.Instance.Player.transform.position) < ClosePlayerThreshold)
            {
                break;
            }
            
            newCell = newCell.GetAdjacentCell(shape.ShapeDirections[directionIndex]);
            directionIndex++;

        } while (newCell != null && newCell.Content == null && newCell.ZoneModifier == MainManager.Instance.GridPlayground.NoneZoneModifier);

        return obstacleList.ToArray();
    }

    private void TrySpawnZone()
    {
        if (ZonesSpawned >= MaxZoneSpawned)
        {
            return;
        }

        var randomModifier = ZoneModifiers[UnityEngine.Random.Range(0, ZoneModifiers.Length)];

        var randomPosition = Vector2.zero;
        var overlappingZoneOrPlayer = false;
        var tries = 0;
        var maxTries = 10;

        do
        {
            randomPosition = new Vector2(UnityEngine.Random.Range(-_gridWidth / 2f, _gridWidth / 2f), UnityEngine.Random.Range(-_gridHeight / 2f, _gridHeight / 2f));
            overlappingZoneOrPlayer = Physics2D.OverlapCircleAll(randomPosition, randomModifier.Radius * 2.5f).Any(x => x.GetComponent<Zone>() != null || x.GetComponent<Player>());
            tries++;
        }
        while (overlappingZoneOrPlayer && tries < maxTries);

        if(overlappingZoneOrPlayer)
        {
            return;
        }

        var overlappedCells = Physics2D.OverlapCircleAll(randomPosition, randomModifier.Radius).Where(x => x.GetComponent<GridCell>() != null).Select(x => x.GetComponent<GridCell>());

        foreach (var overlappedCell in overlappedCells)
        {
            if (overlappedCell.Content != null && overlappedCell.Content.GetComponent<Obstacle>() != null)
            {
                if (overlappedCell.Content.GetComponent<Obstacle>().Permanent)
                {
                    continue;
                }

                overlappedCell.Content.GetComponent<Obstacle>().Clear();
            }
            
            overlappedCell.ZoneModifier = randomModifier;
        }

        foreach (var overlappedCell in overlappedCells)
        {
            overlappedCell.Modify();
        }

        var newZone = Instantiate(ZoneCenterPrefab, randomPosition, Quaternion.identity).GetComponent<Zone>();
        newZone.Initialize(overlappedCells.ToArray(), randomModifier);
        _zones.Add(newZone);
        newZone.transform.parent = _zonesParent;

        ZonesSpawned++;

        var tempZoneCells = new List<GridCell>(overlappedCells);
        
        for (var i = 0; i < randomModifier.FoodSpawnCount; i++)
        {
            var cell = GetRandomEmptyCell(tempZoneCells.ToArray());
            var food = SpawnFood(cell);
            food.Zone = newZone;

            tempZoneCells.Remove(cell);
            newZone.FoodObjects.Add(food);
        }
    }

    public void ClearAllCells()
    {
        for (int i = 0; i < _cells.Length; i++)
        {
            _cells[i].ZoneModifier = NoneZoneModifier;

            if (_cells[i].Content != null)
            {
                var food = _cells[i].Content.GetComponent<Food>();

                if (food != null)
                {
                    Foods.Remove(food);
                }
            }
            
            _cells[i].Modify();
        }
    }

    public void PulseObstacles()
    {
        for (int i = 0; i < Obstacles.Count; i++)
        {
            var Obstacle = Obstacles[i];

            if (Obstacle.DontPulse)
            {
                continue;
            }

            Obstacle.transform.DOPunchScale(Vector3.one * MainManager.Instance.PulseFactor, MainManager.Instance.PulseTime)
                .OnComplete(() => Obstacle.transform.localScale = Vector3.one);
        }
    }

    public void PulseFoods()
    {
        for (int i = 0; i < Foods.Count; i++)
        {
            var food = Foods[i];

            if (food.DontPulse)
            {
                continue;
            }

            food.transform.DOPunchScale(Vector3.one * 0.9f * MainManager.Instance.PulseFactor, MainManager.Instance.PulseTime)
                .OnComplete(() => food.transform.localScale = Vector3.one * 0.9f);
        }
    }
}