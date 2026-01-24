namespace TestSystem.Entities.DTOs.Student
{
    public class StudentResultDto
    {
        public string TestTitle { get; set; } = null!;
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public DateTime CompletedDate { get; set; }
    }
}
