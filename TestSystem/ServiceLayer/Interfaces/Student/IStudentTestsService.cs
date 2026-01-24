using TestSystem.Entities.DTOs.Student;

namespace TestSystem.ServiceLayer.Interfaces.Student
{
    public interface IStudentTestsService
    {
        Task<List<StudentAvailableTestDto>> GetAvailableTestsAsync(string studentUserId);
    }
}
