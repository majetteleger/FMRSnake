using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class Player : MonoBehaviour
{
    [SerializeField]
    public struct Button
    {
        public int Index;
        public Vector3 Direction;
        public bool IsOn;

        public Button(int index, Vector3 direction, bool isOn)
        {
            Index = index;
            Direction = direction;
            IsOn = isOn;
        }
    }

    public GameObject SegmentPrefab;
    public GameObject DummySegmentPrefab;
    public float MoveTime;
    public int MaxMovementMultipler;
    public int IntermediateSegments;
    public float CenterAppearProbabilityIncrement;
    public string PlayerName;
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
        new Button(0, Vector2.left, false),
        new Button(1, Vector2.up, false),
        new Button(2, Vector2.right, false),
        new Button(3, Vector2.down, false),
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
        if (MainManager.Instance.CurrentState != MainManager.GameState.Play)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            _buttons[0].IsOn = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            _buttons[0].IsOn = false;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            _buttons[1].IsOn = true;
        }
        else if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            _buttons[1].IsOn = false;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            _buttons[2].IsOn = true;
        }
        else if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            _buttons[2].IsOn = false;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            _buttons[3].IsOn = true;
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            _buttons[3].IsOn = false;
        }

        // DEBUG CONTROLS

        /*if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            _buttons[0].IsOn = false;
            _buttons[1].IsOn = true;
            _buttons[2].IsOn = true;
            _buttons[3].IsOn = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            _buttons[0].IsOn = true;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            _buttons[0].IsOn = true;
            _buttons[1].IsOn = false;
            _buttons[2].IsOn = true;
            _buttons[3].IsOn = true;
        }
        else if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            _buttons[1].IsOn = true;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            _buttons[0].IsOn = true;
            _buttons[1].IsOn = true;
            _buttons[2].IsOn = false;
            _buttons[3].IsOn = true;
        }
        else if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            _buttons[2].IsOn = true;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            _buttons[0].IsOn = true;
            _buttons[1].IsOn = true;
            _buttons[2].IsOn = true;
            _buttons[3].IsOn = false;
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            _buttons[3].IsOn = true;
        }*/
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var food = other.GetComponent<Food>();
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
        else if (gridCell != null)
        {
            gridCell.Content = gameObject;
            _currentCell = gridCell;
            MainPanel.Instance.BeatIndicator.UpdateIndicator(_currentCell.ZoneModifier.Bar);
        }
        else if (segment != null)
        {
            Debug.Log("Colliding with Tail!");
            Die();
        }
    }

    public void GiveScore(int baseScore, bool lengthMultiplication, bool movementMultiplication)
    {
        Score += baseScore * (lengthMultiplication ? Length : 1) * (movementMultiplication ? _movementMultiplier : 1);
    }

    public void Die()
    {
        //for (int i = 0; i < _segmentsContainer.childCount; i++)
        //{
        //    if (_segmentsContainer.GetChild(i).GetComponent<BoxCollider2D>())
        //    {
        //        _segmentsContainer.GetChild(i).GetComponent<BoxCollider2D>().enabled = false;
        //    }

        //}
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
        _beatIndicator.StartBeat();
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
}