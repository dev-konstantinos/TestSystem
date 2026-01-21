using TestSystem.Entities.DTOs.Student;

namespace TestSystem.Entities.Interfaces
{
    public interface IStudentService
    {
        Task<List<StudentDto>> GetAllAsync();
    }
}
