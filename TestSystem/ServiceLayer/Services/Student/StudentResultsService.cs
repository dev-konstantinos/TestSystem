using Microsoft.EntityFrameworkCore;
using TestSystem.Entities.DTOs.Student;
using TestSystem.MainContext;
using TestSystem.ServiceLayer.Interfaces.Student;

namespace TestSystem.ServiceLayer.Services.Student
{
    // Class to handle student results retrieval
    public class StudentResultsService : IStudentResultsService
    {
        private readonly IDbContextFactory<BusinessDbContext> _dbFactory;

        public StudentResultsService(IDbContextFactory<BusinessDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        // Method to get results for a specific student
        public async Task<List<StudentResultDto>> GetMyResultsAsync(string studentUserId)
        {
            await using var _businessContext = await _dbFactory.CreateDbContextAsync();

            return await _businessContext.TestResults
                .Where(r => r.Student.UserId == studentUserId)
                .OrderByDescending(r => r.CompletedDate)
                .Select(r => new StudentResultDto
                {
                    TestTitle = r.Test.Title,
                    Score = r.Score,
                    MaxScore = r.Test.Questions.Sum(q => q.Points),
                    CompletedDate = r.CompletedDate
                })
                .ToListAsync();
        }
    }
}
