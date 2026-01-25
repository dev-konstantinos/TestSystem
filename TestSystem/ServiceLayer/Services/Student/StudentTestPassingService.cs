using Microsoft.EntityFrameworkCore;
using TestSystem.Entities;
using TestSystem.Entities.DTOs.Student;
using TestSystem.MainContext;
using TestSystem.ServiceLayer.Interfaces.Student;

namespace TestSystem.ServiceLayer.Services.Student
{
    // Service for managing student test taking and submission
    public class StudentTestPassingService : IStudentTestPassingService
    {
        private readonly IDbContextFactory<BusinessDbContext> _dbFactory;

        public StudentTestPassingService(IDbContextFactory<BusinessDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        // Retrieves a test for a student, including questions and options
        public async Task<StudentTestDto> GetTestAsync(string studentUserId, int testId)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            var student = await db.Students
                .Include(s => s.Teachers)
                .FirstOrDefaultAsync(s => s.UserId == studentUserId)
                ?? throw new InvalidOperationException("Student not found");

            var test = await db.Tests
                .Include(t => t.Teachers)
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(t => t.Id == testId)
                ?? throw new InvalidOperationException("Test not found");

            if (!test.Teachers.Any(t => student.Teachers.Contains(t)))
                throw new InvalidOperationException("Access denied");

            var isCompleted = await db.TestResults.AnyAsync(r =>
                r.StudentId == student.Id && r.TestId == testId);

            if (isCompleted)
            {
                return new StudentTestDto
                {
                    TestId = test.Id,
                    Title = test.Title,
                    IsCompleted = true
                };
            }

            return new StudentTestDto
            {
                TestId = test.Id,
                Title = test.Title,
                IsCompleted = false,
                Questions = test.Questions.Select(q => new StudentQuestionDto
                {
                    QuestionId = q.Id,
                    Text = q.Text,
                    Points = q.Points,
                    Options = q.Options.Select(o => new StudentOptionDto
                    {
                        OptionId = o.Id,
                        Text = o.Text
                    }).ToList()
                }).ToList()
            };
        }

        // Submits a student's test answers and calculates the score
        public async Task SubmitAsync(string studentUserId, StudentTestSubmitDto dto)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            var student = await db.Students
                .FirstAsync(s => s.UserId == studentUserId);

            if (await db.TestResults.AnyAsync(r =>
                r.StudentId == student.Id && r.TestId == dto.TestId))
                throw new InvalidOperationException("Test already completed");

            var test = await db.Tests
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Options)
                .FirstAsync(t => t.Id == dto.TestId);

            int score = 0;

            foreach (var q in test.Questions)
            {
                if (!dto.Answers.TryGetValue(q.Id, out var selectedOption))
                    continue;

                var correct = q.Options.FirstOrDefault(o => o.IsCorrect);
                if (correct != null && correct.Id == selectedOption)
                    score += q.Points;
            }

            db.TestResults.Add(new TestResult
            {
                StudentId = student.Id,
                TestId = test.Id,
                Score = score,
                CompletedDate = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }
    }
}
