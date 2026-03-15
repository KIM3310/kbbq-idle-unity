using Xunit;

namespace KbbqIdle.Sim.Tests;

public class DistrictSideQuestSystemTests
{
    [Fact]
    public void DistrictSideQuestSystem_StartsWithFirstAlleyEpisode()
    {
        var save = new global::SaveData();
        var system = new global::DistrictSideQuestSystem(save);

        system.SyncMetaState(tierIndex: 0, canPrestige: false, prestigeLevel: 0, rewardSeed: 10, emitUpdates: false);
        var ui = system.GetUiState();

        Assert.True(ui.visible);
        Assert.Contains("ALLEY", ui.districtTitle);
        Assert.Equal("After-Shift Staff Meal", ui.chapterTitle);
    }

    [Fact]
    public void DistrictSideQuestSystem_UnlocksHigherDistrictEpisodes()
    {
        var save = new global::SaveData();
        var system = new global::DistrictSideQuestSystem(save);

        system.SyncMetaState(tierIndex: 0, canPrestige: false, prestigeLevel: 0, rewardSeed: 10, emitUpdates: false);
        system.RecordServe(servings: 3, perfectServe: false, dailySpecialServed: false, spotlightServed: false);
        system.RecordBoost();
        system.SyncMetaState(tierIndex: 1, canPrestige: false, prestigeLevel: 0, rewardSeed: 10, emitUpdates: true);

        var ui = system.GetUiState();
        Assert.Contains("HONGDAE", ui.districtTitle);
        Assert.False(string.IsNullOrWhiteSpace(ui.speakerName));
    }

    [Fact]
    public void DistrictSideQuestSystem_GlobalEpisodeCanResolveOnPrestigeReady()
    {
        var save = new global::SaveData();
        var system = new global::DistrictSideQuestSystem(save);

        system.SyncMetaState(tierIndex: 4, canPrestige: false, prestigeLevel: 0, rewardSeed: 10, emitUpdates: false);
        foreach (var quest in save.sideQuests)
        {
            if (quest.id == "global_last_push")
            {
                quest.unlocked = true;
                continue;
            }

            quest.unlocked = true;
            quest.completed = true;
            quest.progress = quest.target;
        }

        system.SyncMetaState(tierIndex: 4, canPrestige: true, prestigeLevel: 0, rewardSeed: 10, emitUpdates: true);
        var ui = system.GetUiState();

        Assert.False(ui.visible);
    }
}
