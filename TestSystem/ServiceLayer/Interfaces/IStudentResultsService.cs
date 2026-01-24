using TestSystem.Entities.DTOs.Student;

namespace TestSystem.ServiceLayer.Interfaces
{
    public interface IStudentResultsService
    {
        Task<List<StudentResultDto>> GetMyResultsAsync(string studentUserId);
    }
}
