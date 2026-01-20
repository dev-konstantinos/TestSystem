using System.ComponentModel.DataAnnotations;

namespace TestSystem.Entities
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        // connected to Identity User
        public string UserId { get; set; } = null!;

        public DateTime EnrolledDate { get; set; } = DateTime.UtcNow;

        // Many-to-Many
        public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();

        // One-to-Many
        public ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
    }
}
