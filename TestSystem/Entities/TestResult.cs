using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestSystem.Entities
{
    public class TestResult
    {
        [Key]
        public int Id { get; set; }

        public int Score { get; set; }

        public DateTime CompletedDate { get; set; } = DateTime.UtcNow;

        // One-to-Many
        [ForeignKey("StudentId")]
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        // One-to-Many
        [ForeignKey("TestId")]
        public int TestId { get; set; }
        public Test Test { get; set; } = null!;
    }
}
