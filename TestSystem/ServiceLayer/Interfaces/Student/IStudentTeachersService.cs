using TestSystem.Entities.DTOs.Student;

namespace TestSystem.ServiceLayer.Interfaces.Student
{
    public interface IStudentTeachersService
    {
        Task<List<StudentTeacherDto>> GetMyTeachersAsync(string studentUserId);
    }
}
