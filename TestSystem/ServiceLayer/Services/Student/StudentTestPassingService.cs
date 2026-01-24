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
        private readonly BusinessDbContext _businessContext;

        public StudentTestPassingService(BusinessDbContext db)
        {
            _businessContext = db;
        }

        // Retrieves a test for a student to take, ensuring access and completion checks
        public async Task<StudentTestDto> GetTestAsync(string studentUserId, int testId)
        {
            var student = await _businessContext.Students
                .Include(s => s.Teachers)
                .FirstOrDefaultAsync(s => s.UserId == studentUserId)
                ?? throw new InvalidOperationException("Student not found");

            var test = await _businessContext.Tests
                .Include(t => t.Teachers)
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(t => t.Id == testId)
                ?? throw new InvalidOperationException("Test not found");

            // access check
            if (!test.Teachers.Any(t => student.Teachers.Contains(t)))
                throw new InvalidOperationException("Access denied");

            // already completed check
            if (await _businessContext.TestResults.AnyAsync(r =>
                r.StudentId == student.Id && r.TestId == testId))
                throw new InvalidOperationException("Test already completed");

            return new StudentTestDto
            {
                TestId = test.Id,
                Title = test.Title,
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
            var student = await _businessContext.Students
                .FirstAsync(s => s.UserId == studentUserId);

            var test = await _businessContext.Tests
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Options)
                .FirstAsync(t => t.Id == dto.TestId);

            if (await _businessContext.TestResults.AnyAsync(r =>
                r.StudentId == student.Id && r.TestId == test.Id))
                throw new InvalidOperationException("Already submitted");

            int score = 0;

            foreach (var q in test.Questions)
            {
                if (!dto.Answers.TryGetValue(q.Id, out var selectedOptionId))
                    continue;

                var correct = q.Options.FirstOrDefault(o => o.IsCorrect);
                if (correct != null && correct.Id == selectedOptionId)
                    score += q.Points;
            }

            _businessContext.TestResults.Add(new TestResult
            {
                StudentId = student.Id,
                TestId = test.Id,
                Score = score,
                CompletedDate = DateTime.UtcNow
            });

            await _businessContext.SaveChangesAsync();
        }
    }
}
