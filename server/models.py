from __future__ import annotations

from pydantic import BaseModel, Field


class AuthResponse(BaseModel):
    playerId: str
    token: str


class LeaderboardEntry(BaseModel):
    playerId: str
    displayName: str
    score: float
    rank: int


class LeaderboardResponse(BaseModel):
    entries: list[LeaderboardEntry] = Field(default_factory=list)


class FriendEntry(BaseModel):
    playerId: str
    displayName: str


class FriendListResponse(BaseModel):
    friends: list[FriendEntry] = Field(default_factory=list)


class ScoreSubmitRequest(BaseModel):
    playerId: str
    score: float
    signature: str
    timestamp: int
    nonce: str


class FriendInviteRequest(BaseModel):
    playerId: str
    code: str
    signature: str
    timestamp: int
    nonce: str


class AnalyticsEventRequest(BaseModel):
    playerId: str
    eventName: str
    kv: list[str] = Field(default_factory=list)
    timestamp: int
    nonce: str

