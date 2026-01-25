using Microsoft.EntityFrameworkCore;
using TestSystem.Data;
using TestSystem.Entities.DTOs.Student;
using TestSystem.MainContext;
using TestSystem.ServiceLayer.Interfaces.Student;

namespace TestSystem.ServiceLayer.Services.Student
{
    // Class to handle operations related to student tests
    public class StudentTestsService : IStudentTestsService
    {
        private readonly IDbContextFactory<BusinessDbContext> _dbFactory;
        private readonly ApplicationDbContext _identityContext;

        public StudentTestsService(
            IDbContextFactory<BusinessDbContext> dbFactory,
            ApplicationDbContext identityDb)
        {
            _dbFactory = dbFactory;
            _identityContext = identityDb;
        }

        // Method to get available tests for a student
        public async Task<List<StudentAvailableTestDto>> GetAvailableTestsAsync(string studentUserId)
        {
            await using var _businessContext = await _dbFactory.CreateDbContextAsync();

            // 1️) Get student with related teachers, tests and questions
            var student = await _businessContext.Students
                .Include(s => s.Teachers)
                    .ThenInclude(t => t.Tests)
                        .ThenInclude(t => t.Questions)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == studentUserId);

            if (student == null)
                return new();

            // 2) Get completed test results for the student
            var completedResults = await _businessContext.TestResults
                .Where(r => r.Student.UserId == studentUserId)
                .Select(r => new
                {
                    r.TestId,
                    r.Score
                })
                .ToListAsync();

            var completedDict = completedResults
                .ToDictionary(r => r.TestId, r => r.Score);

            // 3) Resolve teacher identity data
            var teacherUserIds = student.Teachers
                .Select(t => t.UserId)
                .Distinct()
                .ToList();

            var teachers = await _identityContext.Users
                .Where(u => teacherUserIds.Contains(u.Id))
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName
                })
                .ToListAsync();

            // 4) Flatten tests, avoid duplicates, build DTO
            var tests = student.Teachers
                .SelectMany(t => t.Tests.Select(test => new { Teacher = t, Test = test }))
                .GroupBy(x => x.Test.Id)
                .Select(g =>
                {
                    var item = g.First();
                    var teacherUser = teachers.First(u => u.Id == item.Teacher.UserId);

                    var isCompleted = completedDict.TryGetValue(item.Test.Id, out var score);

                    return new StudentAvailableTestDto
                    {
                        TestId = item.Test.Id,
                        Title = item.Test.Title,
                        TeacherName = $"{teacherUser.FirstName} {teacherUser.LastName}",
                        QuestionsCount = item.Test.Questions.Count,
                        MaxScore = item.Test.Questions.Sum(q => q.Points),

                        IsCompleted = isCompleted,
                        Score = isCompleted ? score : null
                    };
                })
                .ToList();

            return tests;
        }
    }
}
