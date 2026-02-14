using System;
using System.Collections.Generic;

[Serializable]
public class AuthResponse
{
    public string playerId;
    public string token;
}

[Serializable]
public class LeaderboardEntry
{
    public string playerId;
    public string displayName;
    public double score;
    public int rank;
}

[Serializable]
public class LeaderboardResponse
{
    public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
}

[Serializable]
public class FriendEntry
{
    public string playerId;
    public string displayName;
}

[Serializable]
public class FriendListResponse
{
    public List<FriendEntry> friends = new List<FriendEntry>();
}

[Serializable]
public class ScoreSubmitRequest
{
    public string playerId;
    public double score;
    public string signature;
    public long timestamp;
    public string nonce;
}

[Serializable]
public class FriendInviteRequest
{
    public string playerId;
    public string code;
    public string signature;
    public long timestamp;
    public string nonce;
}

[Serializable]
public class AnalyticsEventRequest
{
    public string playerId;
    public string eventName;
    public string[] kv;
    public long timestamp;
    public string nonce;
}
