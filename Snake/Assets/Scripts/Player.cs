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
    public int MaxMovementMultipler;
    public int IntermediateSegments;
    public float CenterAppearProbabilityIncrement;
    public string PlayerName;
    public bool HasMoved;
    public Vector3 LastDirection { get; set; }
    public float CenterAppearProbability { get; set; }

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
            if (value > MaxMovementMultipler)
            {
                return;
            }

            _movementMultiplier = value;
            MainPanel.Instance.UpdateMovementMultiplier(_movementMultiplier, false);
        }
    }

    private int Length
    {
        get
        {
            var length = 0;
            var nextSegment = _headSegment;

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
    private Segment _headSegment;
    private Segment _lastSegment;
    private Queue<Vector3> _moveQueue;
    private Queue<bool> _growQueue;
    private bool _moving;
    private Transform _segmentsContainer;
    private int _score;
    private int _movementMultiplier;
    private BeatIndicator _beatIndicator;

    private Button[] _buttons = {
        new Button(Vector2.left, KeyCode.LeftArrow),
        new Button(Vector2.up, KeyCode.UpArrow),
        new Button(Vector2.right, KeyCode.RightArrow),
        new Button(Vector2.down, KeyCode.DownArrow),
    };

    private void Start()
    {
        _beatIndicator = MainPanel.Instance.BeatIndicator;
        _gridPlayground = FindObjectOfType<GridPlayground>();
        _playerCollider = GetComponent<CircleCollider2D>();

        _headSegment = Instantiate(SegmentPrefab, transform).GetComponent<Segment>();
        _headSegment.Center.enabled = true;
        Destroy(_headSegment.GetComponent<BoxCollider2D>());

        _lastSegment = _headSegment;

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
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                QueueMove(Vector3.up);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                QueueMove(Vector3.down);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                QueueMove(Vector3.right);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
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

        if (gridCell != null)
        {
            gridCell.Content = gameObject;
            _currentCell = gridCell;
            if (MainPanel.Instance.BeatIndicator.Bar != _currentCell.ZoneModifier.Bar)
            {
                MainPanel.Instance.BeatIndicator.UpdateIndicator(_currentCell.ZoneModifier.Bar);
            }
        }

    }

    public void GiveScore(int baseScore, bool lengthMultiplication, bool movementMultiplication)
    {
        Score += baseScore * (lengthMultiplication ? Length : 1) * (movementMultiplication ? _movementMultiplier : 1);
    }

    public void Die()
    {
        _playerCollider.enabled = false;
        MainPanel.Instance.BeatIndicator.StopBeat();
        LogScoreToLeaderBoard();
        StartCoroutine(DoDie());
    }

    private void LogScoreToLeaderBoard()
    {
        Leaderboard.Record(PlayerName, Score);
    }

    private IEnumerator DoDie()
    {
        yield return new WaitForSeconds(1);

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

        _beatIndicator.CreateIndicator();
        _beatIndicator.StartMetronome();
        //_beatIndicator.StartBeat();
    }

    public void FailMove()
    {
        if (MovementMultiplier != 1)
        {
            MovementMultiplier = 1;
        }
    }

    public bool AttemptMove()
    {
        var offButtons = new List<Button>();

        for (var i = 0; i < _buttons.Length; i++)
        {
            if (!_buttons[i].IsOn)
            {
                offButtons.Add(_buttons[i]);
            }
        }

        if (offButtons.Count == 1)
        {
            QueueMove(offButtons[0].Direction);

            return true;
        }

        return false;
    }

    public void QueueMove(Vector3 direction)
    {
        HasMoved = true;

        if (MainManager.Instance.CurrentState == MainManager.GameState.Play && _beatIndicator.CurrentBeat != null)
        {
            _beatIndicator.CurrentBeat.Light.color = Color.green;
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

    private void Move(Vector3 direction)
    {
        MovementMultiplier++;

        _moving = true;

        LastDirection = direction;

        var playerMoveDestination = transform.position + direction * _gridPlayground.MoveDistance;
        var movement = transform.DOMove(playerMoveDestination, MoveTime);

        _headSegment.Move(playerMoveDestination);

        if (MainManager.Instance.CurrentState == MainManager.GameState.Play)
        {
            var cameraMoveDestination = playerMoveDestination;
            cameraMoveDestination.z = -10f;

            Camera.main.transform.DOMove(cameraMoveDestination, MoveTime);
        }

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
            newDummySegment.Initialize(newSegment.PreviouSegment, newSegment, i, IntermediateSegments, MoveTime);
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
    }

    public void TryAddInput()
    {

    }
}