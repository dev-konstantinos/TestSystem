using Microsoft.EntityFrameworkCore;
using TestSystem.Entities;
using TestSystem.Entities.DTOs.Teacher;
using TestSystem.MainContext;
using TestSystem.ServiceLayer.Interfaces.Teacher;

namespace TestSystem.ServiceLayer.Services.Teacher
{
    // Class provides services related to retrieving tests assigned to a specific teacher
    public class TeacherTestsService : ITeacherTestsService
    {
        private readonly IDbContextFactory<BusinessDbContext> _dbFactory;

        public TeacherTestsService(IDbContextFactory<BusinessDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        // Retrieves all tests associated with a specific teacher
        public async Task<List<TeacherTestDto>> GetMyTestsAsync(string teacherUserId)
        {
            await using var _businessContext = await _dbFactory.CreateDbContextAsync();

            var teacherId = await _businessContext.Teachers
                .Where(t => t.UserId == teacherUserId)
                .Select(t => t.Id)
                .FirstOrDefaultAsync();

            if (teacherId == 0)
                return new();

            return await _businessContext.Tests
                .Where(t => t.Teachers.Any(x => x.Id == teacherId))
                .Select(t => new TeacherTestDto
                {
                    TestId = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    CreatedDate = t.CreatedDate,
                    QuestionsCount = t.Questions.Count,
                    MaxScore = t.MaxScore
                })
                .AsNoTracking()
                .ToListAsync();
        }

        // Adds a new test and associates it with the specified teacher
        public async Task AddTestAsync(string teacherUserId, string title, string? description)
        {
            await using var _businessContext = await _dbFactory.CreateDbContextAsync();

            var teacher = await _businessContext.Teachers
                .FirstOrDefaultAsync(t => t.UserId == teacherUserId)
                ?? throw new InvalidOperationException("Teacher not found");

            var test = new Test
            {
                Title = title,
                Description = description ?? "",
                MaxScore = 0
            };

            test.Teachers.Add(teacher);

            _businessContext.Tests.Add(test);
            await _businessContext.SaveChangesAsync();
        }

        // Deletes a test if the specified teacher is associated with it
        public async Task DeleteTestAsync(string teacherUserId, int testId)
        {
            await using var _businessContext = await _dbFactory.CreateDbContextAsync();

            var test = await _businessContext.Tests
                .Include(t => t.Teachers)
                .FirstOrDefaultAsync(t => t.Id == testId)
                ?? throw new InvalidOperationException("Test not found");

            if (!test.Teachers.Any(t => t.UserId == teacherUserId))
                throw new InvalidOperationException("Access denied");

            _businessContext.Tests.Remove(test);
            await _businessContext.SaveChangesAsync();
        }
    }
}
