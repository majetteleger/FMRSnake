using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridPlayground : MonoBehaviour 
{
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
	
	private void Start()
	{
		var playerSpawned = false;
		
		for(var x = -GridRadius; x < GridRadius; x += (CellSize + CellSpacing))
		{
			for(var y = -GridRadius; y < GridRadius; y += (CellSize + CellSpacing))
			{
				var newGridCell = Instantiate(CellPrefab, new Vector3(x, y, 0f), Quaternion.identity).GetComponent<GridCell>();
				newGridCell.GetComponent<SpriteRenderer>().size = Vector2.one * CellSize;
				newGridCell.GetComponent<BoxCollider2D>().size = Vector2.one * CellSize;
                newGridCell.transform.SetParent(transform);
				
				if(!playerSpawned && Mathf.Abs(x) < 0.5f && Mathf.Abs(y) < 0.5f)
				{
					Instantiate(PlayerPrefab, new Vector3(x, y, 0f), Quaternion.identity);
					playerSpawned = true;
				}
			}
		}

	    _cells = GetComponentsInChildren<GridCell>();

        _foodSpawnTimer = FoodSpawnTime;
        _zoneSpawnTimer = ZoneSpawnTime;
    }
	
	private void Update()
	{
		if(_foodSpawnTimer > 0f)
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
	    var randomCell = _cells[Random.Range(0, _cells.Length)];

	    Instantiate(FoodPrefab, randomCell.transform);
    }

    private void SpawnZone()
    {
        var randomModifier = ZoneModifiers[Random.Range(0, ZoneModifiers.Length)];

        var randomPosition = new Vector2(Random.Range(-GridRadius, GridRadius), Random.Range(-GridRadius, GridRadius));
        var overlappedCells = Physics2D.OverlapCircleAll(randomPosition, randomModifier.Radius).Where(x => x.GetComponent<GridCell>() != null).Select(x => x.GetComponent<GridCell>());
        
        foreach (var overlappedCell in overlappedCells)
        {
            overlappedCell.ZoneModifiers.Add(randomModifier);
            overlappedCell.Modify(randomModifier);
        }
    }
}
