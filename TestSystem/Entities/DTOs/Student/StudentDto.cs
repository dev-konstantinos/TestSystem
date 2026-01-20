namespace TestSystem.Entities.DTOs.Student
{
    public class StudentDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTime EnrolledDate { get; set; }
        public int TestResultsCount { get; set; }
    }
}
