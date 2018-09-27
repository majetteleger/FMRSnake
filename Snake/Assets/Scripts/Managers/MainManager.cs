using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

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
        LeaderBoard,
        NONE
    }

    public static MainManager Instance;
    
    public GameObject PlayerPrefab;

    [Header("General")]
    public GameObject DesktopCanvas;
    public GameObject MobileCanvas;
    public float DesktopBlueLineXPosition;
    public float MobileBlueLineXPosition;
    public SpriteRenderer StationsRenderer;
    public Sprite DesktopStations;
    public Sprite MobileStations;
    public float TransitionTime;
    public int StartSegments;
    public int StartMoves;
    public int MobileStartMoves;
    public float PulseFactor;
    public float PulseTime;
    public CameraShake CameraShake;

    [Header("Camera Anchors")]
    public Transform DesktopMainMenuAnchor;
    public Transform DesktopBuildYourSnakeAnchor;
    public Transform DesktopLeaderBoardAnchor;
    public Transform MobileMainMenuAnchor;
    public Transform MobileBuildYourSnakeAnchor;
    public Transform MobileLeaderBoardAnchor;
    public Transform MainMenuAnchor { get; set; }
    public Transform BuildYourSnakeAnchor { get; set; }
    public Transform LeaderBoardAnchor { get; set; }

    [Header("Metro")]
    public Color[] MetroColors;
    public MetroLine[] MetroLines;
    public GameObject MetroLinesContainer;
    public float MainMenuFadeTime;

    [Header("UI")]
    public PlayerNamePanel MobilePlayerNamePanel;
    public PlayerNamePanel DesktopPlayerNamePanel;

    public string[] LeaderboardEntries;
    public bool CanReachLeaderboard;
    public GameState CurrentState { get; set; }
    public Player Player { get; set; }
    public GridPlayground GridPlayground { get; set; }
    public LeaderBoardEntry CurrentPlayerEntry { get; set; }
    public List<LeaderBoardEntry> LeaderBoard { get; set; }
    public string CurrentPlayerName { get; set; }
    public int PrepMovesExecuted { get; set; }
    public int SelectedLineIndex { get; set; }
    public int ActualStartMoves { get; set; }
    public PlayerNamePanel PlayerNamePanel { get; set; }

    public int CurrentPlayerLeaderboardIndex
    {
        get { return LeaderBoard.FindIndex(x => x == CurrentPlayerEntry); }
    }
    
    private int _newLeaderBoardId;
    private SpriteRenderer[] _mainMenuRenderers;
    private Vector3 _buildYourSnakeActualAnchor;
    private string _url = "http://ligneleaderboard.herokuapp.com";
    private string _leaderboardRaw;

    private void Awake()
    {
        Instance = this;

        if (Screen.width < Screen.height)
        {
            ActualStartMoves = MobileStartMoves;
            Camera.main.orthographicSize = 4f;
            DesktopCanvas.SetActive(false);
            MobileCanvas.SetActive(true);
            MainMenuAnchor = MobileMainMenuAnchor;
            BuildYourSnakeAnchor = MobileBuildYourSnakeAnchor;
            LeaderBoardAnchor = MobileLeaderBoardAnchor;
            MetroLines[0].transform.localPosition = new Vector3(MobileBlueLineXPosition, MetroLines[0].transform.localPosition.y, MetroLines[0].transform.localPosition.z);
            StationsRenderer.sprite = MobileStations;
            PlayerNamePanel = MobilePlayerNamePanel;
        }
        else
        {
            ActualStartMoves = StartMoves;
            Camera.main.orthographicSize = 3.1f;
            DesktopCanvas.SetActive(true);
            MobileCanvas.SetActive(false);
            MainMenuAnchor = DesktopMainMenuAnchor;
            BuildYourSnakeAnchor = DesktopBuildYourSnakeAnchor;
            LeaderBoardAnchor = DesktopLeaderBoardAnchor;
            MetroLines[0].transform.localPosition = new Vector3(DesktopBlueLineXPosition, MetroLines[0].transform.localPosition.y, MetroLines[0].transform.localPosition.z);
            StationsRenderer.sprite = DesktopStations;
            PlayerNamePanel = DesktopPlayerNamePanel;
        }
    }

    private void Start()
    {
        GridPlayground = FindObjectOfType<GridPlayground>();
        LeaderBoard = new List<LeaderBoardEntry>();
        _mainMenuRenderers = MetroLinesContainer.GetComponentsInChildren<SpriteRenderer>();
        
        TransitionToMainMenu(true);
    }

    public void InputUp()
    {
        switch (CurrentState)
        {
            case GameState.MainMenu:

                TransitionToBuildYourSnake();
                AudioManager.Instance.PlayOtherSFX(AudioManager.Instance.MenuInteraction);

                break;

            case GameState.BuildYourSnake:

                PlayerNamePanel.Input(KeyCode.UpArrow);
                AudioManager.Instance.PlayOtherSFX(AudioManager.Instance.MenuInteraction);

                break;

            case GameState.Play:

                Player.Input(Vector3.up);

                break;

            case GameState.LeaderBoard:

                if (MainPanel.Instance.WarningPanel.activeSelf)
                {
                    MainPanel.Instance.WarningPanel.SetActive(false);
                }
                TransitionToMainMenu();
                AudioManager.Instance.PlayOtherSFX(AudioManager.Instance.MenuInteraction);

                break;
        }
    }

    public void InputRight()
    {
        switch (CurrentState)
        {
            case GameState.MainMenu:

                SelectedLineIndex++;

                if (SelectedLineIndex > MetroLines.Length - 1)
                {
                    SelectedLineIndex = 0;
                }

                AudioManager.Instance.PlayOtherSFX(AudioManager.Instance.MenuInteraction);

                UpdateSelectedLine();

                break;

            case GameState.BuildYourSnake:

                PlayerNamePanel.Input(KeyCode.RightArrow);
                AudioManager.Instance.PlayOtherSFX(AudioManager.Instance.MenuInteraction);

                break;

            case GameState.Play:

                Player.Input(Vector3.right);

                break;

            case GameState.LeaderBoard:
                break;
        }
    }

    public void InputDown()
    {
        switch (CurrentState)
        {
            case GameState.MainMenu:

                TransitionToLeaderBoard();
                AudioManager.Instance.PlayOtherSFX(AudioManager.Instance.MenuInteraction);

                break;

            case GameState.BuildYourSnake:

                PlayerNamePanel.Input(KeyCode.DownArrow);
                AudioManager.Instance.PlayOtherSFX(AudioManager.Instance.MenuInteraction);

                break;

            case GameState.Play:

                Player.Input(Vector3.down);

                break;

            case GameState.LeaderBoard:
                break;
        }
    }

    public void InputLeft()
    {
        switch (CurrentState)
        {
            case GameState.MainMenu:
                
                SelectedLineIndex--;

                if (SelectedLineIndex < 0)
                {
                    SelectedLineIndex = MetroLines.Length - 1;
                }

                AudioManager.Instance.PlayOtherSFX(AudioManager.Instance.MenuInteraction);

                UpdateSelectedLine();

                break;

            case GameState.BuildYourSnake:

                PlayerNamePanel.Input(KeyCode.LeftArrow);
                AudioManager.Instance.PlayOtherSFX(AudioManager.Instance.MenuInteraction);

                break;

            case GameState.Play:

                Player.Input(Vector3.left);

                break;

            case GameState.LeaderBoard:
                break;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Keypad6))
        {
            InputRight();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Keypad4))
        {
            InputLeft();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Keypad8))
        {
            InputUp();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            InputDown();
        }
    }

    public void TransitionToMainMenu(bool instant = false)
    {
        CurrentState = GameState.MainMenu;
        Camera.main.transform.DOMove(MainMenuAnchor.position, instant ? 0f : TransitionTime);

        SelectedLineIndex = UnityEngine.Random.Range(0, MetroLines.Length);
        UpdateSelectedLine();

        MetroLinesContainer.SetActive(true);
        MainPanel.Instance.TransitionToMainMenu();
        PlayerNamePanel.gameObject.SetActive(false);
        ResetSnake();
        //LoadLeaderBoard();
        DownloadScores();

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

        Camera.main.transform.DOMove(_buildYourSnakeActualAnchor, TransitionTime);

        Player.GetComponentInChildren<SpriteRenderer>().color = MetroLines[SelectedLineIndex].Color;
        PrepMovesExecuted = 0;

        for (var i = 0; i < ActualStartMoves; i++)
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
        if (CurrentState == GameState.NONE)
        {
            SaveScore(CurrentPlayerName, SelectedLineIndex);
            SendScore();
        }

        CurrentState = GameState.LeaderBoard;
        Camera.main.transform.DOMove(LeaderBoardAnchor.position, TransitionTime);
        UpdateSelectedLine(true);
        MetroLinesContainer.SetActive(true);
        AudioManager.Instance.FadeAmbianceTo(AudioManager.Instance.AmbianceMenu);
        MainPanel.Instance.TransitionToLeaderBoard();
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

        var spawnPosition = FindNearestCellPosition(approximatePosition, ActualStartMoves);
        
        Player = Instantiate(PlayerPrefab, spawnPosition, Quaternion.identity).GetComponent<Player>();
    }

    public void SendScore()
    {
        Debug.Log("sending score");
        StartCoroutine(DoSendScore(CurrentPlayerName, SelectedLineIndex, Player.Score));
    }

    private IEnumerator DoSendScore(string name, int color, int score)
    {
        WWW www = new WWW(_url + "/" + name + "/" + color + "/" + score);
        yield return www;

        if (string.IsNullOrEmpty(www.error))
            print("Upload Successful");
        else
        {
            print("Error uploading: " + www.error);
            CanReachLeaderboard = false;
        }
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

    private void DownloadScores()
    {
        Debug.Log("Querying database");
        StartCoroutine("DownloadScoresFromDatabase");
    }

    private IEnumerator DownloadScoresFromDatabase()
    {
        WWW www = new WWW(_url + "/getscores");
        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            _leaderboardRaw = www.text;
            FormatLeaderboard(_leaderboardRaw);
            ParseLeaderboardEntries();
        }
        else
        {
            print("Error Downloading: " + www.error);
            //trigger warning;
        }
    }

    private void FormatLeaderboard(string raw)
    {
        LeaderboardEntries = raw.Split('|');
        Debug.Log(LeaderboardEntries);
    }

    private void ParseLeaderboardEntries()
    {
        LeaderBoard.Clear();

        var currentId = 0;
        //var currentIdString = currentId.ToString();
        var currentValue = LeaderboardEntries[currentId]; /*PlayerPrefs.GetString(currentIdString);*/

        while (!string.IsNullOrEmpty(currentValue))
        {
            var values = currentValue.Split(':');
            LeaderBoard.Add(new LeaderBoardEntry(values[0], int.Parse(values[1]), int.Parse(values[2])));

            currentId++;
            //currentIdString = currentId.ToString();
            currentValue = LeaderboardEntries[currentId];
        }

        LeaderBoard = LeaderBoard.OrderByDescending(x => x.Score).ToList();
        _newLeaderBoardId = currentId;
    }
    
    private void UpdateSelectedLine(bool off = false)
    {
        for (var i = 0; i < MetroLines.Length; i++)
        {
            if (!off && i == SelectedLineIndex)
            {
                MetroLines[i].Emphasize();
                continue;
            }

            MetroLines[i].DeEmphasize();
        }
    }

    private Vector3 FindNearestCellPosition(Vector3 approximatePosition, int leftGridOffset = 0)
    {
        _buildYourSnakeActualAnchor = BuildYourSnakeAnchor.position;

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

        _buildYourSnakeActualAnchor = newAnchorPosition;

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
