using TestSystem.Entities.DTOs.Teacher;

namespace TestSystem.ServiceLayer.Interfaces
{
    public interface ITeacherStudentsService
    {
        Task<List<TeacherStudentDto>> GetMyStudentsAsync(string teacherUserId);
    }
}
