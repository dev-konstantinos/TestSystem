namespace TestSystem.Entities.DTOs.Teacher
{
    public class TeacherQuestionDto
    {
        public int QuestionId { get; set; }
        public string Text { get; set; } = null!;
        public int Points { get; set; }
        public List<TeacherOptionDto> Options { get; set; } = new();
    }
}
