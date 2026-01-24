using TestSystem.Entities.DTOs.Teacher;

namespace TestSystem.ServiceLayer.Interfaces.Teacher
{
    public interface ITeacherDashboardService
    {
        Task<TeacherDashboardDto?> GetDashboardAsync(string userId); 
    }
}
