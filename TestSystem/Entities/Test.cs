using System.ComponentModel.DataAnnotations;

namespace TestSystem.Entities
{
    public class Test
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = null!;

        [MaxLength(1000)]
        public string Description { get; set; } = null!;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int MaxScore { get; set; }

        // Many-to-Many
        public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();

        // One-to-Many
        public ICollection<Question> Questions { get; set; } = new List<Question>();

        // One-to-Many
        public ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
    }
}
