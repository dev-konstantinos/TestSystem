using System.ComponentModel.DataAnnotations;
using TestSystem.Entities.DTOs.Teacher;

namespace TestSystem.Entities.DTOs.Teacher
{
    public class TeacherFormModel
    {
        public int? Id { get; set; }

        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        public TeacherCreateDto ToCreateDto() => new()
        {
            FirstName = FirstName,
            LastName = LastName,
            Email = Email
        };

        public TeacherUpdateDto ToUpdateDto() => new()
        {
            Id = Id!.Value,
            FirstName = FirstName,
            LastName = LastName,
            Email = Email
        };
    }
}
