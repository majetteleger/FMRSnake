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

    public Bar Bar;
    public AudioClip LowBeat;
    public AudioClip HighBeat;
    public float MoveTime;
    public GameObject SegmentPrefab;
    public Bar[] Bars;
    
    private Transform _tailParent;
    private Vector3 _lastSegmentPosition;
    private Vector3 _prevHeadPosition;
    private AudioSource _beatSource;
    private GridPlayground _gridPlayground;
    private GridCell _currentCell;
    private Vector3 _lastDirection;

    private Button[] _buttons = {
        new Button(0, new Vector2(-1, 0), false),
        new Button(1, new Vector2(0, 1), false),
        new Button(2, new Vector2(1, 0), false),
        new Button(3, new Vector2(0, -1), false), 
    };
    
	private void Start()
    {
        _tailParent = new GameObject("Tail Parent").GetComponent<Transform>();

        _beatSource = GetComponent<AudioSource>();
        _gridPlayground = FindObjectOfType<GridPlayground>();

        Instantiate(SegmentPrefab, transform);
        
        _lastDirection = Vector3.right;
    }

    private void Update()
    {
        if (MainManager.Instance.GamePaused)
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
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var food = other.GetComponent<Food>();
        var gridCell = other.GetComponent<GridCell>();
        var segment = other.GetComponent<Segment>();

        if (food != null)
        {
            Destroy(other.gameObject);
            Grow();
        }
        else if (gridCell != null)
        {
            gridCell.Content = gameObject;
            _currentCell = gridCell;
        }
        else if (segment != null)
        {
            Debug.Log("Colliding with Tail!");
            //StartCoroutine(Die());
        }
    }

    private IEnumerator Die()
    {
        MainManager.Instance.GamePaused = true;

        yield return new WaitForSeconds(1);
        //end sequence
    }

    public void StartGame()
    {
        StartCoroutine(Beat());
        transform.position = MainManager.Instance.GridPlayground.PlayerSpawnPoint;
        _tailParent.transform.position = MainManager.Instance.GridPlayground.PlayerSpawnPoint;
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
            Move(offButtons[0].Direction);

            return true;
        }

        return false;
    }

    public void Move(Vector3 direction)
    {
        for (var i = 0; i < _tailParent.childCount; i++)
        {
            _tailParent.GetChild(i).GetComponent<Segment>().Move();
        }

        if (_tailParent.childCount > 0)
        {
            _lastSegmentPosition = _tailParent.GetChild(_tailParent.childCount - 1).position;
        }

        _prevHeadPosition = transform.position;
        
        var movement = transform.DOMove(transform.position + direction * _gridPlayground.MoveDistance, MoveTime);

        // Horizontal to vertical
        if (Mathf.Abs(_lastDirection.x) > Mathf.Abs(_lastDirection.y) && Mathf.Abs(direction.x) < Mathf.Abs(direction.y))
        {
            if (direction.y > 0)
            {
                transform.DORotate(new Vector3(0f, 0f, 90f), MoveTime);
            }
            else if (direction.y < 0)
            {
                transform.DORotate(new Vector3(0f, 0f, -90f), MoveTime);
            }
        }
        // Vertical to horizontal
        else if (Mathf.Abs(_lastDirection.x) < Mathf.Abs(_lastDirection.y) && Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0)
            {
                transform.DORotate(new Vector3(0f, 0f, 0f), MoveTime);
            }
            else if (direction.x < 0)
            {
                transform.DORotate(new Vector3(0f, 0f, 180f), MoveTime);
            }
        }

        _lastDirection = direction;

        movement.onComplete += MovementCallback;
    }
    
    public void Grow()
    {
        var newSegment = (Segment) null;

        if (_tailParent.childCount > 0)
        {
            newSegment = Instantiate(SegmentPrefab, _lastSegmentPosition, Quaternion.identity, _tailParent).GetComponent<Segment>();
            newSegment.LastDirection = _lastSegmentPosition - newSegment.transform.position;
        }
        else
        {
            newSegment = Instantiate(SegmentPrefab, _prevHeadPosition, Quaternion.identity, _tailParent).GetComponent<Segment>();
            newSegment.LastDirection = _lastDirection;
        }
        
        newSegment.Player = this;
    }

    private void MovementCallback()
    {
        var debugString = string.Empty;

        for (var i = 0; i < _currentCell.ZoneModifiers.Count; i++)
        {
            debugString += _currentCell.ZoneModifiers[i].Color + (i != _currentCell.ZoneModifiers.Count - 1 ? ", " : string.Empty);
        }

        //Debug.Log(debugString);
    }
}