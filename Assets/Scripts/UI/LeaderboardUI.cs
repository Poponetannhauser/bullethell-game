using UnityEngine;
using TMPro;
using BulletHell.Core;

namespace BulletHell.UI
{
    // Display top 5
    public class LeaderboardUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreListText;

        void OnEnable()
        {
            DisplayLeaderboard();
        }

        public void DisplayLeaderboard()
        {
            if (scoreListText == null) return;

            LeaderboardData data = LeaderboardSystem.LoadLeaderboard();

            if (data.scores.Count == 0)
            {
                scoreListText.text = "NO SCORES YET.\nGO PLAY!";
                return;
            }

            string displayText = "";
            for (int i = 0; i < data.scores.Count; i++)
            {
                var entry = data.scores[i];
                displayText += $"{i + 1}. {entry.score:N0} PTS <size=60%><color=#888888>({entry.date})</color></size>\n";
            }

            scoreListText.text = displayText;
        }
    }
}
