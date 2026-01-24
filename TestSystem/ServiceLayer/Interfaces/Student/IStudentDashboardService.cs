using TestSystem.Entities.DTOs.Student;

namespace TestSystem.ServiceLayer.Interfaces.Student
{
    public interface IStudentDashboardService
    {
        Task<StudentDashboardDto?> GetDashboardAsync(string studentUserId);
    }
}
