namespace TestSystem.Entities.DTOs.Student
{
    public class StudentAvailableTestDto
    {
        public int TestId { get; set; }

        public string Title { get; set; } = null!;
        public string TeacherName { get; set; } = null!;

        public int QuestionsCount { get; set; }
        public int MaxScore { get; set; }
    }
}
