using TestSystem.Entities.DTOs.Student;

namespace TestSystem.ServiceLayer.Interfaces
{
    public interface IStudentDashboardService
    {
        Task<StudentDashboardDto?> GetDashboardAsync(string studentUserId);
    }
}
