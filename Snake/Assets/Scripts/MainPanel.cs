using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainPanel : MonoBehaviour
{
    public static MainPanel Instance;

    public Text Header; 
    public string MainMenuHeader;
    public string BuildYourSnakeHeader;
    public string LeaderBoardHeader;

    private void Awake()
    {
        Instance = this;
    }

    public void TransitionToMainMenu()
    {
        Header.text = MainMenuHeader;
    }

    public void TransitionToBuildYourSnake()
    {
        Header.text = BuildYourSnakeHeader;
    }

    public void TransitionToPlay()
    {
        Header.text = string.Empty;
    }

    public void TransitionToLeaderBoard()
    {
        Header.text = LeaderBoardHeader;
    }
}
