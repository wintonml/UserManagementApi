using UserManagementAPI.Models;

namespace UserManagementAPI.Repositories;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User> CreateAsync(CreateUserRequest user, CancellationToken cancellationToken = default);
    Task<User?> UpdateAsync(Guid id, UpdateUserRequest user, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
