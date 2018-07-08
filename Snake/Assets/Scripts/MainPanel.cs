using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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

    [Header("MainMenu")]

    public string MainMenuHeader;
    public ControlsText MainMenuControls;

    [Header("BuildYourSnake")]

    public string BuildYourSnakeHeader;
    public ControlsText BuildYourSnakeControls;

    [Header("Play")]

    public GameObject PlaySubPanel;
    public BeatIndicator BeatIndicator;
    public Text ScoreText;
    public Text MovementMultiplierText;
    public float ScoreUpdateTime;
    public float MovementMultiplierUpdateTime;
    public GameObject LeaderboardPanel;
    public GameObject EntryPrefab;
    public ControlsText PlayControls;

    [Header("LeaderBoard")]

    public string LeaderBoardHeader;
    public ControlsText LeaderBoardControls;
    
    private float _displayedScore;

    private void Awake()
    {
        Instance = this;
    }

    public void TransitionToMainMenu()
    {
        PlaySubPanel.SetActive(false);
        LeaderboardPanel.SetActive(false);
        Header.text = MainMenuHeader;
        MainMenuControls.ApplyControls();
    }

    public void TransitionToBuildYourSnake()
    {
        PlaySubPanel.SetActive(false);
        LeaderboardPanel.SetActive(false);
        Header.text = BuildYourSnakeHeader;
        BuildYourSnakeControls.ApplyControls();
    }

    public void TransitionToPlay()
    {
        PlaySubPanel.SetActive(true);
        LeaderboardPanel.SetActive(false);
        Header.text = string.Empty;

        // IDEA: could fade control a bit during play mode to make it less intrusive
        // ALSO: move them up so they don't get in the way of the indicators?

        PlayControls.ApplyControls();
    }

    public void TransitionToLeaderBoard()
    {
        GridPlayground.Instance.ResetZones();
        PlaySubPanel.SetActive(false);
        LeaderboardPanel.SetActive(true);
        UpdateHighscores();
        Header.text = LeaderBoardHeader;
        LeaderBoardControls.ApplyControls();
    }

    private void UpdateHighscores()
    {
        var content = LeaderboardPanel.GetComponent<ScrollRect>().content;

        for (int i = 0; i < content.childCount; i++)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        for (int i = 0; i < Leaderboard.EntryCount; ++i)
        {
            var entry = Leaderboard.GetEntry(i);

            var entryText = Instantiate(EntryPrefab, LeaderboardPanel.GetComponent<ScrollRect>().content).GetComponent<Text>();
            entryText.text = entry.Name + ": " + entry.Score;
        }
    }

    public void UpdateScore(int score, bool instant)
    {
        StopAllCoroutines();

        if (instant)
        {
            ScoreText.text = score.ToString();
            _displayedScore = score;

            return;
        }

        StartCoroutine(DoUpdateScore(score));
    }

    public void UpdateMovementMultiplier(int mutliplier, bool instant)
    {
        StopAllCoroutines();

        MovementMultiplierText.text = "x" + mutliplier;

        if (!instant)
        {
            MovementMultiplierText.transform.DOPunchScale(Vector3.one * 0.1f * mutliplier, MovementMultiplierUpdateTime, 10, 0f);
        }
    }

    public void ControlToggle(KeyCode keyCode, bool toggle)
    {
        switch (keyCode)
        {
            case KeyCode.UpArrow:

                if (UpControlText.text == string.Empty)
                {
                    break;
                }

                UpControlImage.DOFade(toggle ? ControlsFadeValue : 1f, Instance.ControlsFadeTime);

                break;

            case KeyCode.RightArrow:

                if (RightControlText.text == string.Empty)
                {
                    break;
                }

                RightControlImage.DOFade(toggle ? ControlsFadeValue : 1f, Instance.ControlsFadeTime);

                break;

            case KeyCode.DownArrow:

                if (DownControlText.text == string.Empty)
                {
                    break;
                }

                DownControlImage.DOFade(toggle ? ControlsFadeValue : 1f, Instance.ControlsFadeTime);

                break;

            case KeyCode.LeftArrow:

                if (LeftControlText.text == string.Empty)
                {
                    break;
                }

                LeftControlImage.DOFade(toggle ? ControlsFadeValue : 1f, Instance.ControlsFadeTime);

                break;
        }
    }

    private IEnumerator DoUpdateScore(int score)
    {
        var currentScore = _displayedScore;
        var timeStep = 0f;

        while (timeStep < 1)
        {
            timeStep += Time.deltaTime / ScoreUpdateTime;
            _displayedScore = Mathf.Lerp(currentScore, score, timeStep);

            ScoreText.text = Mathf.RoundToInt(_displayedScore).ToString();

            yield return null;
        }
    }
}
