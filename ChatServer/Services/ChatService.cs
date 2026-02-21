using ChatServer.Models;
using ChatServer.Repositories;

namespace ChatServer.Services;

public class ChatService
{
    private readonly IMessageRepository _messageRepository;
    
    public ChatService(IMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }

    public async Task<Message> SendMessageAsync(long channelId, long userId, string content)
    {
        if(string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("메시지 내용이 비어있습니다.", nameof(content));
        
        if (content.Length > 500)
            throw new ArgumentException("메시지는 500자 이하여야 합니다.", nameof(content));
        
        return await _messageRepository.SaveAsync(channelId, userId, content);
    }
    
    public async Task<IEnumerable<Message>> GetMessagesAsync(long channelId, int count = 50)
    {
        return await _messageRepository.GetMessagesAsync(channelId, count);
    }
}