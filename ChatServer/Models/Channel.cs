namespace ChatServer.Models;

public class Channel
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
