namespace TestSystem.Entities.DTOs.Student
{
    public class StudentQuestionDto
    {
        public int QuestionId { get; set; }
        public string Text { get; set; } = null!;
        public int Points { get; set; }
        public List<StudentOptionDto> Options { get; set; } = new();
    }
}
