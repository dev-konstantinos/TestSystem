namespace TestSystem.Entities.DTOs.Teacher
{
    public class TeacherOptionDto
    {
        public int OptionId { get; set; }
        public string Text { get; set; } = null!;
        public bool IsCorrect { get; set; }
    }
}
