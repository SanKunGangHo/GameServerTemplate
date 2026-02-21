using Npgsql;

namespace ChatServer.Database;

public class DatabaseInitializer
{
    private readonly string _connectionString;
    
    public DatabaseInitializer(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public NpgsqlConnection GetConnection() => new(_connectionString);
}