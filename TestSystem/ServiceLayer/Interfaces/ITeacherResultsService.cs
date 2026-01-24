using TestSystem.Entities.DTOs.Teacher;

namespace TestSystem.ServiceLayer.Interfaces
{
    public interface ITeacherResultsService
    {
        Task<List<TeacherResultDto>> GetMyResultsAsync(string teacherUserId);
    }
}
