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
    public Transform TailParent;

    private Vector3 _lastSegmentPosition;
    private Vector3 _prevHeadPosition;
    private AudioSource _beatSource;
    private GridPlayground _gridPlayground;
    private GridCell _currentCell;

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

        StartCoroutine(Beat());
	}

    private void Update()
    {
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

        if (food != null)
        {
            Destroy(other.gameObject);
            Grow();
        }
        else if (gridCell != null)
        {
            _currentCell = gridCell;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var gridCell = other.GetComponent<GridCell>();

        if (gridCell != null && _currentCell != null && _currentCell == gridCell)
        {
            _currentCell = null;
        }
    }

    private IEnumerator Beat()
    {
        for (int i = 0; i < Bar.Beats.Count; i++)
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

        for (int i = 0; i < _buttons.Length; i++)
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

    private void Move(Vector3 direction)
    {
        for (int i = 0; i < TailParent.childCount; i++)
        {
            if (i == TailParent.childCount - 1)
            {
                _lastSegmentPosition = TailParent.GetChild(i).position;
            }
            if (i == 0)
            {
                TailParent.GetChild(0).DOMove(transform.position, MoveTime);
            }
            else
            {
                TailParent.GetChild(i).DOMove(TailParent.GetChild(i - 1).position, MoveTime);
            }   
        }

        _prevHeadPosition = transform.position;

        var movement = transform.DOMove(transform.position + direction * _gridPlayground.MoveDistance, MoveTime);

        movement.onComplete += MovementCallback;
    }
    
    private void Grow()
    {
        if (TailParent.childCount > 0)
        {
            Instantiate(SegmentPrefab, _lastSegmentPosition, Quaternion.identity, TailParent);
        }
        else
        {
            Instantiate(SegmentPrefab, _prevHeadPosition, Quaternion.identity, TailParent);
        }
    }

    private void MovementCallback()
    {
        var debugString = string.Empty;

        for (var i = 0; i < _currentCell.ZoneModifiers.Count; i++)
        {
            debugString += _currentCell.ZoneModifiers[i].Color + (i != _currentCell.ZoneModifiers.Count - 1 ? ", " : string.Empty);
        }

        Debug.Log(debugString);
    }
}