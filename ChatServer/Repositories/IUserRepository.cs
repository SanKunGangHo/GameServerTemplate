using ChatServer.Models;

namespace ChatServer.Repositories;

public interface IUserRepository
{
    Task<User> CreateAsync(string username, string passwordHash);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByIdAsync(long id);
}