using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Venti;
using Venti.Plugins.Leaderboard;

public class ScoreTest : MonoBehaviour
{
    public TMP_InputField inputField;
    public Button button;

    public LeaderboardHandler leaderboardHandler;

    private int score = 0;

    public Texture2D texture;

    public void SubmitScore()
    {
        string scoreString = inputField.text;
        if (string.IsNullOrEmpty(scoreString))
        {
            Debug.LogError("Score is empty");
            return;
        }

        int score = int.Parse(scoreString);

        // SessionManager.Instance.EndSession(score);
        SessionManager.Instance.EndSession(score, texture);
        this.score = score;
    }

    public void OnSessionEnd()
    {
        leaderboardHandler.LoadLeaderboard();
        leaderboardHandler.ShowLeaderboard();
    }
}
