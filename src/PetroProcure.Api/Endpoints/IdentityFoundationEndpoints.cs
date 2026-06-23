using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PetroProcure.Api.Security;
using PetroProcure.Application.Security;
using PetroProcure.Domain.Modules.Organization;
using PetroProcure.Domain.Modules.Security;
using PetroProcure.Infrastructure.Identity;
using PetroProcure.Infrastructure.Persistence;
using PetroProcure.Contracts.V1.Identity;
using PetroProcure.Contracts.V1.Organization;
using PetroProcure.Contracts.V1.Workflow;

namespace PetroProcure.Api.Endpoints;

public static class IdentityFoundationEndpoints
{
    public static IEndpointRouteBuilder MapIdentityFoundationEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");

        api.MapGet("/lookups/departments", async (PetroProcureDbContext dbContext) =>
            await dbContext.Departments
                .AsNoTracking()
                .Where(department => department.IsActive)
                .OrderBy(department => department.Name)
                .Select(department => new PetroProcure.Contracts.V1.Common.LookupDto(
                    department.Id, department.Type.ToString(), department.Name))
                .ToListAsync())
            .RequireAuthorization();

        api.MapGet("/lookups/users", async (PetroProcureDbContext dbContext, UserManager<ApplicationUser> userManager) =>
        {
            var users = await userManager.Users.AsNoTracking().OrderBy(user => user.UserName).ToListAsync();
            var profileIds = users.Where(user => user.UserProfileId.HasValue).Select(user => user.UserProfileId!.Value).ToArray();
            var profiles = await dbContext.ApplicationUserProfiles
                .AsNoTracking()
                .Where(profile => profileIds.Contains(profile.Id))
                .ToDictionaryAsync(profile => profile.Id);

            return users.Select(user =>
            {
                profiles.TryGetValue(user.UserProfileId ?? Guid.Empty, out var profile);
                return new PetroProcure.Contracts.V1.Common.LookupDto(
                    user.Id, user.UserName ?? string.Empty, profile?.DisplayName ?? user.UserName ?? user.Id.ToString());
            });
        })
        .RequireAuthorization();

        api.MapGet("/admin/dashboard", async (PetroProcureDbContext dbContext, UserManager<ApplicationUser> userManager) =>
            new AdminDashboardDto(
                await userManager.Users.CountAsync(),
                await dbContext.Roles.CountAsync(),
                await dbContext.Departments.CountAsync(),
                await dbContext.Permissions.CountAsync(permission => permission.IsActive),
                await dbContext.WorkflowActionDefinitions.CountAsync()))
            .RequireAuthorization(PermissionPolicyNames.For(ApplicationPermissions.AdminManageSettings));

        api.MapGet("/departments", async (PetroProcureDbContext dbContext) =>
            await dbContext.Departments
                .AsNoTracking()
                .OrderBy(department => department.Type)
                .Select(department => new DepartmentDto(department.Id, department.Name, department.Type.ToString(), department.IsActive))
                .ToListAsync())
            .RequireAuthorization(PermissionPolicyNames.For(ApplicationPermissions.AdminManageDepartments));

        api.MapPut("/departments/{id:guid}", async (
            Guid id,
            UpdateDepartmentRequest request,
            PetroProcureDbContext dbContext) =>
        {
            var department = await dbContext.Departments.FindAsync(id);
            if (department is null)
            {
                return Results.NotFound(new { Error = "Department was not found." });
            }

            department.Rename(request.Name);
            await dbContext.SaveChangesAsync();
            return Results.Ok(new DepartmentDto(department.Id, department.Name, department.Type.ToString(), department.IsActive));
        })
        .RequireAuthorization(PermissionPolicyNames.For(ApplicationPermissions.AdminManageDepartments));

        api.MapGet("/permissions", async (PetroProcureDbContext dbContext) =>
            await dbContext.Permissions
                .AsNoTracking()
                .OrderBy(permission => permission.Name)
                .Select(permission => new PermissionDto(permission.Id, permission.Name, permission.Description, permission.IsActive))
                .ToListAsync())
            .RequireAuthorization(PermissionPolicyNames.For(ApplicationPermissions.AdminManageRoles));

        api.MapGet("/admin/audit-logs", async (PetroProcureDbContext dbContext) =>
            await dbContext.AdminAuditLogs
                .AsNoTracking()
                .OrderByDescending(log => log.CreatedAt)
                .Take(200)
                .Select(log => new AdminAuditLogDto(log.Id, log.ActorUserId, log.Action, log.EntityType,
                    log.EntityId, log.Summary, log.CreatedAt))
                .ToListAsync())
            .RequireAuthorization(PermissionPolicyNames.For(ApplicationPermissions.AdminManageSettings));

