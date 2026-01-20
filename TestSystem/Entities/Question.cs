using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestSystem.Entities
{
    public class Question
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(500)]
        public string Text { get; set; } = null!;

        public int Points { get; set; } = 1;

        // One-to-Many
        [ForeignKey("TestId")]
        public int TestId { get; set; }
        public Test Test { get; set; } = null!;

        // One-to-Many
        public ICollection<Option> Options { get; set; } = new List<Option>();
    }
}
