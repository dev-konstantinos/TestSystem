using TestSystem.Entities.DTOs.Teacher;

namespace TestSystem.ServiceLayer.Interfaces.Teacher
{
    public interface ITeacherStudentsService
    {
        Task<List<TeacherStudentDto>> GetMyStudentsAsync(string teacherUserId);
        Task<List<TeacherStudentDto>> GetAvailableStudentsAsync(string teacherUserId);
        Task AttachStudentAsync(string teacherUserId, int studentId);
        Task DetachStudentAsync(string teacherUserId, int studentId);
    }
}
