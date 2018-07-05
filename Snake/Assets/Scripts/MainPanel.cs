using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MainPanel : MonoBehaviour
{
    public static MainPanel Instance;

    [Header("General")]

    public Text Header;
    public string MainMenuHeader;
    public string BuildYourSnakeHeader;
    public string LeaderBoardHeader;

    [Header("Play")]

    public GameObject PlaySubPanel;
    public BeatIndicator BeatIndicator;
    public Text ScoreText;
    public Text MovementMultiplierText;
    public float ScoreUpdateTime;
    public float MovementMultiplierUpdateTime;
    public GameObject LeaderboardPanel;
    public GameObject EntryPrefab;

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
    }

    public void TransitionToBuildYourSnake()
    {
        PlaySubPanel.SetActive(false);
        LeaderboardPanel.SetActive(false);
        Header.text = BuildYourSnakeHeader;
    }

    public void TransitionToPlay()
    {
        PlaySubPanel.SetActive(true);
        LeaderboardPanel.SetActive(false);
        Header.text = string.Empty;
    }

    public void TransitionToLeaderBoard()
    {
        PlaySubPanel.SetActive(false);
        LeaderboardPanel.SetActive(true);
        UpdateHighscores();
        Header.text = LeaderBoardHeader;
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
