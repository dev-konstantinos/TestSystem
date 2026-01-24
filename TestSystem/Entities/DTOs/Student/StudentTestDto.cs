namespace TestSystem.Entities.DTOs.Student
{
    public class StudentTestDto
    {
        public int TestId { get; set; }
        public string Title { get; set; } = null!;
        public List<StudentQuestionDto> Questions { get; set; } = new();
    }
}
