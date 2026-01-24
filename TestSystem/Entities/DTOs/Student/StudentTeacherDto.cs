namespace TestSystem.Entities.DTOs.Student
{
    public class StudentTeacherDto
    {
        public int TeacherId { get; set; }
        public string UserId { get; set; } = null!;

        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;

        public DateTime JoinedDate { get; set; }
        public int TestsCount { get; set; }
    }
}
