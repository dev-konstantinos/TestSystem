using System.ComponentModel.DataAnnotations;

namespace TestSystem.Entities.DTOs.Teacher
{
    public class TeacherCreateDto
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = null!;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = null!;

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = null!;
    }
}
