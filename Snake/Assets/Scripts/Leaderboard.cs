using UnityEngine;
using System.Collections.Generic;

public static class Leaderboard
{
    public const int EntryCount = 10;

    public struct ScoreEntry
    {
        public string Name;
        public int Score;

        public ScoreEntry(string name, int score)
        {
            this.Name = name;
            this.Score = score;
        }
    }

    private static List<ScoreEntry> _scoreEntries;

    private static List<ScoreEntry> Entries
    {
        get
        {
            if (_scoreEntries == null)
            {
                _scoreEntries = new List<ScoreEntry>();
                LoadScores();
            }
            return _scoreEntries;
        }
    }

    private const string PlayerPrefsBaseKey = "leaderboard";

    private static void SortScores()
    {
        _scoreEntries.Sort((a, b) => b.Score.CompareTo(a.Score));
    }

    private static void LoadScores()
    {
        _scoreEntries.Clear();

        for (int i = 0; i < EntryCount; ++i)
        {
            ScoreEntry entry;
            entry.Name = PlayerPrefs.GetString(PlayerPrefsBaseKey + "[" + i + "].name", "");
            entry.Score = PlayerPrefs.GetInt(PlayerPrefsBaseKey + "[" + i + "].score", 0);
            _scoreEntries.Add(entry);
        }

        SortScores();
    }

    private static void SaveScores()
    {
        for (int i = 0; i < EntryCount; ++i)
        {
            var entry = _scoreEntries[i];
            PlayerPrefs.SetString(PlayerPrefsBaseKey + "[" + i + "].name", entry.Name);
            PlayerPrefs.SetInt(PlayerPrefsBaseKey + "[" + i + "].score", entry.Score);
        }
    }

    public static ScoreEntry GetEntry(int index)
    {
        return Entries[index];
    }

    public static void Record(string name, int score)
    {
        Entries.Add(new ScoreEntry(name, score));
        SortScores();
        Entries.RemoveAt(Entries.Count - 1);
        SaveScores();
    }

    public static void Clear()
    {
        for (int i = 0; i < EntryCount; ++i)
            _scoreEntries[i] = new ScoreEntry("", 0);
        SaveScores();
    }
}