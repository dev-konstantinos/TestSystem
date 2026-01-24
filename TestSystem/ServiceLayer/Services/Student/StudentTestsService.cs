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
        private readonly BusinessDbContext _businessContext;
        private readonly ApplicationDbContext _identityContext;

        public StudentTestsService(
            BusinessDbContext businessDb,
            ApplicationDbContext identityDb)
        {
            _businessContext = businessDb;
            _identityContext = identityDb;
        }

        // Method to get available tests for a student
        public async Task<List<StudentAvailableTestDto>> GetAvailableTestsAsync(string studentUserId)
        {
            // 1️) Get student with related teachers, tests and questions
            var student = await _businessContext.Students
                .Include(s => s.Teachers)
                    .ThenInclude(t => t.Tests)
                        .ThenInclude(t => t.Questions)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == studentUserId);

            if (student == null)
                return new();

            // 2️) Resolve teacher identity data
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

            // 3️) Flatten tests, avoid duplicates, build DTO
            var tests = student.Teachers
                .SelectMany(t => t.Tests.Select(test => new { Teacher = t, Test = test }))
                .GroupBy(x => x.Test.Id)
                .Select(g =>
                {
                    var item = g.First();
                    var teacherUser = teachers.First(u => u.Id == item.Teacher.UserId);

                    return new StudentAvailableTestDto
                    {
                        TestId = item.Test.Id,
                        Title = item.Test.Title,
                        TeacherName = $"{teacherUser.FirstName} {teacherUser.LastName}",
                        QuestionsCount = item.Test.Questions.Count,
                        MaxScore = item.Test.Questions.Sum(q => q.Points)
                    };
                })
                .ToList();

            return tests;
        }
    }
}
