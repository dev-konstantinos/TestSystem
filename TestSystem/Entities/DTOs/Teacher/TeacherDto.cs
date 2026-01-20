namespace TestSystem.Entities.DTOs.Teacher
{
    public class TeacherDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTime JoinedDate { get; set; }
        public int StudentsCount { get; set; }
    }
}
