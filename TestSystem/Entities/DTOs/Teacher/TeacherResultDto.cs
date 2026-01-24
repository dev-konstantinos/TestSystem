namespace TestSystem.Entities.DTOs.Teacher
{
    public class TeacherResultDto
    {
        public int TestId { get; set; }
        public string TestTitle { get; set; } = null!;

        public int StudentId { get; set; }
        public string StudentName { get; set; } = null!;
        public string StudentEmail { get; set; } = null!;

        public int Score { get; set; }
        public int MaxScore { get; set; }
        public DateTime CompletedDate { get; set; }
    }
}
