using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MainPanel : MonoBehaviour
{
    [Serializable]
    public class ControlsText
    {
        public string Up;
        public string Right;
        public string Down;
        public string Left;

        public void ApplyControls()
        {
            Instance.UpControlText.text = Up;
            Instance.UpControlImage.DOFade(string.IsNullOrEmpty(Up) ? Instance.ControlsFadeValue : 1f, Instance.ControlsFadeTime);

            Instance.RightControlText.text = Right;
            Instance.RightControlImage.DOFade(string.IsNullOrEmpty(Right) ? Instance.ControlsFadeValue : 1f, Instance.ControlsFadeTime);

            Instance.DownControlText.text = Down;
            Instance.DownControlImage.DOFade(string.IsNullOrEmpty(Down) ? Instance.ControlsFadeValue : 1f, Instance.ControlsFadeTime);

            Instance.LeftControlText.text = Left;
            Instance.LeftControlImage.DOFade(string.IsNullOrEmpty(Left) ? 0.5f : 1f, Instance.ControlsFadeTime);
        }
    }

    public static MainPanel Instance;

    [Header("General")]

    public GameObject Title;
    public Text Header;
    public Text UpControlText;
    public Image UpControlImage;
    public Text RightControlText;
    public Image RightControlImage;
    public Text DownControlText;
    public Image DownControlImage;
    public Text LeftControlText;
    public Image LeftControlImage;
    public float ControlsFadeTime;
    public float ControlsFadeValue;
    public Color SuccessColor;
    public Color FailureColor;

    [Header("MainMenu")]

    public string MainMenuHeader;
    public ControlsText MainMenuControls;

    [Header("BuildYourSnake")]

    public string BuildYourSnakeHeader;
    public ControlsText BuildYourSnakeControls;
    public Vector2 BuildYourSnakeCameraOffset;

    [Header("Play")]

    public GameObject PlaySubPanel;
    public BeatIndicator BeatIndicator;
    public RectTransform HealthGauge;
    public Image HealthOverlay;
    public Image ScoreOverlay;
    public Image MultiplierOverlay;
    public Text ScoreText;
    public Text MovementMultiplierText;
    public float ScoreUpdateTime;
    public float MovementMultiplierUpdateTime;
    public GameObject LeaderboardPanel;
    public GameObject EntryPrefab;
    public ControlsText PlayControls;

    [Header("PlayerNameEnter")]
    public ControlsText PlayerNameEnterControls;
    public ControlsText PlayerNameEnterBackControls;
    public ControlsText PlayerNameEnterConfirmControls;

    [Header("LeaderBoard")]

    public GameObject LeaderBoardSubPanel;
    public string LeaderBoardHeader;
    public int TopDisplayedEntries;
    public int DisplayedEntriesBeforePlayer;
    public int DisplayedEntriesAfterPlayer;
    public ControlsText LeaderBoardControls;
    public LeaderboardEntry[] LeaderboardEntries;
    public Transform MiddleDotDotDot;
    public GameObject EndDotDotDot;
    public Color NormalEntryColor;
    public Color HighlightedEntryColor;
    public GameObject WarningPanel;
    public Text WarningText;

    private float _displayedScore;

    private void Awake()
    {
        Instance = this;
    }
    
    public void TransitionToMainMenu()
    {
        PlaySubPanel.SetActive(false);
        LeaderboardPanel.SetActive(false);

        foreach (var leaderboardEntry in LeaderboardEntries)
        {
            leaderboardEntry.gameObject.SetActive(false);
        }

        Title.gameObject.SetActive(true);
        Header.text = MainMenuHeader;
        MainMenuControls.ApplyControls();
    }

    public void TransitionToBuildYourSnake()
    {
        Title.gameObject.SetActive(false);
        PlaySubPanel.SetActive(false);
        LeaderboardPanel.SetActive(false);
        Header.text = BuildYourSnakeHeader;
        PlayerNameEnterControls.ApplyControls();
    }

    public void TransitionToPlay()
    {
        Title.gameObject.SetActive(false);
        PlaySubPanel.SetActive(true);
        LeaderboardPanel.SetActive(false);
        Header.text = string.Empty;

        // IDEA: could fade control a bit during play mode to make it less intrusive
        // ALSO: move them up so they don't get in the way of the indicators?

        var tempAlpha = 0f;
        var tempColor = MainManager.Instance.MetroColors[MainManager.Instance.SelectedLineIndex];

        tempAlpha = BeatIndicator.MetronomeImage.color.a;
        BeatIndicator.MetronomeImage.color = new Color(tempColor.r, tempColor.g, tempColor.b, tempAlpha);

        tempAlpha = BeatIndicator.MetronomeImage.color.a;
        BeatIndicator.BarImage.color = new Color(tempColor.r, tempColor.g, tempColor.b, tempAlpha);

        PlayControls.ApplyControls();
    }
    
    public void TransitionToLeaderBoard()
    {
        Title.gameObject.SetActive(false);
        PlaySubPanel.SetActive(false);
        LeaderboardPanel.SetActive(true);
        Header.text = LeaderBoardHeader;
        LeaderBoardControls.ApplyControls();
        DisplayLeaderBoard();
    }

    public void UpdateHealth(int current, int max, bool negative)
    {
        var anchoredPosition = new Vector2(HealthGauge.anchoredPosition.x, -HealthGauge.sizeDelta.y + ((float)current / (float)max) * HealthGauge.sizeDelta.y);
        HealthGauge.anchoredPosition = anchoredPosition;

        var healthImage = HealthGauge.GetComponent<Image>();
        healthImage.DOColor(Color.Lerp(FailureColor, SuccessColor, (float)current / (float)max), MainManager.Instance.PulseTime);

        if (MainManager.Instance.CurrentState == MainManager.GameState.Play && MainManager.Instance.Player.MovedOnce)
        {
            HealthOverlay.color = negative 
                ? new Color(FailureColor.r, FailureColor.g, FailureColor.b, 0.5f) 
                : new Color(SuccessColor.r, SuccessColor.g, SuccessColor.b, 0.5f);
            HealthOverlay.DOFade(0f, 0.5f);
        }
    }

    public void UpdateScore(int score, bool instant)
    {
        StopAllCoroutines();

        if (MainManager.Instance.CurrentState == MainManager.GameState.Play && MainManager.Instance.Player.MovedOnce)
        {
            ScoreOverlay.color = new Color(SuccessColor.r, SuccessColor.g, SuccessColor.b, 0.5f);
            ScoreOverlay.DOFade(0f, 0.5f);
        }

        if (instant)
        {
            ScoreText.text = score.ToString();
            _displayedScore = score;

            return;
        }

        StartCoroutine(DoUpdateScore(score));
    }

    public void DisplayWarning(string msg)
    {
        WarningText.text = msg;
        WarningPanel.SetActive(true);
    }

    public void UpdateMovementMultiplier(int mutliplier, bool instant, bool negative)
    {
        MovementMultiplierText.transform.localScale = Vector3.one;

        if (MainManager.Instance.CurrentState == MainManager.GameState.Play && MainManager.Instance.Player.MovedOnce)
        {
            MultiplierOverlay.color = negative
                ? new Color(FailureColor.r, FailureColor.g, FailureColor.b, 0.5f)
                : new Color(SuccessColor.r, SuccessColor.g, SuccessColor.b, 0.5f);
            MultiplierOverlay.DOFade(0f, 0.5f);
        }

        MovementMultiplierText.text = "x" + mutliplier;

        if (!instant)
        {
            MovementMultiplierText.transform.DOPunchScale(Vector3.one * 0.05f * mutliplier, MovementMultiplierUpdateTime, 10, 0f);
        }
    }
    
    private void DisplayLeaderBoard()
    {
        var currentPlayerLeaderboardIndex = MainManager.Instance.CurrentPlayerLeaderboardIndex;
        var uiEntryIndex = 0;

        for (var i = 0; i < TopDisplayedEntries && i < MainManager.Instance.LeaderBoard.Count; i++)
        {
            Mathf.Clamp(i, 0, MainManager.Instance.LeaderBoard.Count - 1);

            var leaderBoardEntry = MainManager.Instance.LeaderBoard[i];

            LeaderboardEntries[uiEntryIndex].Rank.text = (i + 1).ToString();
            LeaderboardEntries[uiEntryIndex].Name.text = leaderBoardEntry.DisplayName;

            var format = new NumberFormatInfo { NumberGroupSeparator = " " };
            var scoreString = leaderBoardEntry.Score.ToString("n0", format);

            LeaderboardEntries[uiEntryIndex].Score.text = scoreString;

            LeaderboardEntries[uiEntryIndex].Background.color = i == currentPlayerLeaderboardIndex ? HighlightedEntryColor : NormalEntryColor;
            LeaderboardEntries[uiEntryIndex].Stripe.color = leaderBoardEntry.Color;
            LeaderboardEntries[uiEntryIndex].gameObject.SetActive(true);

            uiEntryIndex++;
        }
        
        var bot = currentPlayerLeaderboardIndex - DisplayedEntriesBeforePlayer;

        if (bot <= TopDisplayedEntries)
        {
            bot = TopDisplayedEntries;
            MiddleDotDotDot.gameObject.SetActive(false);
        }
        else
        {
            MiddleDotDotDot.gameObject.SetActive(true);
            MiddleDotDotDot.SetSiblingIndex(uiEntryIndex);
        }

        var top = bot + DisplayedEntriesBeforePlayer + DisplayedEntriesAfterPlayer + 1;

        for (var i = bot; i < top && i < MainManager.Instance.LeaderBoard.Count; i++)
        {
            Mathf.Clamp(i, 0, MainManager.Instance.LeaderBoard.Count - 1);

            var leaderBoardEntry = MainManager.Instance.LeaderBoard[i];

            LeaderboardEntries[uiEntryIndex].Rank.text = (i + 1).ToString();
            LeaderboardEntries[uiEntryIndex].Name.text = leaderBoardEntry.DisplayName;

            var format = new NumberFormatInfo { NumberGroupSeparator = " " };
            var scoreString = leaderBoardEntry.Score.ToString("n0", format);

            LeaderboardEntries[uiEntryIndex].Score.text = scoreString;

            LeaderboardEntries[uiEntryIndex].Background.color = i == currentPlayerLeaderboardIndex ? HighlightedEntryColor : NormalEntryColor;
            LeaderboardEntries[uiEntryIndex].Stripe.color = leaderBoardEntry.Color;
            LeaderboardEntries[uiEntryIndex].gameObject.SetActive(true);

            uiEntryIndex++;
        }

        EndDotDotDot.SetActive(top < MainManager.Instance.LeaderBoard.Count);
    }

    private IEnumerator DoUpdateScore(int score)
    {
        var currentScore = _displayedScore;
        var timeStep = 0f;

        while (timeStep < 1)
        {
            timeStep += Time.deltaTime / ScoreUpdateTime;
            _displayedScore = Mathf.Lerp(currentScore, score, timeStep);

            var format = new NumberFormatInfo { NumberGroupSeparator = " " };
            var scoreString = Mathf.RoundToInt(_displayedScore).ToString("n0", format);

            ScoreText.text = scoreString;

            yield return null;
        }
    }

    public void UI_Input(string input)
    {
        switch (input)
        {
            case "down":
                MainManager.Instance.InputDown();
                break;

            case "right":
                MainManager.Instance.InputRight();
                break;

            case "up":
                MainManager.Instance.InputUp();
                break;

            case "left":
                MainManager.Instance.InputLeft();
                break;
        }

        
    }
}
