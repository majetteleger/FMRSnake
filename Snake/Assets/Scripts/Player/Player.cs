using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class Player : MonoBehaviour
{
    [SerializeField]
    public class Button
    {
        public Vector3 Direction;
        public KeyCode KeyCode;
        public bool IsOn;

        public Button(Vector3 direction, KeyCode keyCode)
        {
            Direction = direction;
            KeyCode = keyCode;
            IsOn = false;
        }
    }

    public GameObject SegmentPrefab;
    public GameObject DummySegmentPrefab;
    public float MoveTime;
    public int IntermediateSegments;
    public float CenterAppearProbabilityIncrement;
    public int MaxHealth;
    public int HealthDecreaseOnMiss;
    public int HealthIncreaseOnHit;
    public int MaxMovementMultipler;
    public int MultiplerDecreaseOnMiss;
    public int MultiplerIncreaseOnHit;

    public bool HasMoved { get; set; }
    public Vector3 LastDirection { get; set; }
    public float CenterAppearProbability { get; set; }
    public Segment HeadSegment { get; set; }

    public int Health
    {
        get
        {
            return _health;
        }
        set
        {
            _health = Mathf.Clamp(value, 0, MaxHealth);
            MainPanel.Instance.UpdateHealth(_health, MaxHealth);

            if (_health <= 0)
            {
                Die();
            }
        }
    }

    public int Score
    {
        get
        {
            return _score;
        }
        set
        {
            _score = value;
            MainPanel.Instance.UpdateScore(_score, _score == 0);
        }
    }

    public int MovementMultiplier
    {
        get
        {
            return _movementMultiplier;
        }
        set
        {
            if (value > MaxMovementMultipler || value <= 0)
            {
                return;
            }

            _movementMultiplier = Mathf.Clamp(value, 1, MaxMovementMultipler);
            MainPanel.Instance.UpdateMovementMultiplier(_movementMultiplier, false);
        }
    }

    private int Length
    {
        get
        {
            var length = 0;
            var nextSegment = HeadSegment;

            while (nextSegment != null)
            {
                length++;
                nextSegment = nextSegment.NextSegment;
            }

            return length;
        }
    }

    private CircleCollider2D _playerCollider;
    private Vector3 _prevHeadPosition;
    private GridPlayground _gridPlayground;
    private GridCell _currentCell;
    private Segment _lastSegment;
    private Queue<Vector3> _moveQueue;
    private Queue<bool> _growQueue;
    private bool _moving;
    private Transform _segmentsContainer;
    private int _score;
    private int _movementMultiplier;
    private BeatIndicator _beatIndicator;
    private int _health;

    /*private Button[] _buttons = {
        new Button(Vector2.left, KeyCode.LeftArrow),
        new Button(Vector2.up, KeyCode.UpArrow),
        new Button(Vector2.right, KeyCode.RightArrow),
        new Button(Vector2.down, KeyCode.DownArrow),
    };*/

    private void Start()
    {
        _beatIndicator = MainPanel.Instance.BeatIndicator;
        _gridPlayground = FindObjectOfType<GridPlayground>();
        _playerCollider = GetComponent<CircleCollider2D>();

        HeadSegment = Instantiate(SegmentPrefab, transform).GetComponent<Segment>();
        HeadSegment.Center.enabled = true;
        Destroy(HeadSegment.GetComponent<BoxCollider2D>());

        _lastSegment = HeadSegment;

        _moveQueue = new Queue<Vector3>();
        _growQueue = new Queue<bool>();

        _segmentsContainer = new GameObject("Segments").transform;
    }

    private void Update()
    {
        //foreach (var button in _buttons)
        //{
        //    if (Input.GetKeyDown(button.KeyCode))
        //    {
        //        button.IsOn = true;
        //        MainPanel.Instance.ControlToggle(button.KeyCode, true);
        //    }
        //    else if (Input.GetKeyUp(button.KeyCode))
        //    {
        //        button.IsOn = false;
        //        MainPanel.Instance.ControlToggle(button.KeyCode, false);
        //    }
        //}

        if (MainPanel.Instance.BeatIndicator.IsHot && !HasMoved)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Keypad8))
            {
                QueueMove(Vector3.up);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                QueueMove(Vector3.down);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Keypad6))
            {
                QueueMove(Vector3.right);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Keypad4))
            {
                QueueMove(Vector3.left);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var food = other.GetComponent<Food>();
        var obstacle = other.GetComponent<Obstacle>();
        var gridCell = other.GetComponent<GridCell>();
        var segment = other.GetComponent<Segment>();
        var wall = other.GetComponent<Wall>();

        if (food != null)
        {
            GiveScore(food.Zone.ZoneModifier.FoodScore, false, true);
            food.Zone.FoodObjects.Remove(food);
            food.Zone.TryClear();
            Destroy(other.gameObject);
            QueueGrow();
        }
        else if (obstacle != null)
        {
            Debug.Log("Colliding with Obstacle!");
            Die();
        }
        else if (segment != null)
        {
            Debug.Log("Colliding with Tail!");
            Die();
        }
        else if (wall != null)
        {
            Debug.Log("Colliding with Wall!");
            Die();
        }

        if (gridCell != null)
        {
            gridCell.Content = gameObject;
            _currentCell = gridCell;
            if (MainPanel.Instance.BeatIndicator.Bar != _currentCell.ZoneModifier.Bar)
            {
                MainPanel.Instance.BeatIndicator.UpdateBar(_currentCell.ZoneModifier.Bar);
            }
        }

    }

    public void GiveScore(int baseScore, bool lengthMultiplication, bool movementMultiplication)
    {
        Score += baseScore * (lengthMultiplication ? Length : 1) * (movementMultiplication ? _movementMultiplier : 1);
    }

    public void Die()
    {
        _gridPlayground.ClearAllCells();

        var foodObjects = FindObjectsOfType<Food>();

        foreach (var foodObject in foodObjects)
        {
            Destroy(foodObject.gameObject);
        }

        var zoneIndicators = FindObjectsOfType<ZoneIndicator>();

        foreach (var zoneIndicator in zoneIndicators)
        {
            Destroy(zoneIndicator.gameObject);
        }

        var obstacles = FindObjectsOfType<Obstacle>();

        foreach (var obstacle in obstacles)
        {
            Destroy(obstacle.gameObject);
        }

        _playerCollider.enabled = false;
        MainPanel.Instance.BeatIndicator.StopBeat();
        MainManager.Instance.TransitionToLeaderBoard();
    }
    
    public void Destroy()
    {
        if (_lastSegment == null)
        {
            return;
        }

        var tempSegment = _lastSegment;

        while (tempSegment.PreviouSegment != null)
        {
            var segmentToDestroy = tempSegment.gameObject;
            tempSegment = tempSegment.PreviouSegment;

            Destroy(segmentToDestroy);
        }

        var dummySegments = FindObjectsOfType<DummySegment>();

        foreach (var dummySegment in dummySegments)
        {
            Destroy(dummySegment.gameObject);
        }

        Destroy(gameObject);
    }

    public void StartGame()
    {
        _playerCollider.enabled = true;
        Score = 0;
        MovementMultiplier = 1;
        Health = MaxHealth;

        _beatIndicator.CreatePassiveBeats();
        _beatIndicator.CreateActiveBeats();
        _beatIndicator.StartMetronome();
    }
    
    public void QueueMove(Vector3 direction)
    {
        HasMoved = true;
        
        if (MainManager.Instance.CurrentState == MainManager.GameState.Play && _beatIndicator.CurrentActiveBeat != null)
        {
            _beatIndicator.CurrentActiveBeat.Light.color = Color.green;
            _beatIndicator.CurrentActiveBeat.Activated = true;
        }

        if (_moving)
        {
            _moveQueue.Enqueue(direction);
            return;
        }

        Move(direction);
    }

    public void QueueGrow()
    {
        if (_moving)
        {
            _growQueue.Enqueue(true);
            return;
        }

        Grow();
    }

    public void FailBeat()
    {
        Health -= HealthDecreaseOnMiss;
        MovementMultiplier -= MultiplerDecreaseOnMiss;

        //Debug.Log("--");
    }

    private void Move(Vector3 direction)
    {
        MovementMultiplier += MultiplerIncreaseOnHit;
        Health += HealthIncreaseOnHit;

        //Debug.Log("++");

        _moving = true;

        LastDirection = direction;

        var playerMoveDestination = transform.position + direction * _gridPlayground.MoveDistance;
        var movement = transform.DOMove(playerMoveDestination, MainManager.Instance.CurrentState == MainManager.GameState.Play ? MoveTime : MainManager.Instance.TransitionTime);

        HeadSegment.Move(playerMoveDestination);

        var cameraMoveDestination = playerMoveDestination + 
            (MainManager.Instance.CurrentState == MainManager.GameState.BuildYourSnake 
            ? new Vector3(MainPanel.Instance.BuildYourSnakeCameraOffset.x, MainPanel.Instance.BuildYourSnakeCameraOffset.y) 
            : Vector3.zero);

        cameraMoveDestination.z = -10f;

        Camera.main.transform.DOMove(cameraMoveDestination, MainManager.Instance.CurrentState == MainManager.GameState.Play ? MoveTime : MainManager.Instance.TransitionTime);

        movement.onComplete += MovementCallback;
    }
    
    public void Grow()
    {
        var spawnPosition = /*MainManager.Instance.CurrentState == MainManager.GameState.Play
            ? _lastSegment.PreviouSegment.transform.position
            :*/ _lastSegment.transform.position;

        var newSegment = Instantiate(SegmentPrefab, spawnPosition, Quaternion.identity).GetComponent<Segment>();
        newSegment.FrontDummySegments = new DummySegment[IntermediateSegments];
        newSegment.transform.SetParent(_segmentsContainer, true);

        var pastLastSegment = _lastSegment;
        _lastSegment.NextSegment = newSegment;
        newSegment.PreviouSegment = pastLastSegment;
        _lastSegment = newSegment;

        newSegment.GetComponentInChildren<SpriteRenderer>().color = GetComponentInChildren<SpriteRenderer>().color;
        
        var centerShown = newSegment.TryShowCenter(CenterAppearProbability);

        if (centerShown)
        {
            CenterAppearProbability = 0f;
        }
        else
        {
            CenterAppearProbability += CenterAppearProbabilityIncrement;
        }

        for (var i = 0; i < IntermediateSegments; i++)
        {
            var newDummySegment = Instantiate(DummySegmentPrefab, _lastSegment.transform.position, Quaternion.identity).GetComponent<DummySegment>();
            newDummySegment.GetComponentInChildren<SpriteRenderer>().color = GetComponentInChildren<SpriteRenderer>().color;
            newDummySegment.Initialize(newSegment.PreviouSegment, newSegment, i, IntermediateSegments, MainManager.Instance.CurrentState == MainManager.GameState.Play ? MoveTime : MainManager.Instance.TransitionTime);
            newDummySegment.transform.SetParent(_segmentsContainer, true);

            newSegment.FrontDummySegments[i] = newDummySegment;
        }
    }

    private void MovementCallback()
    {
        _moving = false;

        if (_moveQueue.Count > 0)
        {
            var direction = _moveQueue.Dequeue();
            Move(direction);
        }

        if (_growQueue.Count > 0)
        {
            _growQueue.Dequeue();
            Grow();
        }
        
        if (MainManager.Instance.CurrentState == MainManager.GameState.BuildYourSnake)
        {
            MainManager.Instance.PrepMovesExecuted++;

            if (MainManager.Instance.PrepMovesExecuted >= MainManager.Instance.StartSegments)
            {
                MainManager.Instance.PlayerNamePanel.ToggleConfirm(true);
            }
        }
    }
}