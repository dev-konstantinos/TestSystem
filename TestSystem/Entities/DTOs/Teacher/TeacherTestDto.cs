namespace TestSystem.Entities.DTOs.Teacher
{
    public class TeacherTestDto
    {
        public int TestId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public int QuestionsCount { get; set; }
        public int MaxScore { get; set; }
    }
}
