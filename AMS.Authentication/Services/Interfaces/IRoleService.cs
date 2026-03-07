using AMS.Authentication.Models;

namespace AMS.Authentication.Services.Interfaces
{
    /// <summary>
    /// Interface for role and permission management
    /// </summary>
    public interface IRoleService
    {
        /// <summary>
        /// Get all available roles
        /// </summary>
        Task<IEnumerable<AppRole>> GetAllRolesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get role by ID
        /// </summary>
        Task<AppRole?> GetRoleByIdAsync(int roleId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get role by name
        /// </summary>
        Task<AppRole?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Create new role
        /// </summary>
        Task<(bool Success, string? Message)> CreateRoleAsync(string roleName, string description, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update role
        /// </summary>
        Task<(bool Success, string? Message)> UpdateRoleAsync(int roleId, string roleName, string description, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete role
        /// </summary>
        Task<(bool Success, string? Message)> DeleteRoleAsync(int roleId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Assign permission to role
        /// </summary>
        Task<(bool Success, string? Message)> AssignPermissionAsync(int roleId, string permissionName, string description, CancellationToken cancellationToken = default);

        /// <summary>
        /// Remove permission from role
        /// </summary>
        Task<(bool Success, string? Message)> RemovePermissionAsync(int roleId, string permissionName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all permissions for a role
        /// </summary>
        Task<IEnumerable<RolePermission>> GetRolePermissionsAsync(int roleId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Assign role to user
        /// </summary>
        Task<(bool Success, string? Message)> AssignRoleToUserAsync(int userId, string roleName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Remove role from user
        /// </summary>
        Task<(bool Success, string? Message)> RemoveRoleFromUserAsync(int userId, string roleName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get user permissions
        /// </summary>
        Task<IList<string>> GetUserPermissionsAsync(int userId, CancellationToken cancellationToken = default);
    }
}
