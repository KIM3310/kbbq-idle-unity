using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardView : MonoBehaviour
{
    [SerializeField] private Text listText;
    [SerializeField] private Text playerText;
    [SerializeField] private Text friendsText;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button closeButton;

    private GameManager gameManager;
    private readonly List<LeaderboardEntry> cached = new List<LeaderboardEntry>();

    public void Bind(GameManager manager)
    {
        gameManager = manager;
        if (refreshButton != null)
        {
            refreshButton.onClick.RemoveAllListeners();
            refreshButton.onClick.AddListener(Refresh);
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }
    }

    public void Open()
    {
        gameObject.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void Refresh()
    {
        cached.Clear();
        cached.AddRange(BuildMockEntries());
        Render();
        gameManager?.GetAudioManager()?.PlayButton();
    }

    private void Render()
    {
        if (listText != null)
        {
            var lines = new List<string>();
            foreach (var entry in cached)
            {
                lines.Add(entry.rank + ". " + entry.displayName + " " + FormatUtil.FormatCurrency(entry.score));
            }
            listText.text = string.Join("\n", lines);
        }

        if (playerText != null)
        {
            var player = cached.Find(x => x.playerId == "player");
            if (player != null)
            {
                playerText.text = "내 순위 #" + player.rank + "  " + FormatUtil.FormatCurrency(player.score);
            }
            else
            {
                playerText.text = "";
            }
        }

        if (friendsText != null)
        {
            var friendsLines = new List<string>();
            var count = 0;
            foreach (var entry in cached)
            {
                if (entry.playerId == "player")
                {
                    continue;
                }
                friendsLines.Add(entry.displayName + " " + FormatUtil.FormatCurrency(entry.score));
                count++;
                if (count >= 3)
                {
                    break;
                }
            }
            friendsText.text = "친구\n" + string.Join("\n", friendsLines);
        }
    }

    private List<LeaderboardEntry> BuildMockEntries()
    {
        var entries = new List<LeaderboardEntry>();
        var playerScore = gameManager != null ? gameManager.GetTotalEarned() : 0;
        var playerRank = Random.Range(3, 7);
        var baseScore = Mathf.Max(1f, (float)playerScore);

        for (int rank = 1; rank <= 10; rank++)
        {
            if (rank == playerRank)
            {
                entries.Add(new LeaderboardEntry
                {
                    playerId = "player",
                    displayName = "You",
                    rank = rank,
                    score = baseScore
                });
                continue;
            }

            var score = baseScore * Random.Range(0.6f, 1.4f) + (10 - rank) * 20;
            entries.Add(new LeaderboardEntry
            {
                playerId = "ai_" + rank,
                displayName = "Guest" + rank,
                rank = rank,
                score = score
            });
        }

        return entries;
    }
}
