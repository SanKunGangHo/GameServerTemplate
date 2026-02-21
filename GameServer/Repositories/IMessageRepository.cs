using GameServer.Models;
namespace GameServer.Repositories;

public interface IMessageRepository
{
    Task<Message> SaveAsync(long channelId, long userId, string content);
    Task<IEnumerable<Message>> GetMessagesAsync(long channelId, int count = 50);
}