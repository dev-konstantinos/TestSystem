using Microsoft.EntityFrameworkCore;
using TestSystem.Data;
using TestSystem.Entities.DTOs.Teacher;
using TestSystem.MainContext;
using TestSystem.ServiceLayer.Interfaces.Teacher;

namespace TestSystem.ServiceLayer.Services.Teacher
{
    public class TeacherResultsService : ITeacherResultsService
    {
        private readonly IDbContextFactory<BusinessDbContext> _dbFactory;
        private readonly ApplicationDbContext _identityContext;

        public TeacherResultsService(
            IDbContextFactory<BusinessDbContext> dbFactory,
            ApplicationDbContext identityDb)
        {
            _dbFactory = dbFactory;
            _identityContext = identityDb;
        }

        public async Task<List<TeacherResultDto>> GetMyResultsAsync(string teacherUserId)
        {
            await using var _businessContext = await _dbFactory.CreateDbContextAsync();

            // 1️) Getting teacherId
            var teacherId = await _businessContext.Teachers
                .Where(t => t.UserId == teacherUserId)
                .Select(t => t.Id)
                .FirstOrDefaultAsync();

            if (teacherId == 0)
                return new();

            // 2) Getting test results for teacher's tests
            var results = await _businessContext.TestResults
                .Where(r => r.Test.Teachers.Any(t => t.Id == teacherId))
                .Select(r => new
                {
                    r.Score,
                    r.CompletedDate,
                    TestId = r.Test.Id,
                    r.Test.Title,
                    MaxScore = r.Test.Questions.Sum(q => q.Points),
                    StudentId = r.Student.Id,
                    r.Student.UserId
                })
                .AsNoTracking()
                .ToListAsync();

            // 3️) Identity-data for students
            var userIds = results
                .Select(r => r.UserId)
                .Distinct()
                .ToList();

            var users = await _identityContext.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email
                })
                .ToListAsync();

            // 4️) Collect final results
            return results.Select(r =>
            {
                var user = users.First(u => u.Id == r.UserId);

                return new TeacherResultDto
                {
                    TestId = r.TestId,
                    TestTitle = r.Title,
                    StudentId = r.StudentId,
                    StudentName = $"{user.FirstName} {user.LastName}",
                    StudentEmail = user.Email!,
                    Score = r.Score,
                    MaxScore = r.MaxScore,
                    CompletedDate = r.CompletedDate
                };
            }).ToList();
        }
    }
}
