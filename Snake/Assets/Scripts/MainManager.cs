﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    public class LeaderBoardEntry
    {
        public string DisplayName;
        public int Score;

        public LeaderBoardEntry(string displayName, int score)
        {
            DisplayName = displayName;
            Score = score;
        }
    }
    
    public enum GameState
    {
        MainMenu,
        BuildYourSnake,
        Play,
        LeaderBoard
    }

    public static MainManager Instance;
    
    public GameObject PlayerPrefab;

    [Header("General")]
    public float TransitionTime;
    public int StartSegments;

    [Header("Camera Anchors")]
    public Transform MainMenuAnchor;
    public Transform BuildYourSnakeAnchor;
    public Transform LeaderBoardAnchor;

    [Header("Metro")]
    public MetroLine[] MetroLines;
    public GameObject MetroLinesContainer;

    [Header("UI")]
    public PlayerNamePanel PlayerNamePanel;

    public GameState CurrentState { get; set; }
    public Player Player { get; set; }
    public GridPlayground GridPlayground { get; set; }
    public LeaderBoardEntry CurrentPlayerEntry { get; set; }
    public List<LeaderBoardEntry> LeaderBoard { get; set; }
    public string CurrentPlayerName { get; set; }
    public int PrepMovesExecuted { get; set; }

    public int CurrentPlayerLeaderboardIndex
    {
        get { return LeaderBoard.FindIndex(x => x == CurrentPlayerEntry); }
    }

    private int _selectedLineIndex;
    private int _newLeaderBoardId;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GridPlayground = FindObjectOfType<GridPlayground>();
        LeaderBoard = new List<LeaderBoardEntry>();
            
        _selectedLineIndex = 1;
        TransitionToMainMenu();
    }

    private void Update()
    {
        switch (CurrentState)
        {
            case GameState.MainMenu:

                if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Keypad6))
                {
                    _selectedLineIndex++;

                    if (_selectedLineIndex > MetroLines.Length - 1)
                    {
                        _selectedLineIndex = 0;
                    }

                    UpdateSelectedLine();
                }

                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Keypad4))
                {
                    _selectedLineIndex--;

                    if (_selectedLineIndex < 0)
                    {
                        _selectedLineIndex = MetroLines.Length - 1;
                    }

                    UpdateSelectedLine();
                }

                if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Keypad8))
                {
                    TransitionToBuildYourSnake();
                }

                if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.Keypad2))
                {
                    TransitionToLeaderBoard();
                }

                break;

            case GameState.BuildYourSnake:

                if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Keypad6))
                {
                    PlayerNamePanel.Input(KeyCode.RightArrow);
                }

                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Keypad4))
                {
                    PlayerNamePanel.Input(KeyCode.LeftArrow);
                }

                if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Keypad8))
                {
                    PlayerNamePanel.Input(KeyCode.UpArrow);
                }

                if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.Keypad2))
                {
                    PlayerNamePanel.Input(KeyCode.DownArrow);
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

                if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Keypad8))
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
        LoadLeaderBoard();
    }

    public void TransitionToBuildYourSnake()
    {
        CurrentState = GameState.BuildYourSnake;
        UpdateSelectedLine(true);
        PlayerNamePanel.gameObject.SetActive(true);
        MainPanel.Instance.TransitionToBuildYourSnake();

        Player.GetComponentInChildren<SpriteRenderer>().color = MetroLines[_selectedLineIndex].Color;
        PrepMovesExecuted = 0;

        for (var i = 0; i < StartSegments; i++)
        {
            Player.QueueGrow();
            Player.QueueMove(Vector3.right);
        }
    }

    public void TransitionToPlay()
    {
        CurrentState = GameState.Play;
        UpdateSelectedLine(true);
        MetroLinesContainer.SetActive(false);
        MainPanel.Instance.TransitionToPlay();
        PlayerNamePanel.gameObject.SetActive(false);
        Player.StartGame();
    }
    
    public void TransitionToLeaderBoard()
    {
        CurrentState = GameState.LeaderBoard;
        SaveScore(CurrentPlayerName);
        Camera.main.transform.DOMove(LeaderBoardAnchor.position, TransitionTime);
        UpdateSelectedLine(true);
        MetroLinesContainer.SetActive(true);
        
        MainPanel.Instance.TransitionToLeaderBoard();
        GridPlayground.Instance.ZonesSpawned = 0;
        ResetSnake();
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
    
    public void SaveScore(string displayName)
    {
        var newValue = string.Format("{0}:{1}", displayName, Player.Score);
        PlayerPrefs.SetString(_newLeaderBoardId.ToString(), newValue);

        CurrentPlayerEntry = new LeaderBoardEntry(displayName, Player.Score);
        LeaderBoard.Add(CurrentPlayerEntry);
        LeaderBoard = LeaderBoard.OrderByDescending(x => x.Score).ToList();
        
        _newLeaderBoardId++;
    }

    private void LoadLeaderBoard()
    {
        LeaderBoard.Clear();

        var currentId = 0;
        var currentIdString = currentId.ToString();
        var currentValue = PlayerPrefs.GetString(currentIdString);

        while (!string.IsNullOrEmpty(currentValue))
        {
            var values = currentValue.Split(':');
            LeaderBoard.Add(new LeaderBoardEntry(values[0], int.Parse(values[1])));

            currentId++;
            currentIdString = currentId.ToString();
            currentValue = PlayerPrefs.GetString(currentIdString);
        }

        LeaderBoard = LeaderBoard.OrderByDescending(x => x.Score).ToList();
        _newLeaderBoardId = currentId;
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
