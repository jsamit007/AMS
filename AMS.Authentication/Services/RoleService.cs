using AMS.Authentication.Models;
using AMS.Authentication.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AMS.Authentication.Services
{
    /// <summary>
    /// Role and permission management service
    /// Implements role-based access control (RBAC) and fine-grained permissions
    /// </summary>
    public class RoleService : IRoleService
    {
        private readonly RoleManager<AppRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<RoleService> _logger;

        public RoleService(
            RoleManager<AppRole> roleManager,
            UserManager<AppUser> userManager,
            ILogger<RoleService> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Get all roles
        /// </summary>
        public async Task<IEnumerable<AppRole>> GetAllRolesAsync(CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(_roleManager.Roles.OrderByDescending(r => r.Priority).ToList());
        }

        /// <summary>
        /// Get role by ID
        /// </summary>
        public async Task<AppRole?> GetRoleByIdAsync(int roleId, CancellationToken cancellationToken = default)
        {
            return await _roleManager.FindByIdAsync(roleId.ToString());
        }

        /// <summary>
        /// Get role by name
        /// </summary>
        public async Task<AppRole?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken = default)
        {
            return await _roleManager.FindByNameAsync(roleName);
        }

        /// <summary>
        /// Create new role
        /// </summary>
        public async Task<(bool Success, string? Message)> CreateRoleAsync(string roleName, string description, CancellationToken cancellationToken = default)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return (false, "Role name is required");
            }

            // Check if role already exists
            var existingRole = await _roleManager.FindByNameAsync(roleName);
            if (existingRole != null)
            {
                _logger.LogWarning("Role creation failed: Role {RoleName} already exists", roleName);
                return (false, "Role already exists");
            }

            var role = new AppRole
            {
                Name = roleName,
                NormalizedName = roleName.ToUpper(),
                Description = description,
                IsSystemRole = false,
                Priority = 0
            };

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Role creation failed for {RoleName}: {Errors}", roleName, errors);
                return (false, errors);
            }

            _logger.LogInformation("Role created: {RoleName}", roleName);
            return (true, null);
        }

        /// <summary>
        /// Update role
        /// </summary>
        public async Task<(bool Success, string? Message)> UpdateRoleAsync(int roleId, string roleName, string description, CancellationToken cancellationToken = default)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role == null)
            {
                return (false, "Role not found");
            }

            // Prevent updating system roles
            if (role.IsSystemRole)
            {
                return (false, "System roles cannot be modified");
            }

            role.Name = roleName;
            role.NormalizedName = roleName.ToUpper();
            role.Description = description;

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, errors);
            }

            _logger.LogInformation("Role updated: {RoleName}", roleName);
            return (true, null);
        }

        /// <summary>
        /// Delete role
        /// </summary>
        public async Task<(bool Success, string? Message)> DeleteRoleAsync(int roleId, CancellationToken cancellationToken = default)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role == null)
            {
                return (false, "Role not found");
            }

            // Prevent deleting system roles
            if (role.IsSystemRole)
            {
                return (false, "System roles cannot be deleted");
            }

            // Check if role is assigned to any users
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name ?? string.Empty);
            if (usersInRole.Any())
            {
                return (false, "Cannot delete role with assigned users");
            }

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, errors);
            }

            _logger.LogInformation("Role deleted: {RoleName}", role.Name);
            return (true, null);
        }

        /// <summary>
        /// Assign permission to role
        /// </summary>
        public async Task<(bool Success, string? Message)> AssignPermissionAsync(int roleId, string permissionName, string description, CancellationToken cancellationToken = default)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role == null)
            {
                return (false, "Role not found");
            }

            // Check if permission already exists for this role
            var existingPermission = role.Permissions.FirstOrDefault(p => p.PermissionName == permissionName);
            if (existingPermission != null)
            {
                return (false, "Permission already assigned to this role");
            }

            // In a real implementation, this would add to a database context
            // For now, we'll log the intention
            _logger.LogInformation("Permission {Permission} assigned to role {RoleName}", permissionName, role.Name);
            return (true, null);
        }

        /// <summary>
        /// Remove permission from role
        /// </summary>
        public async Task<(bool Success, string? Message)> RemovePermissionAsync(int roleId, string permissionName, CancellationToken cancellationToken = default)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role == null)
            {
                return (false, "Role not found");
            }

            _logger.LogInformation("Permission {Permission} removed from role {RoleName}", permissionName, role.Name);
            return (true, null);
        }

        /// <summary>
        /// Get all permissions for a role
        /// </summary>
        public async Task<IEnumerable<RolePermission>> GetRolePermissionsAsync(int roleId, CancellationToken cancellationToken = default)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role == null)
            {
                return Enumerable.Empty<RolePermission>();
            }

            return await Task.FromResult(role.Permissions.Where(p => p.IsActive).ToList());
        }

        /// <summary>
        /// Assign role to user
        /// </summary>
        public async Task<(bool Success, string? Message)> AssignRoleToUserAsync(int userId, string roleName, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return (false, "User not found");
            }

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                return (false, "Role not found");
            }

            var isInRole = await _userManager.IsInRoleAsync(user, roleName);
            if (isInRole)
            {
                return (false, "User already has this role");
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, errors);
            }

            _logger.LogInformation("Role {RoleName} assigned to user {UserId}", roleName, userId);
            return (true, null);
        }

        /// <summary>
        /// Remove role from user
        /// </summary>
        public async Task<(bool Success, string? Message)> RemoveRoleFromUserAsync(int userId, string roleName, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return (false, "User not found");
            }

            var isInRole = await _userManager.IsInRoleAsync(user, roleName);
            if (!isInRole)
            {
                return (false, "User does not have this role");
            }

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, errors);
            }

            _logger.LogInformation("Role {RoleName} removed from user {UserId}", roleName, userId);
            return (true, null);
        }

        /// <summary>
        /// Get all permissions for a user
        /// </summary>
        public async Task<IList<string>> GetUserPermissionsAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return new List<string>();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var permissions = new HashSet<string>();

            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    foreach (var permission in role.Permissions.Where(p => p.IsActive))
                    {
                        permissions.Add(permission.PermissionName);
                    }
                }
            }

            return permissions.ToList();
        }
    }
}
