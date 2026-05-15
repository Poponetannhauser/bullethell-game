using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BulletHell.Core
{
    [Serializable]
    public class ScoreEntry
    {
        public int score;
        public string date;

        public ScoreEntry(int score, string date)
        {
            this.score = score;
            this.date = date;
        }
    }

    [Serializable]
    public class LeaderboardData
    {
        public List<ScoreEntry> scores = new List<ScoreEntry>();
    }

    // Handles local Top 5 highscore persistence via PlayerPrefs (JSON)
    public static class LeaderboardSystem
    {
        private const string SAVE_KEY = "LocalLeaderboard";
        private const int MAX_ENTRIES = 5;

        public static void SaveScore(int newScore)
        {
            LeaderboardData data = LoadLeaderboard();

            string currentDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            data.scores.Add(new ScoreEntry(newScore, currentDate));

            data.scores = data.scores
                .OrderByDescending(s => s.score)
                .Take(MAX_ENTRIES)
                .ToList();

            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        public static LeaderboardData LoadLeaderboard()
        {
            if (!PlayerPrefs.HasKey(SAVE_KEY))
                return new LeaderboardData();

            string json = PlayerPrefs.GetString(SAVE_KEY);
            return JsonUtility.FromJson<LeaderboardData>(json);
        }
    }
}
