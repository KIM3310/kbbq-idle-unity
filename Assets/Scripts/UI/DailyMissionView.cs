using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyMissionView : MonoBehaviour
{
    [SerializeField] private Text[] missionTexts;
    [SerializeField] private Button[] claimButtons;
    [SerializeField] private bool compactHud = true;

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

        if (compactHud)
        {
            RenderCompact();
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

    private void RenderCompact()
    {
        var activeMission = GetPriorityMission();

        if (missionTexts.Length > 0 && missionTexts[0] != null)
        {
            if (activeMission == null)
            {
                missionTexts[0].text = "Mission Hub: all clear";
            }
            else
            {
                var status = activeMission.claimed ? "CLAIMED" : activeMission.completed ? "READY" : "IN PROGRESS";
                missionTexts[0].text = "Mission: " + activeMission.type + " " +
                                       activeMission.progress.ToString("0") + "/" + activeMission.target.ToString("0") +
                                       "  " + status;
            }
        }

        for (int i = 1; i < missionTexts.Length; i++)
        {
            if (missionTexts[i] != null)
            {
                missionTexts[i].text = "";
            }
        }

        if (claimButtons != null)
        {
            for (int i = 0; i < claimButtons.Length; i++)
            {
                if (claimButtons[i] != null)
                {
                    claimButtons[i].gameObject.SetActive(false);
                    claimButtons[i].interactable = false;
                }
            }
        }

        if (activeMission != null && activeMission.completed && !activeMission.claimed)
        {
            gameManager?.ClaimDailyMission(activeMission.id);
        }
    }

    private DailyMissionState GetPriorityMission()
    {
        for (int i = 0; i < cached.Count; i++)
        {
            var mission = cached[i];
            if (mission != null && !mission.claimed)
            {
                return mission;
            }
        }

        return cached.Count > 0 ? cached[0] : null;
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
