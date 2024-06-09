using System;

[Serializable]
public struct UserProfile
{
    public string Username { get; set; }
    public int MatchesPlayed { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int TotalScore { get; set; }
}