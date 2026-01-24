using TestSystem.Entities.DTOs.Student;

namespace TestSystem.ServiceLayer.Interfaces
{
    public interface IStudentTeachersService
    {
        Task<List<StudentTeacherDto>> GetMyTeachersAsync(string studentUserId);
    }
}
