using System.Collections.Concurrent;
using UserManagementAPI.Models;

namespace UserManagementAPI.Repositories;

public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<Guid, User> _users = new();
    //private readonly List<User> _users = new();

    public Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.Values.ToArray().AsEnumerable());
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.TryGetValue(id, out var user) ? user : null);
    }

    public Task<User> CreateAsync(CreateUserRequest user, CancellationToken cancellationToken = default)
    {
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _users[newUser.Id] = newUser;
        return Task.FromResult(newUser);
    }

    public Task<User?> UpdateAsync(Guid id, UpdateUserRequest user, CancellationToken cancellationToken = default)
    {
        if (!_users.TryGetValue(id, out var existing))
            return Task.FromResult<User?>(null);

        existing.FirstName = user.FirstName;
        existing.LastName = user.LastName;
        existing.Email = user.Email;
        existing.UpdatedAt = DateTime.UtcNow;

        _users[id] = existing;
        return Task.FromResult(existing);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = _users.TryRemove(id, out _);
        return Task.FromResult(deleted);
    }
}
