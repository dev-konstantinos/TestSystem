using Microsoft.EntityFrameworkCore;
using TestSystem.Entities.DTOs.Teacher;
using TestSystem.MainContext;
using TestSystem.ServiceLayer.Interfaces.Teacher;

namespace TestSystem.ServiceLayer.Services.Teacher
{
    // Service to get teacher dashboard data
    public class TeacherDashboardService : ITeacherDashboardService
    {
        private readonly IDbContextFactory<BusinessDbContext> _dbFactory;

        public TeacherDashboardService(IDbContextFactory<BusinessDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        // Get dashboard data for a specific teacher by user ID
        public async Task<TeacherDashboardDto?> GetDashboardAsync(string userId)
        {
            await using var _businessContext = await _dbFactory.CreateDbContextAsync();

            var teacher = await _businessContext.Teachers
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (teacher == null)
                return null;

            var studentsCount = await _businessContext.Teachers
                .Where(t => t.Id == teacher.Id)
                .SelectMany(t => t.Students)
                .CountAsync();

            var testsCount = await _businessContext.Teachers
                .Where(t => t.Id == teacher.Id)
                .SelectMany(t => t.Tests)
                .CountAsync();

            var resultsCount = await _businessContext.TestResults
                .CountAsync(r =>
                    r.Test.Teachers.Any(t => t.Id == teacher.Id));

            return new TeacherDashboardDto
            {
                TeacherId = teacher.Id,
                StudentsCount = studentsCount,
                TestsCount = testsCount,
                ResultsCount = resultsCount
            };
        }
    }
}
