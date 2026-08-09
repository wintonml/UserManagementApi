using UserManagementAPI.Models;

namespace UserManagementAPI.Repositories;

public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new();

    public Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.AsEnumerable());
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.SingleOrDefault(u => u.Id == id));
    }

    public Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        user.Id = Guid.NewGuid();
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = user.CreatedAt;
        _users.Add(user);
        return Task.FromResult(user);
    }

    public Task<User?> UpdateAsync(Guid id, User user, CancellationToken cancellationToken = default)
    {
        var existing = _users.SingleOrDefault(u => u.Id == id);
        if (existing is null)
        {
            return Task.FromResult<User?>(null);
        }

        existing.FirstName = user.FirstName;
        existing.LastName = user.LastName;
        existing.Email = user.Email;
        existing.UpdatedAt = DateTime.UtcNow;
        return Task.FromResult(existing);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = _users.SingleOrDefault(u => u.Id == id);
        if (user is null)
        {
            return Task.FromResult(false);
        }

        _users.Remove(user);
        return Task.FromResult(true);
    }
}
