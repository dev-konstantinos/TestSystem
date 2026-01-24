using TestSystem.Entities.DTOs.Student;

namespace TestSystem.ServiceLayer.Interfaces.Student
{
    public interface IStudentResultsService
    {
        Task<List<StudentResultDto>> GetMyResultsAsync(string studentUserId);
    }
}
