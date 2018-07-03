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
    public Bar Bar;
    public Bar[] Bars;
    public AudioClip LowBeat;
    public AudioClip HighBeat;
    public float MoveTime;
    public int IntermediateSegments;
    public float CenterAppearProbabilityIncrement;

    public Vector3 LastDirection { get; set; }
    public float CenterAppearProbability { get; set; }

    private CircleCollider2D _playerCollider;
    private Vector3 _prevHeadPosition;
    private AudioSource _beatSource;
    private GridPlayground _gridPlayground;
    private GridCell _currentCell;
    private Segment _headSegment;
    private Segment _lastSegment;
    private Queue<Vector3> _moveQueue;
    private Queue<bool> _growQueue;
    private bool _moving;
    private Transform _segmentsContainer;

    private Button[] _buttons = {
        new Button(0, new Vector2(-1, 0), false),
        new Button(1, new Vector2(0, 1), false),
        new Button(2, new Vector2(1, 0), false),
        new Button(3, new Vector2(0, -1), false),
    };

    private void Start()
    {
        _beatSource = GetComponent<AudioSource>();
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

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Bar = Bars[UnityEngine.Random.Range(0, Bars.Length)];
        }
        // // Simpler but seems to bug sometimes when I mash keys quickly
        //_buttons[0] = Input.GetKey(KeyCode.LeftArrow);
        //_buttons[1] = Input.GetKey(KeyCode.UpArrow);
        //_buttons[2] = Input.GetKey(KeyCode.RightArrow);
        //_buttons[3] = Input.GetKey(KeyCode.DownArrow);

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
            food.Zone.FoodObjects.Remove(food);
            food.Zone.TryClear();
            Destroy(other.gameObject);
            QueueGrow();
        }
        else if (gridCell != null)
        {
            gridCell.Content = gameObject;
            _currentCell = gridCell;
        }
        else if (segment != null)
        {
            Debug.Log("Colliding with Tail!");
            Die();
        }
    }

    public void Die()
    {
        StopAllCoroutines();
        StartCoroutine(DoDie());
    }

    private IEnumerator DoDie()
    {
        yield return new WaitForSeconds(1);

        // destroy zones
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
        StartCoroutine(Beat());
    }

    private IEnumerator Beat()
    {
        for (var i = 0; i < Bar.Beats.Count; i++)
        {
            if (!Bar.Beats[i].IsHigh)
            {
                _beatSource.clip = LowBeat;
            }
            else
            {
                _beatSource.clip = HighBeat;

                var success = AttemptMove();

                if (!success)
                {
                    FailMove();
                }
            }

            _beatSource.Play();

            yield return new WaitForSeconds(Bar.Beats[i].Delay);
        }

        StartCoroutine(Beat());
    }

    private void FailMove()
    {
        //Debug.Log("Did not move!");
    }

    private bool AttemptMove()
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