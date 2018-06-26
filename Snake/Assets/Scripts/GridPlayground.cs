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
    public float CellSize;
	public float CellSpacing;
	public float GridRadius;
	public float FoodSpawnTime;
    public ZoneModifier[] ZoneModifiers;
	public float ZoneSpawnTime;

    public float MoveDistance { get { return CellSize + CellSpacing; } }

    private float _foodSpawnTimer;
    private float _zoneSpawnTimer;
    private GridCell[] _cells;

    private void Awake()
    {
        Instance = this;
    }

	private void Start()
	{
		for(var x = transform.position.x - GridRadius; x < GridRadius; x += (CellSize + CellSpacing))
		{
			for(var y = transform.position.y - GridRadius; y < GridRadius; y += (CellSize + CellSpacing))
			{
				var newGridCell = Instantiate(CellPrefab, new Vector3(x, y, 0f), Quaternion.identity).GetComponent<GridCell>();
				newGridCell.GetComponent<SpriteRenderer>().size = Vector2.one * CellSize;
				newGridCell.GetComponent<BoxCollider2D>().size = Vector2.one * CellSize;
                newGridCell.transform.SetParent(transform);
			}
		}

	    _cells = GetComponentsInChildren<GridCell>();

        _foodSpawnTimer = FoodSpawnTime;
        _zoneSpawnTimer = ZoneSpawnTime;
    }
	
	private void Update()
	{
	    if (MainManager.Instance.CurrentState != MainManager.GameState.Play)
	    {
	        return;
	    }

        if (_foodSpawnTimer > 0f)
		{
			_foodSpawnTimer -= Time.deltaTime;
			
			if(_foodSpawnTimer < 0)
			{
				SpawnFood();
				_foodSpawnTimer = FoodSpawnTime;
			}
		}

	    if (_zoneSpawnTimer > 0f)
	    {
	        _zoneSpawnTimer -= Time.deltaTime;

	        if (_zoneSpawnTimer < 0)
	        {
	            SpawnZone();
	            _zoneSpawnTimer = ZoneSpawnTime;
	        }
	    }
    }
	
	private void SpawnFood()
	{
        var randomCell = GetRandomEmptyCell();

        if (randomCell == null)
            return;

        //var randomCell = _cells[UnityEngine.Random.Range(0, _cells.Length)];

        GameObject newFood = Instantiate(FoodPrefab, randomCell.transform);

        randomCell.Content = newFood;
    }

    private GridCell GetRandomEmptyCell()
    {
        var emptyCells = _cells.Where(cell => cell.Content == null).ToArray();

        if (emptyCells.Length == 0)
            return null;
        else
            return emptyCells[UnityEngine.Random.Range(0, emptyCells.Length)];

    }

    private void SpawnZone()
    {
        var randomModifier = ZoneModifiers[UnityEngine.Random.Range(0, ZoneModifiers.Length)];

        var randomPosition = new Vector2(UnityEngine.Random.Range(-GridRadius, GridRadius), UnityEngine.Random.Range(-GridRadius, GridRadius));
        var overlappedCells = Physics2D.OverlapCircleAll(randomPosition, randomModifier.Radius).Where(x => x.GetComponent<GridCell>() != null).Select(x => x.GetComponent<GridCell>());
        
        foreach (var overlappedCell in overlappedCells)
        {
            overlappedCell.ZoneModifiers.Add(randomModifier);
            overlappedCell.Modify(randomModifier);
        }
    }

    public void ShowCells()
    {
        foreach (var cell in _cells)
        {
            cell.GetComponent<SpriteRenderer>().enabled = true;
        }
    }

    public void HideCells()
    {
        foreach (var cell in _cells)
        {
            cell.GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}
