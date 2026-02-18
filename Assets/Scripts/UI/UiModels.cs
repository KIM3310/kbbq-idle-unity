public struct UpgradeUiEntry
{
    public string id;
    public string displayName;
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
    public bool flipped;
    public bool readyToCollect;
    public bool burned;
}
