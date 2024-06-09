using System;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System.IO;

public class DatabaseManager : MonoBehaviour
{
    private string _dbPath;

    void Start()
    {
        _dbPath = Path.Combine(Application.persistentDataPath, "unity.db");
        
        if (!File.Exists(_dbPath))
        {
            CreateDatabase();
        }
    }

    void CreateDatabase()
    {
        string connection = "URI=file:" + _dbPath;
        using (var conn = new SqliteConnection(connection))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS users (" +
                                  "id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                                  "username TEXT NOT NULL, " +
                                  "password TEXT NOT NULL, " +
                                  "matches_played INTEGER DEFAULT 0, " +
                                  "wins INTEGER DEFAULT 0, " +
                                  "losses INTEGER DEFAULT 0, " +
                                  "total_score INTEGER DEFAULT 0)";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "INSERT INTO users (username, password) VALUES ('johndoe', 'johndoe'), " +
                                  "('johnsmith', 'johnsmith')";
                cmd.ExecuteNonQuery();
            }
            conn.Close();
        }
    }

    public bool Login(string username, string password)
    {
        string connection = "URI=file:" + _dbPath;
        using (var conn = new SqliteConnection(connection))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT id, username FROM users WHERE username=@username AND password=@password";
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Debug.Log("Login successful: " + reader["username"]);
                        return true;
                    }
                    else
                    {
                        Debug.Log("Login failed");
                        return false;
                    }
                }
            }
        }
    }
    
    public bool Register(string username, string password)
    {
        string connection = "URI=file:" + _dbPath;
        using (var conn = new SqliteConnection(connection))
        {
            conn.Open();
            using (var checkCmd = conn.CreateCommand())
            {
                // Check if the username already exists
                checkCmd.CommandText = "SELECT COUNT(*) FROM users WHERE username=@username";
                checkCmd.Parameters.AddWithValue("@username", username);
                long userExists = (long)checkCmd.ExecuteScalar();

                if (userExists > 0)
                {
                    Debug.Log("Username already exists");
                    return false; 
                }
            }

            using (var cmd = conn.CreateCommand())
            {
                // Insert new user
                cmd.CommandText = "INSERT INTO users (username, password) VALUES (@username, @password)";
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password); 
                cmd.ExecuteNonQuery();
                Debug.Log("User registered successfully");
                return true;
            }
        }
    }

    public void UpdateUserProfile(string username, int matchesPlayedIncrement, int winsIncrement, int lossesIncrement, 
        int totalScoreIncrement)
    {
        string connection = "URI=file:" + _dbPath;
        using (var conn = new SqliteConnection(connection))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                UPDATE users 
                SET 
                    matches_played = matches_played + @matchesPlayedIncrement,
                    wins = wins + @winsIncrement,
                    losses = losses + @lossesIncrement,
                    total_score = total_score + @totalScoreIncrement
                WHERE 
                    username = @username";

                cmd.Parameters.AddWithValue("@matchesPlayedIncrement", matchesPlayedIncrement);
                cmd.Parameters.AddWithValue("@winsIncrement", winsIncrement);
                cmd.Parameters.AddWithValue("@lossesIncrement", lossesIncrement);
                cmd.Parameters.AddWithValue("@totalScoreIncrement", totalScoreIncrement);
                cmd.Parameters.AddWithValue("@username", username);

                cmd.ExecuteNonQuery();
            }
        }
    }


    public UserProfile? GetUserProfile(string username)
    {
        string connection = "URI=file:" + _dbPath;
        using (var conn = new SqliteConnection(connection))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT username, matches_played, wins, losses, total_score FROM " +
                                  "users WHERE username=@username";
                cmd.Parameters.AddWithValue("@username", username);

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        UserProfile profile = new UserProfile
                        {
                            Username = reader["username"].ToString(),
                            MatchesPlayed = int.Parse(reader["matches_played"].ToString()),
                            Wins = int.Parse(reader["wins"].ToString()),
                            Losses = int.Parse(reader["losses"].ToString()),
                            TotalScore = int.Parse(reader["total_score"].ToString())
                        };
                        return profile;
                    }

                    Debug.Log("User not found");
                    return null;
                }
            }
        }
    }
}

[Serializable]
public struct UserProfile
{
    public string Username { get; set; }
    public int MatchesPlayed { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int TotalScore { get; set; }
}
