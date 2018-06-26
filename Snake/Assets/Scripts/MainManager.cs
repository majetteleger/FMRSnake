using System;
using System.Collections;
using System.Collections.Generic;
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
    private Vector3 _playCenterPosition;
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GridPlayground = FindObjectOfType<GridPlayground>();

        _playCenterPosition = BuildYourSnakeAnchor.transform.position;
        _playCenterPosition.z = 0f;

        Player = Instantiate(PlayerPrefab, _playCenterPosition, Quaternion.identity).GetComponent<Player>();
        GridPlayground.transform.position = _playCenterPosition;

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
        
        Player.transform.position = _playCenterPosition;

        var foodObjects = FindObjectsOfType<Food>();

        foreach (var foodObject in foodObjects)
        {
            Destroy(foodObject.gameObject);
        }
    }

    private void AddPlayer()
    {
        Player.Grow();
        Player.Move(Vector3.right);

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
}
