using System.Collections.Generic;
using Xunit;

namespace KbbqIdle.Sim.Tests;

public class StoryQuestSystemTests
{
    [Fact]
    public void StoryQuestSystem_StartsWithFirstAlleyChapter()
    {
        var save = new global::SaveData();
        var system = new global::StoryQuestSystem(save);

        system.SyncMetaState(tierIndex: 0, playerLevel: 1, canPrestige: false, prestigeLevel: 0, rewardSeed: 10, emitUpdates: false);
        var ui = system.GetUiState();

        Assert.True(ui.visible);
        Assert.Equal("First Regulars", ui.chapterTitle);
        Assert.Contains("ACT I", ui.actTitle);
    }

    [Fact]
    public void StoryQuestSystem_ProgressesThroughEarlyAlleyAndUnlocksHongdae()
    {
        var save = new global::SaveData();
        var system = new global::StoryQuestSystem(save);

        system.SyncMetaState(tierIndex: 0, playerLevel: 1, canPrestige: false, prestigeLevel: 0, rewardSeed: 10, emitUpdates: true);
        for (var i = 0; i < 6; i++)
        {
            system.RecordServe(servings: 1, perfectServe: false, dailySpecialServed: false, spotlightServed: false);
        }

        Assert.Equal("Keep The Coals Alive", system.GetUiState().chapterTitle);

        system.RecordBoost();
        system.RecordBoost();
        Assert.Equal("House Special Rumor", system.GetUiState().chapterTitle);

        for (var i = 0; i < 4; i++)
        {
            system.RecordServe(servings: 1, perfectServe: false, dailySpecialServed: true, spotlightServed: false);
        }

        system.SyncMetaState(tierIndex: 1, playerLevel: 3, canPrestige: false, prestigeLevel: 0, rewardSeed: 10, emitUpdates: true);
        Assert.Equal("Open Mic Crowd", system.GetUiState().chapterTitle);

        var updates = Drain(system);
        Assert.Contains(updates, update => update.title == "Neon Arrival".ToUpperInvariant() || update.detail.Contains("pop-up", System.StringComparison.OrdinalIgnoreCase));
        Assert.Contains(save.storyLog, entry => entry.speaker == "DJ Yuna");
    }

    [Fact]
    public void StoryQuestSystem_GlobalFinaleRollsIntoPrestigeChapter()
    {
        var save = new global::SaveData();
        var system = new global::StoryQuestSystem(save);

        system.SyncMetaState(tierIndex: 4, playerLevel: 10, canPrestige: false, prestigeLevel: 0, rewardSeed: 10, emitUpdates: false);
        foreach (var quest in save.storyQuests)
        {
            if (quest.id == "global_finale" || quest.id == "global_relaunch")
            {
                continue;
            }

            quest.unlocked = true;
            quest.completed = true;
            quest.progress = quest.target;
        }

        system.SyncMetaState(tierIndex: 4, playerLevel: 10, canPrestige: true, prestigeLevel: 0, rewardSeed: 10, emitUpdates: true);
        Assert.Equal("Restart Stronger", system.GetUiState().chapterTitle);

        system.RecordPrestige();
        var finalUi = system.GetUiState();
        Assert.Equal("STORY COMPLETE", finalUi.actTitle);
        Assert.Contains("All District Arcs Resolved", finalUi.chapterTitle);
    }

    [Fact]
    public void StoryQuestSystem_ReturnsRecentStoryLogUiState()
    {
        var save = new global::SaveData();
        var system = new global::StoryQuestSystem(save);

        system.SyncMetaState(tierIndex: 0, playerLevel: 1, canPrestige: false, prestigeLevel: 0, rewardSeed: 10, emitUpdates: true);
        for (var i = 0; i < 6; i++)
        {
            system.RecordServe(servings: 1, perfectServe: false, dailySpecialServed: false, spotlightServed: false);
        }

        var log = system.GetStoryLogUiState();
        Assert.True(log.visible);
        Assert.False(string.IsNullOrWhiteSpace(log.speaker));
        Assert.False(string.IsNullOrWhiteSpace(log.line));
    }

    private static List<global::StoryQuestUpdate> Drain(global::StoryQuestSystem system)
    {
        var list = new List<global::StoryQuestUpdate>();
        while (system.TryDequeueUpdate(out var update))
        {
            list.Add(update);
        }

        return list;
    }
}
