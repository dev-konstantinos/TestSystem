using Microsoft.EntityFrameworkCore;
using TestSystem.Entities.DTOs.Student;
using TestSystem.MainContext;
using TestSystem.ServiceLayer.Interfaces.Student;

namespace TestSystem.ServiceLayer.Services.Student
{
    // Class to manage student dashboard data retrieval
    public class StudentDashboardService : IStudentDashboardService
    {
        private readonly BusinessDbContext _businessContext;

        public StudentDashboardService(BusinessDbContext db)
        {
            _businessContext = db;
        }

        // method to get dashboard data for a student
        public async Task<StudentDashboardDto?> GetDashboardAsync(string studentUserId)
        {
            var studentId = await _businessContext.Students
                .Where(s => s.UserId == studentUserId)
                .Select(s => s.Id)
                .FirstOrDefaultAsync();

            if (studentId == 0)
                return null;

            var teachersCount = await _businessContext.Students
                .Where(s => s.Id == studentId)
                .SelectMany(s => s.Teachers)
                .CountAsync();

            var availableTestsCount = await _businessContext.Students
                .Where(s => s.Id == studentId)
                .SelectMany(s => s.Teachers)
                .SelectMany(t => t.Tests)
                .Distinct()
                .CountAsync();

            var resultsCount = await _businessContext.TestResults
                .CountAsync(r => r.StudentId == studentId);

            return new StudentDashboardDto
            {
                TeachersCount = teachersCount,
                AvailableTestsCount = availableTestsCount,
                ResultsCount = resultsCount
            };
        }
    }
}
