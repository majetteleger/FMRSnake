using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    public enum GameState
    {
        MainMenu,
        BuildYourSnake,
        Play,
        LeaderBoard
    }

    public static MainManager Instance;
    
    public GameObject PlayerPrefab;

    [Header("State Machine")]
    public float TransitionTime;

    [Header("Camera Anchors")]
    public Transform MainMenuAnchor;
    public Transform BuildYourSnakeAnchor;
    public Transform LeaderBoardAnchor;

    [Header("Metro")]
    public MetroLine[] MetroLines;
    public GameObject MetroLinesContainer;

    public GameState CurrentState { get; set; }
    public Player Player { get; set; }
    public GridPlayground GridPlayground { get; set; }

    private int _selectedLineIndex;
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GridPlayground = FindObjectOfType<GridPlayground>();
        
        _selectedLineIndex = 1;
        TransitionToMainMenu();
    }

    private void Update()
    {
        switch (CurrentState)
        {
            case GameState.MainMenu:

                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    _selectedLineIndex++;

                    if (_selectedLineIndex > MetroLines.Length - 1)
                    {
                        _selectedLineIndex = 0;
                    }

                    UpdateSelectedLine();
                }

                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    _selectedLineIndex--;

                    if (_selectedLineIndex < 0)
                    {
                        _selectedLineIndex = MetroLines.Length - 1;
                    }

                    UpdateSelectedLine();
                }

                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    TransitionToBuildYourSnake();
                }

                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    TransitionToLeaderBoard();
                }

                break;

            case GameState.BuildYourSnake:

                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    AddPlayer();
                }

                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    RemovePlayer();
                }

                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    TransitionToPlay();
                }

                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    TransitionToMainMenu();
                }

                break;

            case GameState.Play:

                // DEBUG
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Player.Die();
                }
                //

                break;

            case GameState.LeaderBoard:

                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    TransitionToMainMenu();
                }

                break;
        }
    }

    public void TransitionToMainMenu()
    {
        CurrentState = GameState.MainMenu;
        Camera.main.transform.DOMove(MainMenuAnchor.position, TransitionTime);
        UpdateSelectedLine();
        MetroLinesContainer.SetActive(true);
        MainPanel.Instance.TransitionToMainMenu();
        ResetSnake();
    }

    public void TransitionToBuildYourSnake()
    {
        CurrentState = GameState.BuildYourSnake;
        Camera.main.transform.DOMove(BuildYourSnakeAnchor.position, TransitionTime);
        UpdateSelectedLine(true);
        MainPanel.Instance.TransitionToBuildYourSnake();

        Player.GetComponentInChildren<SpriteRenderer>().color = MetroLines[_selectedLineIndex].Color;
    }

    public void TransitionToPlay()
    {
        CurrentState = GameState.Play;
        UpdateSelectedLine(true);
        MetroLinesContainer.SetActive(false);
        MainPanel.Instance.TransitionToPlay();

        Player.StartGame();
        //GridPlayground.ShowCells();
    }

    public void TransitionToLeaderBoard()
    {
        CurrentState = GameState.LeaderBoard;
        Camera.main.transform.DOMove(LeaderBoardAnchor.position, TransitionTime);
        UpdateSelectedLine(true);
        MetroLinesContainer.SetActive(true);
        MainPanel.Instance.TransitionToLeaderBoard();
        ResetSnake();

        var foodObjects = FindObjectsOfType<Food>();

        foreach (var foodObject in foodObjects)
        {
            Destroy(foodObject.gameObject);
        }
    }

    public void ResetSnake()
    {
        if (Player != null)
        {
            Player.Destroy();
        }

        var approximatePosition = BuildYourSnakeAnchor.transform.position;
        approximatePosition.z = 0f;

        var spawnPosition = FindNearestCellPosition(approximatePosition);

        Player = Instantiate(PlayerPrefab, spawnPosition, Quaternion.identity).GetComponent<Player>();
    }

    private void AddPlayer()
    {
        Player.Grow();
        Player.QueueMove(Vector3.right);

        //
    }

    private void RemovePlayer()
    {
        //
    }
    
    private void UpdateSelectedLine(bool off = false)
    {
        for (var i = 0; i < MetroLines.Length; i++)
        {
            if (!off && i == _selectedLineIndex)
            {
                MetroLines[i].Emphasize();
                continue;
            }

            MetroLines[i].DeEmphasize();
        }
    }

    private Vector3 FindNearestCellPosition(Vector3 approximatePosition)
    {
        var gridCells = Physics2D.OverlapCircleAll(approximatePosition, GridPlayground.CellSize).Select(x => x.transform).ToArray();

        var distance = float.MaxValue;
        var resultCell = (Transform) null;

        foreach (var gridCell in gridCells)
        {
            var tempDistance = Vector2.Distance(approximatePosition, transform.position);

            if (tempDistance < distance)
            {
                distance = tempDistance;
                resultCell = gridCell;
            }
        }

        return resultCell.position;
    }
}
