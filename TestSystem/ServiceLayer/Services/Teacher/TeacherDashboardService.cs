using Microsoft.EntityFrameworkCore;
using TestSystem.Entities.DTOs.Teacher;
using TestSystem.MainContext;
using TestSystem.ServiceLayer.Interfaces.Teacher;

namespace TestSystem.ServiceLayer.Services.Teacher
{
    // Service to get teacher dashboard data
    public class TeacherDashboardService : ITeacherDashboardService
    {
        private readonly BusinessDbContext _businessContext;

        public TeacherDashboardService(BusinessDbContext db)
        {
            _businessContext = db;
        }

        // Get dashboard data for a specific teacher by user ID
        public async Task<TeacherDashboardDto?> GetDashboardAsync(string userId)
        {
            var teacher = await _businessContext.Teachers
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.UserId == userId); // System.InvalidOperationException: 'A second operation was started on this context instance before a previous operation completed. This is usually caused by different threads concurrently using the same instance of DbContext. For more information on how to avoid threading issues with DbContext, see https://go.microsoft.com/fwlink/?linkid=2097913.'

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
