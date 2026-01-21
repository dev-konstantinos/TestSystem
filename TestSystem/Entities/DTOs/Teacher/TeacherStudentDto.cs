namespace TestSystem.Entities.DTOs.Teacher
{
    public class TeacherStudentDto
    {
        public int StudentId { get; set; }
        public string UserId { get; set; } = null!;

        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;

        public DateTime EnrolledDate { get; set; }

        public int TestsPassed { get; set; }
        public double? AverageScore { get; set; }
    }
}
