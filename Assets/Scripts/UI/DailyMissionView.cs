using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyMissionView : MonoBehaviour
{
    [SerializeField] private Text[] missionTexts;
    [SerializeField] private Button[] claimButtons;

    private GameManager gameManager;
    private readonly List<DailyMissionState> cached = new List<DailyMissionState>();

    public void Bind(GameManager manager)
    {
        gameManager = manager;
    }

    public void Render(IReadOnlyList<DailyMissionState> missions)
    {
        cached.Clear();
        if (missions != null)
        {
            cached.AddRange(missions);
        }

        if (missionTexts == null)
        {
            return;
        }

        for (int i = 0; i < missionTexts.Length; i++)
        {
            if (i >= cached.Count)
            {
                if (missionTexts[i] != null)
                {
                    missionTexts[i].text = "";
                }
                if (claimButtons != null && i < claimButtons.Length && claimButtons[i] != null)
                {
                    claimButtons[i].interactable = false;
                }
                continue;
            }

            var mission = cached[i];
            if (missionTexts[i] != null)
            {
                var status = mission.claimed ? "Claimed" : mission.completed ? "Complete" : "In Progress";
                missionTexts[i].text = mission.type + " " + mission.progress.ToString("0") + "/" + mission.target.ToString("0") + " " + status;
            }

            if (claimButtons != null && i < claimButtons.Length && claimButtons[i] != null)
            {
                claimButtons[i].interactable = mission.completed && !mission.claimed;
            }
        }
    }

    public void ClaimMission(int index)
    {
        if (gameManager == null || index < 0 || index >= cached.Count)
        {
            return;
        }

        var mission = cached[index];
        if (mission != null)
        {
            gameManager.ClaimDailyMission(mission.id);
        }
    }
}
