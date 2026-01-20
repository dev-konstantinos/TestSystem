using System.ComponentModel.DataAnnotations;

namespace TestSystem.Entities
{
    public class Teacher
    {
        [Key]
        public int Id { get; set; }

        // Connected to IdentityUser
        public string UserId { get; set; } = null!;

        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;

        // Many-to-Many
        public ICollection<Student> Students { get; set; } = new List<Student>();

        // Many-to-Many
        public ICollection<Test> Tests { get; set; } = new List<Test>();
    }
}
