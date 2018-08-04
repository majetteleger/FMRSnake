using System;
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
        public Color Color;

        public LeaderBoardEntry(string displayName, int score, int color)
        {
            DisplayName = displayName;
            Score = score;
            Color = MainManager.Instance.MetroColors[color];
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
    public int StartMoves;
    public float PulseFactor;

    [Header("Camera Anchors")]
    public Transform MainMenuAnchor;
    public Transform BuildYourSnakeAnchor;
    public Transform LeaderBoardAnchor;

    [Header("Metro")]
    public Color[] MetroColors;
    public MetroLine[] MetroLines;
    public GameObject MetroLinesContainer;
    public float MainMenuFadeTime;

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
    private SpriteRenderer[] _mainMenuRenderers;
    private Vector3 BuildYourSnakeActualAnchor;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GridPlayground = FindObjectOfType<GridPlayground>();
        LeaderBoard = new List<LeaderBoardEntry>();
        _mainMenuRenderers = MetroLinesContainer.GetComponentsInChildren<SpriteRenderer>();
        
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

                    AudioManager.Instance.PlayOtherSFX(AudioManager.Instance.MenuInteraction);

                    UpdateSelectedLine();
                }

                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Keypad4))
                {
                    _selectedLineIndex--;

                    if (_selectedLineIndex < 0)
                    {
                        _selectedLineIndex = MetroLines.Length - 1;
                    }

                    AudioManager.Instance.PlayOtherSFX(AudioManager.Instance.MenuInteraction);

                    UpdateSelectedLine();
                }

                if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Keypad8))
                {
                    TransitionToBuildYourSnake();
                    AudioManager.Instance.PlayOtherSFX(AudioManager.Instance.MenuInteraction);
                }

                if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.Keypad2))
                {
                    TransitionToLeaderBoard();
                    AudioManager.Instance.PlayOtherSFX(AudioManager.Instance.MenuInteraction);
                }

                break;

            case GameState.BuildYourSnake:

                if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Keypad6))
                {
                    PlayerNamePanel.Input(KeyCode.RightArrow);
                    AudioManager.Instance.PlayOtherSFX(AudioManager.Instance.MenuInteraction);
                }

                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Keypad4))
                {
                    PlayerNamePanel.Input(KeyCode.LeftArrow);
                    AudioManager.Instance.PlayOtherSFX(AudioManager.Instance.MenuInteraction);
                }

                if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Keypad8))
                {
                    PlayerNamePanel.Input(KeyCode.UpArrow);
                    AudioManager.Instance.PlayOtherSFX(AudioManager.Instance.MenuInteraction);
                }

                if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.Keypad2))
                {
                    PlayerNamePanel.Input(KeyCode.DownArrow);
                    AudioManager.Instance.PlayOtherSFX(AudioManager.Instance.MenuInteraction);
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
                    AudioManager.Instance.PlayOtherSFX(AudioManager.Instance.MenuInteraction);
                }

                break;
        }
    }

    public void TransitionToMainMenu()
    {
        CurrentState = GameState.MainMenu;
        Camera.main.transform.DOMove(MainMenuAnchor.position, TransitionTime);

        _selectedLineIndex = UnityEngine.Random.Range(0, MetroLines.Length);
        UpdateSelectedLine();

        MetroLinesContainer.SetActive(true);
        MainPanel.Instance.TransitionToMainMenu();
        PlayerNamePanel.gameObject.SetActive(false);
        ResetSnake();
        LoadLeaderBoard();

        foreach (var mainMenuRenderer in _mainMenuRenderers)
        {
            mainMenuRenderer.DOFade(1f, MainMenuFadeTime);
        }
    }

    public void TransitionToBuildYourSnake()
    {
        CurrentState = GameState.BuildYourSnake;
        UpdateSelectedLine(true);
        PlayerNamePanel.gameObject.SetActive(true);
        MainPanel.Instance.TransitionToBuildYourSnake();

        Camera.main.transform.DOMove(BuildYourSnakeActualAnchor, TransitionTime);

        Player.GetComponentInChildren<SpriteRenderer>().color = MetroLines[_selectedLineIndex].Color;
        PrepMovesExecuted = 0;

        for (var i = 0; i < StartMoves; i++)
        {
            Player.QueueMove(Vector3.right);

            if (StartSegments > i)
            {
                Player.QueueGrow();
            }
        }

        foreach (var mainMenuRenderer in _mainMenuRenderers)
        {
            mainMenuRenderer.DOFade(0f, MainMenuFadeTime);
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
        SaveScore(CurrentPlayerName, _selectedLineIndex);
        Camera.main.transform.DOMove(LeaderBoardAnchor.position, TransitionTime);
        UpdateSelectedLine(true);
        MetroLinesContainer.SetActive(true);
        AudioManager.Instance.FadeAmbianceTo(AudioManager.Instance.AmbianceMenu);
        MainPanel.Instance.TransitionToLeaderBoard();
        GridPlayground.Instance.ZonesSpawned = 0;
        ResetSnake();

        foreach (var mainMenuRenderer in _mainMenuRenderers)
        {
            mainMenuRenderer.DOFade(1f, MainMenuFadeTime);
        }
    }
    
    public void ResetSnake()
    {
        if (Player != null)
        {
            Player.Destroy();
        }

        var approximatePosition = BuildYourSnakeAnchor.transform.position + new Vector3(0f, MainPanel.Instance.BuildYourSnakeCameraOffset.y);
        approximatePosition.z = 0f;

        var spawnPosition = FindNearestCellPosition(approximatePosition, StartMoves);
        
        Player = Instantiate(PlayerPrefab, spawnPosition, Quaternion.identity).GetComponent<Player>();
    }
    
    public void SaveScore(string displayName, int lineColor)
    {
        if (string.IsNullOrEmpty(displayName))
        {
            return;
        }

        var newValue = string.Format("{0}:{1}:{2}", displayName, Player.Score, lineColor);
        PlayerPrefs.SetString(_newLeaderBoardId.ToString(), newValue);

        CurrentPlayerEntry = new LeaderBoardEntry(displayName, Player.Score, lineColor);
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
            // this part is updating the existing playerprefs entry to fit the new format, we should get rid of this eventually
            if (currentValue.Split(':').Length != 3)
            {
                currentValue += ":1";
            }
            //

            var values = currentValue.Split(':');
            LeaderBoard.Add(new LeaderBoardEntry(values[0], int.Parse(values[1]), int.Parse(values[2])));

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

    private Vector3 FindNearestCellPosition(Vector3 approximatePosition, int leftGridOffset = 0)
    {
        BuildYourSnakeActualAnchor = BuildYourSnakeAnchor.position;

        var gridCells = Physics2D.OverlapCircleAll(approximatePosition, GridPlayground.CellSize).Select(x => x.transform).ToArray();

        var distance = float.MaxValue;
        var resultCell = (GridCell) null;

        foreach (var gridCell in gridCells)
        {
            var tempDistance = Vector2.Distance(approximatePosition, transform.position);

            if (tempDistance < distance)
            {
                distance = tempDistance;
                resultCell = gridCell.GetComponent<GridCell>();
            }
        }

        var newAnchorPosition = resultCell.transform.position - new Vector3(0f, MainPanel.Instance.BuildYourSnakeCameraOffset.y);
        newAnchorPosition.z = -10f;

        BuildYourSnakeActualAnchor = newAnchorPosition;

        for (var i = 0; i < leftGridOffset; i++)
        {
            if (resultCell == null)
            {
                return Vector3.zero;
            }

            resultCell = resultCell.GetAdjacentCell(ObstacleShape.Direction.Left);
        }

        return resultCell == null ? Vector3.zero : resultCell.transform.position;
    }
}
