using Dapper;
using GameServer.Database;
using GameServer.Models;

namespace GameServer.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DatabaseInitializer _db;
    
    public UserRepository(DatabaseInitializer db)
    {
        _db = db;
    }

    public async Task<User> CreateAsync(string username, string passwordHash)
    {
        const string sql = @"INSERT INTO users (username, password_hash)
                VALUES (@Username, @PasswordHash)
                RETURNING id, username, password_hash, created_at";

        using var conn = _db.GetConnection();
        return await conn.QuerySingleAsync<User>(sql, new { Username = username, PasswordHash = passwordHash });
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        const string sql = @"SELECT id, username, password_hash AS PasswordHash, created_at AS CreatedAt
                FROM users
                WHERE username = @Username";
        
        using var conn = _db.GetConnection();
        return await conn.QuerySingleOrDefaultAsync<User>(sql, new { Username = username });
    }
    
    public async Task<User?> GetByIdAsync(long id)
    {
        const string sql = @"SELECT id, username, password_hash AS PasswordHash, created_at AS CreatedAt
                FROM users
                WHERE id = @Id";
        
        using var conn = _db.GetConnection();
        return await conn.QuerySingleOrDefaultAsync<User>(sql, new { Id = id });
    }
}