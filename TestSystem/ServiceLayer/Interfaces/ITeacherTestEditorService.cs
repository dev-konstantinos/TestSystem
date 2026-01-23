using TestSystem.Entities.DTOs.Teacher;

namespace TestSystem.ServiceLayer.Interfaces
{
    public interface ITeacherTestEditorService
    {
        Task<List<TeacherQuestionDto>> GetQuestionsAsync(string teacherUserId, int testId);

        Task AddQuestionAsync(string teacherUserId, int testId, string text, int points);

        Task DeleteQuestionAsync(string teacherUserId, int questionId);

        Task AddOptionAsync(string teacherUserId, int questionId, string text, bool isCorrect);

        Task DeleteOptionAsync(string teacherUserId, int optionId);
    }
}
