using API.Features.UserManagerFeature.DomainLayer.Entities;
using API.Features.UserManagerFeature.DomainLayer.Enums;
using CodingPatterns.DomainLayer;

namespace API.Features.UserManagerFeature.DomainLayer.Repositories;

public interface IUserRepository : IRepository<User>
{   
    // Read Operations
    Task<User?> GetUserByIdAsync(string id);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User> GetUserByEmailAsync(string email);

    // Create Operation
    Task<bool> CreateUserIfNotRegisteredAsync(User newUser);

    // Update Operations
    Task UpdateUserAsync(User user);
    Task UpdateUserEmailAsync(string userId, string newEmail);
    Task UpdateUserPasswordAsync(string userId, string newPassword);
    Task<bool> UpdateUserStatusByEmailAsync(string email, UserStatus newStatus);

    // Delete Operation
    Task<bool> DeleteUserByEmailAsync(string email);
}