        api.MapGet("/admin/settings", async (PetroProcureDbContext dbContext) =>
            await dbContext.SystemSettings
                .AsNoTracking()
                .OrderBy(setting => setting.Key)
                .Select(setting => new SystemSettingDto(setting.Key,
                    setting.IsSecret ? null : setting.Value,
                    setting.Description,
                    setting.IsSecret))
                .ToListAsync())
            .RequireAuthorization(PermissionPolicyNames.For(ApplicationPermissions.AdminManageSettings));

        api.MapPut("/admin/settings/{key}", async (
            string key,
            UpdateSystemSettingRequest request,
            PetroProcureDbContext dbContext,
            ICurrentUserService currentUser,
            AdminAuditService audit,
            CancellationToken ct) =>
        {
            var setting = await dbContext.SystemSettings.FindAsync([key], ct);
            if (setting is null)
            {
                setting = new SystemSetting { Key = key };
                dbContext.SystemSettings.Add(setting);
            }
            setting.Value = request.Value;
            setting.Description = request.Description;
            setting.IsSecret = request.IsSecret;
            setting.UpdatedAt = DateTime.UtcNow;
            setting.UpdatedByUserId = currentUser.UserId;
            await dbContext.SaveChangesAsync(ct);
            await audit.LogAsync("UpdateSetting", "SystemSetting", key, $"Setting '{key}' updated.", ct);
            return Results.Ok(new SystemSettingDto(setting.Key, setting.IsSecret ? null : setting.Value, setting.Description, setting.IsSecret));
        })
        .RequireAuthorization(PermissionPolicyNames.For(ApplicationPermissions.AdminManageSettings));

        api.MapGet("/roles", async (
            PetroProcureDbContext dbContext,
            RoleManager<IdentityRole<Guid>> roleManager) =>
        {
            var roles = await roleManager.Roles.AsNoTracking().OrderBy(role => role.Name).ToListAsync();
            var roleIds = roles.Select(role => role.Id).ToArray();
            var permissions = await dbContext.RolePermissions
                .AsNoTracking()
                .Where(rolePermission => roleIds.Contains(rolePermission.RoleId))
                .Join(dbContext.Permissions.AsNoTracking(),
                    rolePermission => rolePermission.PermissionId,
                    permission => permission.Id,
                    (rolePermission, permission) => new { rolePermission.RoleId, permission.Name })
                .ToListAsync();

            return roles.Select(role => new RoleDto(
                role.Id,
                role.Name ?? string.Empty,
                permissions.Where(permission => permission.RoleId == role.Id).Select(permission => permission.Name).Order().ToArray()));
        })
        .RequireAuthorization(PermissionPolicyNames.For(ApplicationPermissions.AdminManageRoles));

