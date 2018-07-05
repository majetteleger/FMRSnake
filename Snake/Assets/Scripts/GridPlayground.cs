using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    public int ObstacleLifeTime;

    public float MoveDistance { get { return CellSize + CellSpacing; } }
    public int ZonesSpawned { get; set; }
    public int ObstaclesSpawned { get; set; }

    private Player _player;
    private float _obstacleSpawnTimer;
    private float _gridHeight;
    private float _gridWidth;
    private float _zoneSpawnTimer;
    private GridCell[] _cells;
    
    private void Awake()
    {
        Instance = this;

        _gridWidth = BackgroundRenderer.sprite.rect.size.x / 100f;
        _gridHeight = BackgroundRenderer.sprite.rect.size.y / 100f;

        for (var x = transform.position.x - _gridWidth / 2; x < _gridWidth / 2; x += (CellSize + CellSpacing))
        {
            for (var y = transform.position.y - _gridHeight / 2; y < _gridHeight / 2; y += (CellSize + CellSpacing))
            {
                var newGridCell = Instantiate(CellPrefab, new Vector3(x, y, 0f), Quaternion.identity).GetComponent<GridCell>();
                newGridCell.GetComponent<SpriteRenderer>().size = Vector2.one * CellSize;
                newGridCell.GetComponent<BoxCollider2D>().size = Vector2.one * CellSize;
                newGridCell.transform.SetParent(transform);
            }
        }

        _cells = GetComponentsInChildren<GridCell>();
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
                TrySpawnObstacle();
                _obstacleSpawnTimer = ObstacleSpawnTime;
            }
        }
    }
	
    private Obstacle SpawnObstacle(GridCell cell)
    {
        var newObstacle = Instantiate(ObstaclePrefab, cell.transform);
        cell.Content = newObstacle;

        return newObstacle.GetComponent<Obstacle>();
    }

	private Food SpawnFood(GridCell cell)
	{
        var newFood = Instantiate(FoodPrefab, cell.transform);
        cell.Content = newFood;

        return newFood.GetComponent<Food>();
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

    private void TrySpawnObstacle()
    {
        if (ObstaclesSpawned > MaxObstaclesSpawned)
        {
            return;
        }

        // yea i know it's long...
        var aroundPlayer = Physics2D.OverlapCircleAll(MainManager.Instance.Player.transform.position, 5).Where(x => x.GetComponent<GridCell>() != null && x.GetComponent<GridCell>().Content == null).ToList();
        var closeToPlayer = Physics2D.OverlapCircleAll(MainManager.Instance.Player.transform.position, 1).Where(x => x.GetComponent<GridCell>() != null && x.GetComponent<GridCell>().Content == null).ToArray();

        for (int i = 0; i < closeToPlayer.Length; i++)
        {
            if (aroundPlayer.Contains(closeToPlayer[i]))
            {
                aroundPlayer.Remove(closeToPlayer[i]);
            }
        }

        Debug.Log(aroundPlayer.Count);

        var cell = aroundPlayer[UnityEngine.Random.Range(0, aroundPlayer.Count)].GetComponent<GridCell>();

        var newObstacle = SpawnObstacle(cell);

        newObstacle.LifeTime = ObstacleLifeTime;

        ObstaclesSpawned++;
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
            overlappedCell.ZoneModifier = randomModifier;
            overlappedCell.Modify();
        }

        var newZone = Instantiate(ZoneCenterPrefab, randomPosition, Quaternion.identity).GetComponent<Zone>();
        newZone.Initialize(overlappedCells.ToArray(), randomModifier);

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
            _cells[i].Modify();
        }
    }
}
