namespace TestSystem.Entities.DTOs.Student
{
    public class StudentDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTime EnrolledDate { get; set; }
        public string FullName => $"{FirstName} {LastName}";
    }
}
