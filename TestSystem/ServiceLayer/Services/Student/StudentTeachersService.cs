using Microsoft.EntityFrameworkCore;
using TestSystem.Data;
using TestSystem.Entities.DTOs.Student;
using TestSystem.MainContext;
using TestSystem.ServiceLayer.Interfaces.Student;

namespace TestSystem.ServiceLayer.Services.Student
{
    // Service to manage the relationship between students and their teachers
    public class StudentTeachersService : IStudentTeachersService
    {
        private readonly IDbContextFactory<BusinessDbContext> _dbFactory;
        private readonly ApplicationDbContext _identityContext;

        public StudentTeachersService(
            IDbContextFactory<BusinessDbContext> dbFactory,
            ApplicationDbContext identityDb)
        {
            _dbFactory = dbFactory;
            _identityContext = identityDb;
        }

        // method to get all teachers for a specific student
        public async Task<List<StudentTeacherDto>> GetMyTeachersAsync(string studentUserId)
        {
            await using var _businessContext = await _dbFactory.CreateDbContextAsync();

            var studentId = await _businessContext.Students
                .Where(s => s.UserId == studentUserId)
                .Select(s => s.Id)
                .FirstOrDefaultAsync();

            if (studentId == 0)
                return new();

            // 1️) get teachers from business db
            var teachers = await _businessContext.Teachers
                .Where(t => t.Students.Any(s => s.Id == studentId))
                .Select(t => new
                {
                    t.Id,
                    t.UserId,
                    t.JoinedDate,
                    TestsCount = t.Tests.Count
                })
                .AsNoTracking()
                .ToListAsync();

            var userIds = teachers.Select(t => t.UserId).ToList();

            // 2️) get users from identity db
            var users = await _identityContext.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FirstName,
                    u.LastName
                })
                .ToListAsync();

            // 3️) merge
            return teachers.Select(t =>
            {
                var user = users.First(u => u.Id == t.UserId);

                return new StudentTeacherDto
                {
                    TeacherId = t.Id,
                    UserId = user.Id,
                    Email = user.Email!,
                    FullName = $"{user.FirstName} {user.LastName}",
                    JoinedDate = t.JoinedDate,
                    TestsCount = t.TestsCount
                };
            }).ToList();
        }
    }
}
