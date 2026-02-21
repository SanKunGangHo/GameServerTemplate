using GameServer.Models;
using GameServer.Repositories;

namespace GameServer.Services;

public class ChatService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;

    public ChatService(IMessageRepository messageRepository, IUserRepository userRepository)
    {
        _messageRepository = messageRepository;
        _userRepository = userRepository;
    }

    public async Task<Message> SendMessageAsync(long channelId, long userId, string content)
    {
        if(string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("메시지 내용이 비어있습니다.", nameof(content));

        if (content.Length > 500)
            throw new ArgumentException("메시지는 500자 이하여야 합니다.", nameof(content));

        var user = await _userRepository.GetByIdAsync(userId);
        var username = user?.Username ?? "unknown";

        return await _messageRepository.SaveAsync(channelId, userId, username, content);
    }
    
    public async Task<IEnumerable<Message>> GetMessagesAsync(long channelId, int count = 50)
    {
        return await _messageRepository.GetMessagesAsync(channelId, count);
    }
}