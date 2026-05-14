using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using RestAll.Desktop.Core.Auth;

namespace RestAll.Desktop.Infrastructure.Auth;

public class SqliteSessionStorage : ISessionStorage
{
    private readonly string _databasePath;
    private readonly ILogger<SqliteSessionStorage> _logger;
    private const string SessionsTable = "user_sessions";

    public SqliteSessionStorage(ILogger<SqliteSessionStorage> logger)
    {
        _logger = logger;
        _databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RestAll", "offline.db");
        
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_databasePath)!);
        
        using var connection = new SqliteConnection($"Data Source={_databasePath}");
        connection.Open();
        
        var createSessionsTable = @"
            CREATE TABLE IF NOT EXISTS user_sessions (
                id INTEGER PRIMARY KEY,
                access_token TEXT NOT NULL,
                refresh_token TEXT NOT NULL,
                full_name TEXT NOT NULL,
                role TEXT NOT NULL,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP
            )";
        
        var command = connection.CreateCommand();
        command.CommandText = createSessionsTable;
        command.ExecuteNonQuery();
        
        _logger.LogInformation("SQLite sessions table initialized at {Path}", _databasePath);
    }

    public async Task<UserSession?> GetSessionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync(cancellationToken);
            
            var command = connection.CreateCommand();
            command.CommandText = "SELECT access_token, refresh_token, full_name, role FROM user_sessions ORDER BY created_at DESC LIMIT 1";
            
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                var accessToken = reader.GetString(0);
                var refreshToken = reader.GetString(1);
                var fullName = reader.GetString(2);
                var role = reader.GetString(3);
                
                var session = new UserSession(accessToken, refreshToken, fullName, role);
                _logger.LogDebug("Retrieved user session from storage");
                return session;
            }
            
            _logger.LogDebug("No user session found in storage");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session from storage");
            return null;
        }
    }

    public async Task SaveSessionAsync(UserSession session, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync(cancellationToken);
            
            // Clear existing sessions first
            var clearCommand = connection.CreateCommand();
            clearCommand.CommandText = "DELETE FROM user_sessions";
            await clearCommand.ExecuteNonQueryAsync(cancellationToken);
            
            // Insert new session
            var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = @"
                INSERT INTO user_sessions (access_token, refresh_token, full_name, role)
                VALUES (@access_token, @refresh_token, @full_name, @role)";
            
            insertCommand.Parameters.AddWithValue("@access_token", session.AccessToken);
            insertCommand.Parameters.AddWithValue("@refresh_token", session.RefreshToken);
            insertCommand.Parameters.AddWithValue("@full_name", session.FullName);
            insertCommand.Parameters.AddWithValue("@role", session.Role);
            
            await insertCommand.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogDebug("Saved user session to storage");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving session to storage");
        }
    }

    public async Task ClearSessionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync(cancellationToken);
            
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM user_sessions";
            
            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogDebug("Cleared user session from storage");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing session from storage");
        }
    }
}
