using TestSystem.Entities.DTOs.Teacher;

namespace TestSystem.ServiceLayer.Interfaces
{
    public interface ITeacherDashboardService
    {
        Task<TeacherDashboardDto?> GetDashboardAsync(string userId); 
    }
}
