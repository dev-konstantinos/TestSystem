using TestSystem.Entities.DTOs.Student;

namespace TestSystem.ServiceLayer.Interfaces
{
    public interface IStudentTestPassingService
    {
        Task<StudentTestDto> GetTestAsync(string studentUserId, int testId);
        Task SubmitAsync(string studentUserId, StudentTestSubmitDto dto);
    }
}
