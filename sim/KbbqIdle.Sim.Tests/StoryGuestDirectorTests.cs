using Xunit;

namespace KbbqIdle.Sim.Tests;

public class StoryGuestDirectorTests
{
    [Fact]
    public void StoryGuestDirector_QueuesDistrictGuestWhenTierReached()
    {
        var save = new global::SaveData();
        var director = new global::StoryGuestDirector(save);

        director.SyncMetaState(tierIndex: 1, canPrestige: false);

        Assert.True(director.TryDequeueEncounter(out var encounter));
        Assert.False(string.IsNullOrWhiteSpace(encounter.displayName));
        Assert.False(string.IsNullOrWhiteSpace(encounter.arrivalLine));
    }

    [Fact]
    public void StoryGuestDirector_DoesNotRepeatResolvedGuest()
    {
        var save = new global::SaveData();
        var director = new global::StoryGuestDirector(save);

        director.SyncMetaState(tierIndex: 0, canPrestige: false);
        Assert.True(director.TryDequeueEncounter(out var encounter));
        Assert.True(director.TryResolveEncounter(encounter.id, out _));

        director.SyncMetaState(tierIndex: 0, canPrestige: false);
        while (director.TryDequeueEncounter(out var next))
        {
            Assert.NotEqual(encounter.id, next.id);
        }
    }

    [Fact]
    public void StoryGuestDirector_FinaleGuestRequiresPrestigeReady()
    {
        var save = new global::SaveData();
        var director = new global::StoryGuestDirector(save);

        director.SyncMetaState(tierIndex: 4, canPrestige: false);
        while (director.TryDequeueEncounter(out var encounter))
        {
            Assert.NotEqual("global_amira_finale", encounter.id);
        }

        director.SyncMetaState(tierIndex: 4, canPrestige: true);
        var sawFinale = false;
        while (director.TryDequeueEncounter(out var encounter))
        {
            if (encounter.id == "global_amira_finale")
            {
                sawFinale = true;
            }
        }

        Assert.True(sawFinale);
    }

    [Fact]
    public void StoryGuestDirector_FinaleGuestIsMarkedAsBoss()
    {
        var save = new global::SaveData();
        var director = new global::StoryGuestDirector(save);

        director.SyncMetaState(tierIndex: 4, canPrestige: true);
        while (director.TryDequeueEncounter(out var encounter))
        {
            if (encounter.id == "global_amira_finale")
            {
                Assert.True(encounter.isBossGuest);
                Assert.True(encounter.isFinaleGuest);
                return;
            }
        }

        Assert.Fail("Expected finale boss guest.");
    }

    [Fact]
    public void StoryGuestDirector_BadgeBoardCountsEarnedBadges()
    {
        var save = new global::SaveData();
        var director = new global::StoryGuestDirector(save);

        director.SyncMetaState(tierIndex: 1, canPrestige: false);
        Assert.True(director.TryDequeueEncounter(out var first));
        Assert.True(director.TryResolveEncounter(first.id, out _));

        var board = director.GetBadgeBoardUiState();
        Assert.True(board.visible);
        Assert.Contains("1/", board.progressLine);
        Assert.Contains("Badge", board.badgeLine);
    }

    [Fact]
    public void StoryGuestDirector_RetryLineEscalates()
    {
        var save = new global::SaveData();
        var director = new global::StoryGuestDirector(save);

        director.SyncMetaState(tierIndex: 4, canPrestige: true);
        Assert.True(director.TryDequeueEncounter(out var encounter));
        while (encounter.id != "global_amira_finale" && director.TryDequeueEncounter(out encounter))
        {
        }

        var first = director.GetRetryLine(encounter, director.RecordRetry(encounter.id));
        var second = director.GetRetryLine(encounter, director.RecordRetry(encounter.id));
        var third = director.GetRetryLine(encounter, director.RecordRetry(encounter.id));

        Assert.NotEqual(first, second);
        Assert.NotEqual(second, third);
    }

    [Fact]
    public void StoryGuestDirector_ChampionUnlocksAfterAllOtherBadges()
    {
        var save = new global::SaveData();
        var director = new global::StoryGuestDirector(save);

        var allButChampion = new[]
        {
            "alley_jun",
            "hongdae_sori",
            "gangnam_park",
            "hanok_sunwoo",
            "global_niko",
            "global_amira_finale",
        };

        director.SyncMetaState(tierIndex: 4, canPrestige: true);
        foreach (var id in allButChampion)
        {
            Assert.True(director.TryResolveEncounter(id, out _));
        }

        director.SyncMetaState(tierIndex: 4, canPrestige: true);
        var foundChampion = false;
        while (director.TryDequeueEncounter(out var encounter))
        {
            if (encounter.id == "champion_mirae")
            {
                foundChampion = true;
                Assert.True(encounter.isBossGuest);
                Assert.Equal(3, encounter.bossPhases);
            }
        }

        var board = director.GetBadgeBoardUiState();
        Assert.Contains("Champion unlocked", board.detailLine);
        Assert.True(foundChampion);
    }
}
