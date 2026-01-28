using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TestSystem.Data;
using TestSystem.Entities.DTOs.Admin;
using TestSystem.Infrastructure.Identity;
using TestSystem.MainContext;
using TestSystem.ServiceLayer.Interfaces.Admin;

namespace TestSystem.ServiceLayer.Services.Admin
{
    // Class for managing user roles and synchronizing with business entities
    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDbContextFactory<BusinessDbContext> _dbFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserManagementService(
            UserManager<ApplicationUser> userManager,
            IDbContextFactory<BusinessDbContext> dbFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _dbFactory = dbFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        // Retrieves all users along with their roles and related business entity statuses
        public async Task<List<UserAdminDto>> GetAllAsync()
        {
            await using var _businessContext = await _dbFactory.CreateDbContextAsync();

            var users = await _userManager.Users.ToListAsync();

            var studentIds = await _businessContext.Students
                .Select(s => s.UserId)
                .ToListAsync();

            var teacherIds = await _businessContext.Teachers
                .Select(t => t.UserId)
                .ToListAsync();

            var result = new List<UserAdminDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                result.Add(new UserAdminDto
                {
                    UserId = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName!,
                    LastName = user.LastName!,
                    IsAdmin = roles.Contains(AppRoles.Admin),
                    IsStudent = studentIds.Contains(user.Id),
                    IsTeacher = teacherIds.Contains(user.Id)
                });
            }
            return result;
        }

        // Sets or unsets a role for a user and synchronizes with business entities
        public async Task SetRoleAsync(string userId, string role, bool enabled)
        {
            await using var _businessContext = await _dbFactory.CreateDbContextAsync();

            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new InvalidOperationException("User not found");

            // protect against removing Admin role from oneself
            var currentUserId = _httpContextAccessor.HttpContext?
                .User?
                .FindFirst(ClaimTypes.NameIdentifier)?
                .Value;

            if (role == AppRoles.Admin &&
                userId == currentUserId &&
                !enabled)
            {
                throw new InvalidOperationException(
                    "You cannot remove Admin role from yourself");
            }

            if (enabled)
            {
                if (!await _userManager.IsInRoleAsync(user, role))
                    await _userManager.AddToRoleAsync(user, role);
            }
            else
            {
                if (await _userManager.IsInRoleAsync(user, role))
                    await _userManager.RemoveFromRoleAsync(user, role);
            }

            if (role == AppRoles.Student)
                await SyncStudent(userId, enabled);

            if (role == AppRoles.Teacher)
                await SyncTeacher(userId, enabled);
        }

        // Deletes a user after ensuring no business roles are assigned (temporarily solution)
        public async Task DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new InvalidOperationException("User not found");

            // not allow deleting oneself
            var currentUserId = _httpContextAccessor.HttpContext?
                .User?
                .FindFirst(ClaimTypes.NameIdentifier)?
                .Value;

            if (userId == currentUserId)
                throw new InvalidOperationException("You cannot delete yourself");

            // checking for existing business roles
            await using var businessContext = await _dbFactory.CreateDbContextAsync();

            var isStudent = await businessContext.Students.AnyAsync(s => s.UserId == userId);
            var isTeacher = await businessContext.Teachers.AnyAsync(t => t.UserId == userId);

            if (isStudent || isTeacher)
                throw new InvalidOperationException(
                    "User has business roles. Remove roles before deleting.");

            // delete user from Identity
            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                throw new InvalidOperationException(
                    string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        // Synchronizes the Student entity based on role assignment
        private async Task SyncStudent(string userId, bool enabled)
        {
            await using var _businessContext = await _dbFactory.CreateDbContextAsync();

            var student = await _businessContext.Students
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (enabled && student == null)
            {
                _businessContext.Students.Add(new Entities.Student { UserId = userId });
                await _businessContext.SaveChangesAsync();
            }

            if (!enabled && student != null)
            {
                _businessContext.Students.Remove(student);
                await _businessContext.SaveChangesAsync();
            }
        }

        // Synchronizes the Teacher entity based on role assignment
        private async Task SyncTeacher(string userId, bool enabled)
        {
            await using var _businessContext = await _dbFactory.CreateDbContextAsync();

            var teacher = await _businessContext.Teachers
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (enabled && teacher == null)
            {
                _businessContext.Teachers.Add(new Entities.Teacher { UserId = userId });
                await _businessContext.SaveChangesAsync();
            }

            if (!enabled && teacher != null)
            {
                _businessContext.Teachers.Remove(teacher);
                await _businessContext.SaveChangesAsync();
            }
        }
    }
}
