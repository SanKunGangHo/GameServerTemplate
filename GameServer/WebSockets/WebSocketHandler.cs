using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using GameServer.Models;
using GameServer.Services;

namespace GameServer.WebSockets;

public class WebSocketHandler
{
    private readonly ChatService _chatService;
    
    // 채널 ID -> (유저 ID, WebSocket) 리스트
    //key: 채널 ID, value: (유저 ID, WebSocket) 리스트
    private ConcurrentDictionary<long, List<(long UserId, WebSocket Socket)>> _channels = new();
    
    public WebSocketHandler(ChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task HandleAsync(WebSocket socket, long userId, long channelId)
    {
        AddToChannel(channelId, userId, socket);

        try
        {
            await ReceiveLoop(socket, userId, channelId);
        }
        finally
        {
            RemoveFromChannel(channelId, socket);
        }
    }

    private async Task ReceiveLoop(WebSocket socket, long userId, long channelId)
    {
        var buffer = new byte[1024 * 4];
        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(buffer, CancellationToken.None);
            
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "연결 종료", CancellationToken.None);
                break;
            }
            
            var content = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var message = await _chatService.SendMessageAsync(channelId, userId, content);
            
            await BroadcastAsync(channelId, message);
        }
    }
    
    private async Task BroadcastAsync(long channelId, Message message)
    {
        if(!_channels.TryGetValue(channelId, out var sockets)) return;
        
        var json = JsonSerializer.Serialize(message);
        var bytes = Encoding.UTF8.GetBytes(json);
        
        foreach (var (_, socket) in sockets)
        {
            if (socket.State != WebSocketState.Open) continue;
            await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    private void AddToChannel(long channelId, long userId, WebSocket socket)
    {
        _channels.GetOrAdd(channelId, _ => new List<(long, WebSocket)>()).Add((userId, socket));
    }
    
    private void RemoveFromChannel(long channelId, WebSocket socket)
    {
        if(!_channels.TryGetValue(channelId, out var sockets)) return;
        sockets?.RemoveAll(x => x.Socket == socket);
    }
}