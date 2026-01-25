using Microsoft.EntityFrameworkCore;
using TestSystem.Entities;
using TestSystem.Entities.DTOs.Teacher;
using TestSystem.MainContext;
using TestSystem.ServiceLayer.Interfaces.Teacher;

namespace TestSystem.ServiceLayer.Services.Teacher
{
    // Service class for teachers to edit tests, questions, and options
    public class TeacherTestEditorService : ITeacherTestEditorService
    {
        private readonly IDbContextFactory<BusinessDbContext> _dbFactory;

        public TeacherTestEditorService(IDbContextFactory<BusinessDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        // Helper method to get a test owned by the teacher
        private async Task<Test> GetOwnedTest(string teacherUserId, int testId)
        {
            await using var _businessContext = await _dbFactory.CreateDbContextAsync();

            var test = await _businessContext.Tests
                .Include(t => t.Teachers)
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(t => t.Id == testId)
                ?? throw new InvalidOperationException("Test not found");

            if (!test.Teachers.Any(t => t.UserId == teacherUserId))
                throw new InvalidOperationException("Access denied");

            return test;
        }

        // Get all questions for a test owned by the teacher
        public async Task<List<TeacherQuestionDto>> GetQuestionsAsync(string teacherUserId, int testId)
        {
            await using var _businessContext = await _dbFactory.CreateDbContextAsync();

            var test = await GetOwnedTest(teacherUserId, testId);

            return test.Questions.Select(q => new TeacherQuestionDto
            {
                QuestionId = q.Id,
                Text = q.Text,
                Points = q.Points,
                Options = q.Options.Select(o => new TeacherOptionDto
                {
                    OptionId = o.Id,
                    Text = o.Text,
                    IsCorrect = o.IsCorrect
                }).ToList()
            }).ToList();
        }

        // Add a new question to a test owned by the teacher
        public async Task AddQuestionAsync(string teacherUserId, int testId, string text, int points)
        {
            await using var _businessContext = await _dbFactory.CreateDbContextAsync();

            var test = await GetOwnedTest(teacherUserId, testId);

            test.Questions.Add(new Question
            {
                Text = text,
                Points = points
            });

            await _businessContext.SaveChangesAsync();
            await RecalculateMaxScoreAsync(test.Id);
        }

        // Delete a question from a test owned by the teacher
        public async Task DeleteQuestionAsync(string teacherUserId, int questionId)
        {
            await using var _businessContext = await _dbFactory.CreateDbContextAsync();

            var question = await _businessContext.Questions
                .Include(q => q.Test)
                    .ThenInclude(t => t.Teachers)
                .FirstOrDefaultAsync(q => q.Id == questionId)
                ?? throw new InvalidOperationException("Question not found");

            if (!question.Test.Teachers.Any(t => t.UserId == teacherUserId))
                throw new InvalidOperationException("Access denied");

            var testId = question.TestId;

            _businessContext.Questions.Remove(question);

            await _businessContext.SaveChangesAsync();
            await RecalculateMaxScoreAsync(testId);
        }

        // Add a new option to a question owned by the teacher
        public async Task AddOptionAsync(string teacherUserId, int questionId, string text, bool isCorrect)
        {
            await using var _businessContext = await _dbFactory.CreateDbContextAsync();

            var question = await _businessContext.Questions
                .Include(q => q.Test)
                    .ThenInclude(t => t.Teachers)
                .FirstOrDefaultAsync(q => q.Id == questionId)
                ?? throw new InvalidOperationException("Question not found");

            if (!question.Test.Teachers.Any(t => t.UserId == teacherUserId))
                throw new InvalidOperationException("Access denied");

            question.Options.Add(new Option
            {
                Text = text,
                IsCorrect = isCorrect
            });

            await _businessContext.SaveChangesAsync();
        }

        // Delete an option from a question owned by the teacher
        public async Task DeleteOptionAsync(string teacherUserId, int optionId)
        {
            await using var _businessContext = await _dbFactory.CreateDbContextAsync();

            var option = await _businessContext.Options
                .Include(o => o.Question)
                    .ThenInclude(q => q.Test)
                        .ThenInclude(t => t.Teachers)
                .FirstOrDefaultAsync(o => o.Id == optionId)
                ?? throw new InvalidOperationException("Option not found");

            if (!option.Question.Test.Teachers.Any(t => t.UserId == teacherUserId))
                throw new InvalidOperationException("Access denied");

            _businessContext.Options.Remove(option);
            await _businessContext.SaveChangesAsync();
        }

        // Recalculate the maximum score for a test based on its questions
        private async Task RecalculateMaxScoreAsync(int testId)
        {
            await using var _businessContext = await _dbFactory.CreateDbContextAsync();

            var maxScore = await _businessContext.Questions
                .Where(q => q.TestId == testId)
                .SumAsync(q => q.Points);

            var test = await _businessContext.Tests
                .FirstAsync(t => t.Id == testId);

            test.MaxScore = maxScore;

            await _businessContext.SaveChangesAsync();
        }
    }
}
