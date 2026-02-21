using Dapper;
using GameServer.Database;
using GameServer.Models;

namespace GameServer.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly DatabaseInitializer _db;

    public MessageRepository(DatabaseInitializer db)
    {
        _db = db;
    }

    public async Task<Message> SaveAsync(long channelId, long userId, string content)
    {
        const string sql = @"
            INSERT INTO messages (channel_id, user_id, content)
            VALUES (@ChannelId, @UserId, @Content)
            RETURNING id, channel_id, user_id, content, sent_at";

        using var conn = _db.GetConnection();
        return await conn.QuerySingleAsync<Message>(sql, new
        {
            ChannelId = channelId, 
            UserId = userId, 
            Content = content
        });
    }

    public async Task<IEnumerable<Message>> GetMessagesAsync(long channelId, int count = 50)
    {
        const string sql = @"
                SELECT m.id, m.channel_id, u.username, m.content, m.sent_at
                From messages m
                JOIN users u ON m.user_id = u.id
                WHERE m.channel_id = @ChannelId
                ORDER BY m.sent_at DESC
                LIMIT @Count";
        
        using var conn = _db.GetConnection();
        return await conn.QueryAsync<Message>(sql, new { ChannelId = channelId, Count = count });
    }
}