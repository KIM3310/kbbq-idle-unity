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
}
