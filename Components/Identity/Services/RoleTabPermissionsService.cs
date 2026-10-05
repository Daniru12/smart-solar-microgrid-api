using SmartSolarMicrogrid.API.Components.Identity.DTOs;
using SmartSolarMicrogrid.API.Components.Identity.Interfaces;
using SmartSolarMicrogrid.API.Components.Identity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartSolarMicrogrid.API.Components.Identity.Services
{
    public class RoleTabPermissionsService : IRoleTabPermissionsService
    {
        private readonly IRoleTabPermissionsRepository _repository;

        public RoleTabPermissionsService(IRoleTabPermissionsRepository repository)
        {
            _repository = repository;
        }

        public async Task<RoleTabPermissionsResponse> GetTabPermissionsByRoleAsync(Role role)
        {
            var permissions = await _repository.GetByRoleAsync(role);

            if (permissions == null)
            {
                // No configuration exists for this role - return null for VisibleTabs (show all)
                return new RoleTabPermissionsResponse
                {
                    Role = role.ToString(),
                    VisibleTabs = null,
                    UpdatedAt = null
                };
            }

            return new RoleTabPermissionsResponse
            {
                Role = permissions.Role.ToString(),
                VisibleTabs = permissions.VisibleTabs,
                UpdatedAt = permissions.UpdatedAt
            };
        }

        public async Task<List<RoleTabPermissionsResponse>> GetAllRolePermissionsAsync()
        {
            var allPermissions = await _repository.GetAllAsync();

            // Get all available roles
            var allRoles = Enum.GetValues<Role>().ToList();

            var responses = new List<RoleTabPermissionsResponse>();

            foreach (var role in allRoles)
            {
                var existing = allPermissions.FirstOrDefault(p => p.Role == role);
                if (existing != null)
                {
                    responses.Add(new RoleTabPermissionsResponse
                    {
                        Role = existing.Role.ToString(),
                        VisibleTabs = existing.VisibleTabs,
                        UpdatedAt = existing.UpdatedAt
                    });
                }
                else
                {
                    // No configuration for this role
                    responses.Add(new RoleTabPermissionsResponse
                    {
                        Role = role.ToString(),
                        VisibleTabs = null,
                        UpdatedAt = null
                    });
                }
            }

            return responses;
        }

        public async Task<RoleTabPermissionsResponse> UpdateTabPermissionsByRoleAsync(Role role, UpdateRoleTabPermissionsRequest request)
        {
            if (request.VisibleTabs == null)
            {
                // Remove configuration to restore default (show all tabs)
                await _repository.DeleteAsync(role);
                return new RoleTabPermissionsResponse
                {
                    Role = role.ToString(),
                    VisibleTabs = null,
                    UpdatedAt = null
                };
            }

            var permissions = new RoleTabPermissions
            {
                Role = role,
                VisibleTabs = request.VisibleTabs,
                UpdatedAt = DateTime.UtcNow
            };

            await _repository.CreateOrUpdateAsync(permissions);

            return new RoleTabPermissionsResponse
            {
                Role = permissions.Role.ToString(),
                VisibleTabs = permissions.VisibleTabs,
                UpdatedAt = permissions.UpdatedAt
            };
        }
    }
}