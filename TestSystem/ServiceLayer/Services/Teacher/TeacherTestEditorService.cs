using Microsoft.EntityFrameworkCore;
using TestSystem.Entities;
using TestSystem.Entities.DTOs.Teacher;
using TestSystem.MainContext;
using TestSystem.ServiceLayer.Interfaces.Teacher;

namespace TestSystem.ServiceLayer.Services.Teacher
{
    // Service for editing tests by teachers
    public class TeacherTestEditorService : ITeacherTestEditorService
    {
        private readonly IDbContextFactory<BusinessDbContext> _dbFactory;

        public TeacherTestEditorService(IDbContextFactory<BusinessDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        // Get all questions for a test
        public async Task<List<TeacherQuestionDto>> GetQuestionsAsync(
            string teacherUserId,
            int testId)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            var test = await GetOwnedTest(db, teacherUserId, testId);

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

        // Add a question to a test
        public async Task AddQuestionAsync(
            string teacherUserId,
            int testId,
            string text,
            int points)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            var test = await GetOwnedTest(db, teacherUserId, testId);

            test.Questions.Add(new Question
            {
                Text = text,
                Points = points
            });

            await db.SaveChangesAsync();
            await RecalculateMaxScoreAsync(db, test.Id);
            await db.SaveChangesAsync();
        }

        // Delete a question
        public async Task DeleteQuestionAsync(
            string teacherUserId,
            int questionId)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            var question = await db.Questions
                .Include(q => q.Test)
                    .ThenInclude(t => t.Teachers)
                .FirstOrDefaultAsync(q => q.Id == questionId)
                ?? throw new InvalidOperationException("Question not found");

            if (!question.Test.Teachers.Any(t => t.UserId == teacherUserId))
                throw new InvalidOperationException("Access denied");

            var testId = question.TestId;

            db.Questions.Remove(question);

            await db.SaveChangesAsync();
            await RecalculateMaxScoreAsync(db, testId);
            await db.SaveChangesAsync();
        }

        // Add an option to a question
        public async Task AddOptionAsync(
            string teacherUserId,
            int questionId,
            string text,
            bool isCorrect)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            var question = await db.Questions
                .Include(q => q.Test)
                    .ThenInclude(t => t.Teachers)
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == questionId)
                ?? throw new InvalidOperationException("Question not found");

            if (!question.Test.Teachers.Any(t => t.UserId == teacherUserId))
                throw new InvalidOperationException("Access denied");

            question.Options.Add(new Option
            {
                Text = text,
                IsCorrect = isCorrect
            });

            await db.SaveChangesAsync();
        }

        // Delete an option
        public async Task DeleteOptionAsync(
            string teacherUserId,
            int optionId)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            var option = await db.Options
                .Include(o => o.Question)
                    .ThenInclude(q => q.Test)
                        .ThenInclude(t => t.Teachers)
                .FirstOrDefaultAsync(o => o.Id == optionId)
                ?? throw new InvalidOperationException("Option not found");

            if (!option.Question.Test.Teachers.Any(t => t.UserId == teacherUserId))
                throw new InvalidOperationException("Access denied");

            db.Options.Remove(option);
            await db.SaveChangesAsync();
        }

        // Helper to get a test owned by the teacher
        private async Task<Test> GetOwnedTest(
            BusinessDbContext db,
            string teacherUserId,
            int testId)
        {
            var test = await db.Tests
                .Include(t => t.Teachers)
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(t => t.Id == testId)
                ?? throw new InvalidOperationException("Test not found");

            if (!test.Teachers.Any(t => t.UserId == teacherUserId))
                throw new InvalidOperationException("Access denied");

            return test;
        }

        // Helper to recalculate the max score of a test
        private async Task RecalculateMaxScoreAsync(
            BusinessDbContext db,
            int testId)
        {
            var maxScore = await db.Questions
                .Where(q => q.TestId == testId)
                .SumAsync(q => q.Points);

            var test = await db.Tests.FirstAsync(t => t.Id == testId);
            test.MaxScore = maxScore;
        }
    }
}
