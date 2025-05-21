using TMPro;
using UnityEngine;

[System.Serializable]
public class LeaderboardItem
{
    public int rank;
    public string name;
    public int score;
    public string sessionId;
    public bool isCurrent;
}

public class LeaderboardItemHandler : MonoBehaviour
{
    [SerializeField] GameObject highlight;
    [SerializeField] TextMeshProUGUI rankText;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI scoreText;

    public void UpdateInfo(LeaderboardItem info)
    {
        rankText.text = info.rank.ToString();
        nameText.text = info.name;
        scoreText.text = info.score.ToString();
        highlight.SetActive(info.isCurrent);
    }

    void ResetInfo()
    {
        highlight.SetActive(false);
        rankText.text = "";
        nameText.text = "";
        scoreText.text = "";
    }
}
