using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestSystem.Entities
{
    public class Option
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(300)]
        public string Text { get; set; } = null!;

        public bool IsCorrect { get; set; } = false;

        // One-to-Many
        [ForeignKey("QuestionId")]
        public int QuestionId { get; set; }
        public Question Question { get; set; } = null!;
    }
}
