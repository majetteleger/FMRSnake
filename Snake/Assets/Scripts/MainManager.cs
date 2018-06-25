using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    public static MainManager Instance;
    
    public GameObject PlayerPrefab;

    public Player Player { get; set; }
    public GridPlayground GridPlayground { get; set; }
    public bool GameStarted { get; set; }

    private bool[] loadedPlayerIndices = new bool[4];
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Player = Instantiate(PlayerPrefab).GetComponent<Player>();
        GridPlayground = FindObjectOfType<GridPlayground>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            AddPlayer(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            AddPlayer(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            AddPlayer(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            StartGame();
        }
    }

    private void AddPlayer(int playerIndex)
    {
        if (loadedPlayerIndices[playerIndex])
        {
            return;
        }

        Player.Grow();
        Player.Move(Vector3.right);
    }

    private void StartGame()
    {
        Player.StartGame();
        GameStarted = true;

        GridPlayground.ShowCells();
    }
}
