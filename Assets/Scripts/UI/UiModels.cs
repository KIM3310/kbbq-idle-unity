public struct UpgradeUiEntry
{
    public string id;
    public string displayName;
    public string badgeText;
    public string impactText;
    public int level;
    public double cost;
    public double score;
    public bool affordable;
    public string category;
    public bool isBest;
}

public struct MeatInventoryUiEntry
{
    public string menuId;
    public string displayName;
    public int rawCount;
    public int cookedCount;
    public double buyCost;
    public bool isFeatured;
}

public struct GrillSlotUiState
{
    public int slotIndex;
    public bool occupied;
    public string menuId;
    public string displayName;
    public float cookProgress01;
    public float secondsToReady;
    public bool canFlip;
    public bool perfectWindow;
    public bool flipped;
    public bool readyToCollect;
    public bool burned;
}

public struct GameplayReviewPack
{
    public string contract;
    public string headline;
    public string storeTier;
    public int playerLevel;
    public double incomePerSecond;
    public double totalEarned;
    public int queueCount;
    public float servedPerMinute;
    public float averageWaitSeconds;
    public string monetizationMode;
    public string reviewStep;
    public string focusedRoute;
    public string reviewerSnapshot;
    public string focusedOpsSnapshot;
    public string twoMinuteReview;
    public string reviewRoutes;
    public string proofAssets;
}

public struct SessionGoalUiState
{
    public string headline;
    public string detail;
    public string accentLabel;
    public float urgency01;
}

public struct RestaurantShowcaseUiState
{
    public string title;
    public string primary;
    public string secondary;
    public string footer;
    public float heat01;
}

public struct HypeUiState
{
    public string headline;
    public string detail;
    public float fill01;
    public float alert01;
}

public struct LiveEventBannerUiState
{
    public string title;
    public string detail;
    public float accent01;
    public float urgency01;
    public bool visible;
}

public struct StoryQuestUiState
{
    public string actTitle;
    public string chapterTitle;
    public string speakerName;
    public string narrative;
    public string objectiveLine;
    public string rewardLine;
    public string statusLine;
    public float accent01;
    public bool visible;
}

public struct StoryLogUiState
{
    public string headline;
    public string speaker;
    public string line;
    public float accent01;
    public bool visible;
}

public struct DistrictSideQuestUiState
{
    public string districtTitle;
    public string speakerName;
    public string chapterTitle;
    public string objectiveLine;
    public string rewardLine;
    public string statusLine;
    public float accent01;
    public bool visible;
}