        api.MapPost("/roles", async (
            CreateRoleRequest request,
            RoleManager<IdentityRole<Guid>> roleManager,
            AdminAuditService audit,
            CancellationToken ct) =>
        {
            var role = new IdentityRole<Guid>(request.Name) { Id = Guid.NewGuid() };
            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded)
                return Results.ValidationProblem(result.Errors.ToDictionary(error => error.Code, error => new[] { error.Description }));
            await audit.LogAsync("CreateRole", "Role", role.Id.ToString(), $"Role '{role.Name}' created.", ct);
            return Results.Created($"/api/roles/{role.Id}", new RoleDto(role.Id, role.Name ?? string.Empty, []));
        })
        .RequireAuthorization(PermissionPolicyNames.For(ApplicationPermissions.AdminManageRoles));

        api.MapPut("/roles/{roleId:guid}", async (
            Guid roleId,
            UpdateRoleRequest request,
            RoleManager<IdentityRole<Guid>> roleManager,
            AdminAuditService audit,
            CancellationToken ct) =>
        {
            var role = await roleManager.FindByIdAsync(roleId.ToString());
            if (role is null) return Results.NotFound(new { Error = "Role was not found." });
            role.Name = request.Name;
            role.NormalizedName = request.Name.ToUpperInvariant();
            var result = await roleManager.UpdateAsync(role);
            if (!result.Succeeded)
                return Results.ValidationProblem(result.Errors.ToDictionary(error => error.Code, error => new[] { error.Description }));
            await audit.LogAsync("UpdateRole", "Role", role.Id.ToString(), $"Role renamed to '{role.Name}'.", ct);
            return Results.NoContent();
        })
        .RequireAuthorization(PermissionPolicyNames.For(ApplicationPermissions.AdminManageRoles));

        api.MapGet("/users", async (PetroProcureDbContext dbContext, UserManager<ApplicationUser> userManager) =>
        {
            var users = await userManager.Users
                .AsNoTracking()
                .OrderBy(user => user.UserName)
                .ToListAsync();

            var profileIds = users
                .Where(user => user.UserProfileId.HasValue)
                .Select(user => user.UserProfileId!.Value)
                .ToArray();

            var profiles = await dbContext.ApplicationUserProfiles
                .AsNoTracking()
                .Where(profile => profileIds.Contains(profile.Id))
                .ToDictionaryAsync(profile => profile.Id);

            var profileDepartmentIds = await dbContext.UserDepartments
                .AsNoTracking()
                .Where(userDepartment => profileIds.Contains(userDepartment.UserProfileId))
                .GroupBy(userDepartment => userDepartment.UserProfileId)
                .ToDictionaryAsync(
                    group => group.Key,
                    group => group.Select(userDepartment => userDepartment.DepartmentId).ToArray());

            var result = new List<UserDto>();

            foreach (var user in users)
            {
                profiles.TryGetValue(user.UserProfileId ?? Guid.Empty, out var profile);
                var roles = await userManager.GetRolesAsync(user);
                profileDepartmentIds.TryGetValue(user.UserProfileId ?? Guid.Empty, out var departmentIds);
                result.Add(new UserDto(user.Id, user.UserName ?? string.Empty, user.Email, profile?.DisplayName,
                    user.UserProfileId, roles.ToArray(), departmentIds ?? []));
            }

            return result;
        })
        .RequireAuthorization(PermissionPolicyNames.For(ApplicationPermissions.AdminManageUsers));

        api.MapPut("/users/{id:guid}", async (
            Guid id,
            UpdateUserRequest request,
            PetroProcureDbContext dbContext,
            UserManager<ApplicationUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user is null)
            {
                return Results.NotFound(new { Error = "User was not found." });
            }

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                user.Email = request.Email.Trim();
                user.NormalizedEmail = user.Email.ToUpperInvariant();
            }

            if (user.UserProfileId.HasValue)
            {
                var profile = await dbContext.ApplicationUserProfiles.FindAsync(user.UserProfileId.Value);
                if (profile is not null && !string.IsNullOrWhiteSpace(request.DisplayName))
                {
                    profile.Rename(request.DisplayName);
                }
            }

            if (request.IsActive.HasValue)
            {
                user.LockoutEnd = request.IsActive.Value ? null : DateTimeOffset.MaxValue;
            }

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return Results.ValidationProblem(updateResult.Errors.ToDictionary(error => error.Code, error => new[] { error.Description }));
            }

            await dbContext.SaveChangesAsync();
            return Results.NoContent();
        })
        .RequireAuthorization(PermissionPolicyNames.For(ApplicationPermissions.AdminManageUsers));

        api.MapPost("/users/{id:guid}/reset-password", async (
            Guid id,
            ResetPasswordRequest request,
            UserManager<ApplicationUser> userManager,
            AdminAuditService audit,
            CancellationToken ct) =>
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user is null) return Results.NotFound(new { Error = "User was not found." });
            if (await userManager.HasPasswordAsync(user))
            {
                var remove = await userManager.RemovePasswordAsync(user);
                if (!remove.Succeeded)
                    return Results.ValidationProblem(remove.Errors.ToDictionary(error => error.Code, error => new[] { error.Description }));
            }
            var add = await userManager.AddPasswordAsync(user, request.NewPassword);
            if (!add.Succeeded)
                return Results.ValidationProblem(add.Errors.ToDictionary(error => error.Code, error => new[] { error.Description }));
            await audit.LogAsync("ResetPassword", "User", user.Id.ToString(), $"Password reset for '{user.UserName}'.", ct);
            return Results.NoContent();
        })
        .RequireAuthorization(PermissionPolicyNames.For(ApplicationPermissions.AdminManageUsers));

        api.MapPost("/users", async (
            CreateUserRequest request,
            PetroProcureDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager) =>
        {
            var profile = new ApplicationUserProfile(Guid.NewGuid(), request.DisplayName, request.Email);
            dbContext.ApplicationUserProfiles.Add(profile);

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = request.UserName,
                Email = request.Email,
                EmailConfirmed = request.EmailConfirmed,
                UserProfileId = profile.Id
            };

            var createResult = await userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                return Results.ValidationProblem(createResult.Errors.ToDictionary(error => error.Code, error => new[] { error.Description }));
            }

            foreach (var role in request.Roles.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    return Results.BadRequest(new { Error = $"Role '{role}' does not exist." });
                }

                var roleResult = await userManager.AddToRoleAsync(user, role);
                if (!roleResult.Succeeded)
                {
                    return Results.ValidationProblem(roleResult.Errors.ToDictionary(error => error.Code, error => new[] { error.Description }));
                }
            }

            await dbContext.SaveChangesAsync();

            return Results.Created($"/api/users/{user.Id}", new UserDto(
                user.Id, user.UserName!, user.Email, profile.DisplayName, profile.Id, request.Roles, []));
        })
        .RequireAuthorization(PermissionPolicyNames.For(ApplicationPermissions.AdminManageUsers));

        api.MapPost("/users/{id:guid}/roles", async (
            Guid id,
            AssignUserRolesRequest request,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager) =>
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user is null)
            {
                return Results.NotFound(new { Error = "User was not found." });
            }

            foreach (var role in request.Roles.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    return Results.BadRequest(new { Error = $"Role '{role}' does not exist." });
                }
            }

            var currentRoles = await userManager.GetRolesAsync(user);
            var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                return Results.ValidationProblem(removeResult.Errors.ToDictionary(error => error.Code, error => new[] { error.Description }));
            }

            var addResult = await userManager.AddToRolesAsync(user, request.Roles.Distinct(StringComparer.OrdinalIgnoreCase));
            if (!addResult.Succeeded)
            {
                return Results.ValidationProblem(addResult.Errors.ToDictionary(error => error.Code, error => new[] { error.Description }));
            }

            return Results.NoContent();
        })
        .RequireAuthorization(PermissionPolicyNames.For(ApplicationPermissions.AdminManageUsers));

        api.MapPost("/users/{id:guid}/departments", async (
            Guid id,
            AssignUserDepartmentRequest request,
            PetroProcureDbContext dbContext) =>
        {
            var userExists = await dbContext.Users.AnyAsync(user => user.Id == id);
            if (!userExists)
            {
                return Results.NotFound(new { Error = "User was not found." });
            }

            var departmentExists = await dbContext.Departments.AnyAsync(department => department.Id == request.DepartmentId);
            if (!departmentExists)
            {
                return Results.BadRequest(new { Error = "Department was not found." });
            }

            var alreadyAssigned = await dbContext.UserDepartments
                .AnyAsync(userDepartment => userDepartment.UserProfileId == request.UserProfileId && userDepartment.DepartmentId == request.DepartmentId);

            if (alreadyAssigned)
            {
                return Results.Conflict(new { Error = "User profile is already assigned to the department." });
            }

            var assignment = new UserDepartment(Guid.NewGuid(), request.UserProfileId, request.DepartmentId, request.IsPrimary);
            dbContext.UserDepartments.Add(assignment);
            await dbContext.SaveChangesAsync();

            return Results.Created($"/api/users/{id}/departments/{assignment.Id}",
                new UserDepartmentDto(assignment.Id, assignment.UserProfileId, assignment.DepartmentId, assignment.IsPrimary));
        })
        .RequireAuthorization(PermissionPolicyNames.For(ApplicationPermissions.AdminManageUsers));

        api.MapDelete("/users/{id:guid}/departments/{assignmentId:guid}", async (
            Guid id,
            Guid assignmentId,
            PetroProcureDbContext dbContext,
            AdminAuditService audit,
            CancellationToken ct) =>
        {
            var assignment = await dbContext.UserDepartments.FindAsync([assignmentId], ct);
            if (assignment is null) return Results.NotFound(new { Error = "Assignment was not found." });
            dbContext.UserDepartments.Remove(assignment);
            await dbContext.SaveChangesAsync(ct);
            await audit.LogAsync("RemoveUserDepartment", "User", id.ToString(), $"Department assignment '{assignmentId}' removed.", ct);
            return Results.NoContent();
        })
        .RequireAuthorization(PermissionPolicyNames.For(ApplicationPermissions.AdminManageUsers));

        api.MapPost("/roles/{roleId:guid}/permissions", async (
            Guid roleId,
            AssignRolePermissionsRequest request,
            PetroProcureDbContext dbContext) =>
        {
            var roleExists = await dbContext.Roles.AnyAsync(role => role.Id == roleId);
            if (!roleExists)
            {
                return Results.NotFound(new { Error = "Role was not found." });
            }

            var permissionIds = request.PermissionIds.Distinct().ToArray();
            var existingPermissionIds = await dbContext.Permissions
                .Where(permission => permissionIds.Contains(permission.Id))
                .Select(permission => permission.Id)
                .ToListAsync();

            if (existingPermissionIds.Count != permissionIds.Length)
            {
                return Results.BadRequest(new { Error = "One or more permissions were not found." });
            }

            var existingAssignments = await dbContext.RolePermissions
                .Where(rolePermission => rolePermission.RoleId == roleId)
                .ToListAsync();

            foreach (var assignment in existingAssignments.Where(assignment => !permissionIds.Contains(assignment.PermissionId)))
            {
                dbContext.RolePermissions.Remove(assignment);
            }

            var existingAssignmentIds = existingAssignments.Select(assignment => assignment.PermissionId).ToArray();
            foreach (var permissionId in permissionIds.Except(existingAssignmentIds))
            {
                dbContext.RolePermissions.Add(new RolePermission(Guid.NewGuid(), roleId, permissionId));
            }

            await dbContext.SaveChangesAsync();
            await dbContext.AdminAuditLogs.AddAsync(new AdminAuditLog
            {
                Id = Guid.NewGuid(),
                Action = "AssignRolePermissions",
                EntityType = "Role",
                EntityId = roleId.ToString(),
                Summary = $"{permissionIds.Length} permissions assigned.",
                CreatedAt = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync();
            return Results.NoContent();
        })
        .RequireAuthorization(PermissionPolicyNames.For(ApplicationPermissions.AdminManageRoles));

        api.MapGet("/workflow/action-definitions", async (PetroProcureDbContext dbContext) =>
            await dbContext.WorkflowActionDefinitions
                .AsNoTracking()
                .OrderBy(definition => definition.FromDepartmentType)
                .ThenBy(definition => definition.Code)
                .Select(definition => new WorkflowActionDefinitionDto(
                    definition.Id,
                    definition.Code,
                    definition.Title,
                    definition.FromDepartmentType,
                    definition.ToDepartmentType,
                    definition.FromStatus,
                    definition.ToStatus,
                    definition.RequiredPermission,
                    definition.RequiresComment,
                    definition.IsReturnAction,
                    definition.IsFinalAction,
                    definition.IsActive))
                .ToListAsync())
            .RequireAuthorization(PermissionPolicyNames.For(ApplicationPermissions.AdminManageSettings));

        api.MapPut("/workflow/action-definitions/{id:guid}", async (
            Guid id,
            UpdateWorkflowActionDefinitionRequest request,
            PetroProcureDbContext dbContext,
            AdminAuditService audit,
            CancellationToken ct) =>
        {
            var action = await dbContext.WorkflowActionDefinitions.FindAsync([id], ct);
            if (action is null) return Results.NotFound(new { Error = "Workflow action was not found." });
            action.Update(request.Title, request.RequiredPermission, request.RequiresComment, request.IsActive);
            await dbContext.SaveChangesAsync(ct);
            await audit.LogAsync("UpdateWorkflowAction", "WorkflowActionDefinition", id.ToString(), $"Workflow action '{action.Code}' updated.", ct);
            return Results.NoContent();
        })
        .RequireAuthorization(PermissionPolicyNames.For(ApplicationPermissions.AdminManageSettings));

        api.MapPost("/workflow/action-definitions/{id:guid}/activate", async (
            Guid id,
            PetroProcureDbContext dbContext,
            AdminAuditService audit,
            CancellationToken ct) =>
        {
            var action = await dbContext.WorkflowActionDefinitions.FindAsync([id], ct);
            if (action is null) return Results.NotFound(new { Error = "Workflow action was not found." });
            action.Activate();
            await dbContext.SaveChangesAsync(ct);
            await audit.LogAsync("ActivateWorkflowAction", "WorkflowActionDefinition", id.ToString(), $"Workflow action '{action.Code}' activated.", ct);
            return Results.NoContent();
        })
        .RequireAuthorization(PermissionPolicyNames.For(ApplicationPermissions.AdminManageSettings));

        api.MapPost("/workflow/action-definitions/{id:guid}/deactivate", async (
            Guid id,
            PetroProcureDbContext dbContext,
            AdminAuditService audit,
            CancellationToken ct) =>
        {
            var action = await dbContext.WorkflowActionDefinitions.FindAsync([id], ct);
            if (action is null) return Results.NotFound(new { Error = "Workflow action was not found." });
            action.Deactivate();
            await dbContext.SaveChangesAsync(ct);
            await audit.LogAsync("DeactivateWorkflowAction", "WorkflowActionDefinition", id.ToString(), $"Workflow action '{action.Code}' deactivated.", ct);
            return Results.NoContent();
        })
        .RequireAuthorization(PermissionPolicyNames.For(ApplicationPermissions.AdminManageSettings));

        return app;
    }

}
