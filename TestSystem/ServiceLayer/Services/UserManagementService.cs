using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TestSystem.Data;
using TestSystem.Entities;
using TestSystem.Entities.DTOs.User;
using TestSystem.Infrastructure.Identity;
using TestSystem.MainContext;
using TestSystem.ServiceLayer.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;


namespace TestSystem.ServiceLayer.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly BusinessDbContext _businessContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserManagementService(
            UserManager<ApplicationUser> userManager,
            BusinessDbContext businessDb,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _businessContext = businessDb;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<UserAdminDto>> GetAllAsync()
        {
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

        public async Task SetRoleAsync(string userId, string role, bool enabled)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new InvalidOperationException("User not found");

            // SERVER-SIDE SELF-ADMIN PROTECTION
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

        private async Task SyncStudent(string userId, bool enabled)
        {
            var student = await _businessContext.Students
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (enabled && student == null)
            {
                _businessContext.Students.Add(new Student { UserId = userId });
                await _businessContext.SaveChangesAsync();
            }

            if (!enabled && student != null)
            {
                _businessContext.Students.Remove(student);
                await _businessContext.SaveChangesAsync();
            }
        }

        private async Task SyncTeacher(string userId, bool enabled)
        {
            var teacher = await _businessContext.Teachers
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (enabled && teacher == null)
            {
                _businessContext.Teachers.Add(new Teacher { UserId = userId });
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
