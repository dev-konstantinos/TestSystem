using TestSystem.Entities.DTOs.Teacher;

namespace TestSystem.ServiceLayer.Interfaces.Teacher
{
    public interface ITeacherTestsService
    {
        Task<List<TeacherTestDto>> GetMyTestsAsync(string teacherUserId);
        Task AddTestAsync(string teacherUserId, string title, string? description);
        Task DeleteTestAsync(string teacherUserId, int testId);
    }
}
