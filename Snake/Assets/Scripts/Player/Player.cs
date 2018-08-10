using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using System.Linq;
using UnityEngine.UI;

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
    public AnimationCurve MoveCurve;
    public float PlayMoveTime;
    public float SetupMoveTime;
    public float DeathShrinkTime;
    public float DeathShrinkDelay;
    public int IntermediateSegments;
    public float CenterAppearProbabilityIncrement;
    public int MaxHealth;
    public int HealthDecreaseOnMiss;
    public int HealthIncreaseOnHit;
    public int MaxMovementMultipler;
    public int MultiplierDecreaseOnMiss;
    public int MultiplierIncreaseOnHit;
    public int MultiplierDecreaseOnSpam;
    public float MissShakeAmount;

    public GridCell CurrentCell { get; set; }
    public bool HasMoved { get; set; }
    public Vector3 LastDirection { get; set; }
    public float CenterAppearProbability { get; set; }
    public Segment HeadSegment { get; set; }
    public bool MovedOnce { get; set; }

    private Tweener _currentMove;

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

    private Segment _lastSegment;
    private Queue<Vector3> _moveQueue;
    private Queue<bool> _growQueue;
    private bool _moving;
    private Transform _segmentsContainer;
    private int _score;
    private int _movementMultiplier;
    private BeatIndicator _beatIndicator;
    private int _health;
    private List<Segment> _segments;

    /*private Button[] _buttons = {
        new Button(Vector2.left, KeyCode.LeftArrow),
        new Button(Vector2.up, KeyCode.UpArrow),
        new Button(Vector2.right, KeyCode.RightArrow),
        new Button(Vector2.down, KeyCode.DownArrow),
    };*/

    private void Start()
    {
        _segments = new List<Segment>();
        _beatIndicator = MainPanel.Instance.BeatIndicator;
        _gridPlayground = FindObjectOfType<GridPlayground>();
        _playerCollider = GetComponent<CircleCollider2D>();

        HeadSegment = Instantiate(SegmentPrefab, transform).GetComponent<Segment>();
        _segments.Add(HeadSegment);
        HeadSegment.Center.enabled = true;
        Destroy(HeadSegment.GetComponent<BoxCollider2D>());
        
        _lastSegment = HeadSegment;

        _moveQueue = new Queue<Vector3>();
        _growQueue = new Queue<bool>();

        var segments = GameObject.Find("Segments");

        _segmentsContainer = segments == null ? new GameObject("Segments").transform : segments.transform;
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
        else if ((MainManager.Instance.CurrentState == MainManager.GameState.Play) && !MainPanel.Instance.BeatIndicator.IsHot || HasMoved)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Keypad8))
            {
                _beatIndicator.CreateDummyMetronome(false);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                _beatIndicator.CreateDummyMetronome(false);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Keypad6))
            {
                _beatIndicator.CreateDummyMetronome(false);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Keypad4))
            {
                _beatIndicator.CreateDummyMetronome(false);
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
            _gridPlayground.Foods.Remove(food);
            food.Zone.FoodObjects.Remove(food);
            food.Zone.TryClear();
            other.gameObject.transform.DOScale(0f, MainManager.Instance.PulseTime).OnComplete(() => Destroy(other.gameObject));
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
            if (gridCell.Content != null && gridCell.Content.GetComponent<Obstacle>() != null)
            {
                
            }
            else
            {
                gridCell.Content = gameObject;
                CurrentCell = gridCell;

                if (MainPanel.Instance.BeatIndicator.Bar != CurrentCell.ZoneModifier.Bar)
                {
                    MainPanel.Instance.BeatIndicator.UpdateBarAtNextBeat = true;
                }
            }
        }
    }

    public void GiveScore(int baseScore, bool lengthMultiplication, bool movementMultiplication)
    {
        Score += (baseScore * (lengthMultiplication ? Length : 1) * (movementMultiplication ? _movementMultiplier : 1)) + UnityEngine.Random.Range(0, 6);
    }

    public void Die()
    {
        AudioManager.Instance.PlayOtherSFX(AudioManager.Instance.GameEnd);
        
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
            obstacle.Clear();
        }

        var zones = FindObjectsOfType<Zone>();

        foreach (var zone in zones)
        {
            zone.Delete();
        }

        _playerCollider.enabled = false;
        MainPanel.Instance.BeatIndicator.StopBeat();

        StartCoroutine(DoDie());
    }

    private IEnumerator DoDie()
    {
        MainManager.Instance.CurrentState = MainManager.GameState.NONE;

        if (_currentMove != null)
        {
            _currentMove.Kill();
        }

        foreach (var segment in _segments)
        {
            if (segment.CurrentMove != null)
            {
                segment.CurrentMove.Kill();
            }
        }

        var reversedSegments = new List<Segment>(_segments);
        reversedSegments.Reverse();
        
        yield return new WaitForSeconds(DeathShrinkDelay);

        for (var i = 0; i < reversedSegments.Count; i++)
        {
            var segment = reversedSegments[i];

            var canGo = false;

            if (segment.PreviouSegment == null)
            {
                segment.transform.DOScale(0f, DeathShrinkTime).OnComplete(() =>
                {
                    canGo = true;
                });
            }
            else
            {
                segment.Center.transform.DOScale(0f, DeathShrinkTime);
                segment.transform.DOMove(segment.PreviouSegment.transform.position, DeathShrinkTime).SetEase(MoveCurve).OnComplete(() =>
                {
                    foreach (var frontDummySegment in segment.FrontDummySegments)
                    {
                        Destroy(frontDummySegment.gameObject);
                    }

                    Destroy(segment.gameObject);
                    canGo = true;
                });
            }

            yield return new WaitForSeconds(DeathShrinkDelay);

            yield return new WaitUntil(() => canGo);
        }

        yield return new WaitForSeconds(DeathShrinkDelay);

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

        AudioManager.Instance.PlayOtherSFX(AudioManager.Instance.GameStart);

        _beatIndicator.Bar = _beatIndicator.BaseBar;
        _beatIndicator.CreatePassiveBeats();
        _beatIndicator.CreateActiveBeats();
        _beatIndicator.StartMetronome();
    }
    
    public void QueueMove(Vector3 direction)
    {
        HasMoved = true;
        
        if (MainManager.Instance.CurrentState == MainManager.GameState.Play)
        {
            if (_beatIndicator.CurrentActiveBeat != null)
            {
                AudioManager.Instance.PlayActivatedBeat();
                _beatIndicator.CurrentActiveBeat.Image.color = _beatIndicator.CurrentActiveBeat.SuccessColor;
                _beatIndicator.CurrentActiveBeat.Activated = true;
                _beatIndicator.CreateDummyMetronome(true);
            }
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
        if (MainManager.Instance.CurrentState == MainManager.GameState.Play)
        {
            AudioManager.Instance.PlayOtherSFX(AudioManager.Instance.GetFood);
        }

        if (_moving)
        {
            _growQueue.Enqueue(true);
            return;
        }

        Grow();
    }

    public void FailBeat()
    {
        if (!MovedOnce)
        {
            return;
        }

        Health -= HealthDecreaseOnMiss;
        MovementMultiplier -= MultiplierDecreaseOnMiss;
        MainManager.Instance.CameraShake.Shake(MissShakeAmount, MainManager.Instance.PulseTime);
    }

    private void Move(Vector3 direction)
    {
        MovementMultiplier += MultiplierIncreaseOnHit;
        Health += HealthIncreaseOnHit;
        
        _moving = true;

        LastDirection = direction;

        var playerMoveDestination = transform.position + direction * _gridPlayground.MoveDistance;
        _currentMove = transform.DOMove(playerMoveDestination, MainManager.Instance.CurrentState == MainManager.GameState.Play ? PlayMoveTime : SetupMoveTime).SetEase(MoveCurve);

        HeadSegment.Move(playerMoveDestination, MainManager.Instance.CurrentState == MainManager.GameState.Play ? PlayMoveTime : SetupMoveTime);

        if (MainManager.Instance.CurrentState == MainManager.GameState.Play)
        {
            var cameraMoveDestination = playerMoveDestination;
            cameraMoveDestination.z = -10f;

            Camera.main.transform.DOMove(cameraMoveDestination, PlayMoveTime).SetEase(MoveCurve);

            if (!MovedOnce)
            {
                MovedOnce = true;
            }
        }

        _currentMove.onComplete += MovementCallback;
    }
    
    public void Grow()
    {
        var spawnPosition = _lastSegment.transform.position;

        var newSegment = Instantiate(SegmentPrefab, spawnPosition, Quaternion.identity).GetComponent<Segment>();
        newSegment.FrontDummySegments = new DummySegment[IntermediateSegments + 1];
        newSegment.transform.SetParent(_segmentsContainer, true);
        _segments.Add(newSegment);

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

        for (var i = 0; i < IntermediateSegments + 1; i++)
        {
            var newDummySegment = Instantiate(DummySegmentPrefab, _lastSegment.transform.position, Quaternion.identity).GetComponent<DummySegment>();
            newDummySegment.GetComponentInChildren<SpriteRenderer>().color = GetComponentInChildren<SpriteRenderer>().color;

            newDummySegment.Initialize(
                newSegment.PreviouSegment, 
                newSegment, 
                i, 
                IntermediateSegments, 
                MainManager.Instance.CurrentState == MainManager.GameState.Play ? PlayMoveTime : SetupMoveTime,
                MoveCurve
            );

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

            if (MainManager.Instance.PrepMovesExecuted >= MainManager.Instance.StartMoves)
            {
                MainManager.Instance.PlayerNamePanel.ToggleConfirm(true);
            }
        }

        _currentMove = null;
    }

    public void PulseHeadSegment()
    {
        for (int i = 0; i < _segments.Count; i++)
        {
            if (_segments[i].Center.GetComponent<SpriteRenderer>().enabled)
            {
                _segments[i].Center.transform.DOPunchScale(Vector3.one * MainManager.Instance.PulseFactor, 0.2f);
            }
        }
    }
}