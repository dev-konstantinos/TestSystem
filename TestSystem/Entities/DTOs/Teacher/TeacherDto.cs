namespace TestSystem.Entities.DTOs.Teacher
{
    public class TeacherDto
    {
        public int TeacherId { get; set; }
        public string UserId { get; set; } = null!;

        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;

        public DateTime JoinedDate { get; set; }
        public string FullName => $"{FirstName} {LastName}";
    }
}
