using Microsoft.EntityFrameworkCore;
using TestSystem.Data;
using TestSystem.Entities.DTOs.Teacher;
using TestSystem.MainContext;
using TestSystem.ServiceLayer.Interfaces;

namespace TestSystem.ServiceLayer.Services
{
    public class TeacherStudentsService : ITeacherStudentsService
    {
        private readonly BusinessDbContext _businessContext;
        private readonly ApplicationDbContext _identityContext;

        public TeacherStudentsService(
            BusinessDbContext db,
            ApplicationDbContext identityDb)
        {
            _businessContext = db;
            _identityContext = identityDb;
        }

        public async Task<List<TeacherStudentDto>> GetMyStudentsAsync(string teacherUserId)
        {
            var teacherId = await _businessContext.Teachers
                .Where(t => t.UserId == teacherUserId)
                .Select(t => t.Id)
                .FirstOrDefaultAsync();

            if (teacherId == 0)
                return new();

            // 1️) Getting students from business DB
            var students = await _businessContext.Students
                .Where(s => s.Teachers.Any(t => t.Id == teacherId))
                .Select(s => new
                {
                    s.Id,
                    s.UserId,
                    s.EnrolledDate,
                    TestsPassed = s.TestResults.Count,
                    AverageScore = s.TestResults.Any()
                        ? s.TestResults.Average(r => r.Score)
                        : (double?)null
                })
                .AsNoTracking()
                .ToListAsync();

            var userIds = students.Select(s => s.UserId).ToList();

            // 2️) Getting users from identity DB
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

            // 3️) Combining data from both DBs
            return students.Select(s =>
            {
                var user = users.First(u => u.Id == s.UserId);

                return new TeacherStudentDto
                {
                    StudentId = s.Id,
                    UserId = user.Id,
                    Email = user.Email!,
                    FullName = $"{user.FirstName} {user.LastName}",
                    EnrolledDate = s.EnrolledDate,
                    TestsPassed = s.TestsPassed,
                    AverageScore = s.AverageScore
                };
            }).ToList();
        }
    }
}
