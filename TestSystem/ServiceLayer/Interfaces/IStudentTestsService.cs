using TestSystem.Entities.DTOs.Student;

namespace TestSystem.ServiceLayer.Interfaces
{
    public interface IStudentTestsService
    {
        Task<List<StudentAvailableTestDto>> GetAvailableTestsAsync(string studentUserId);
    }
}
