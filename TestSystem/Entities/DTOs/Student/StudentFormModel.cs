using System.ComponentModel.DataAnnotations;

namespace TestSystem.Entities.DTOs.Student
{
    public class StudentFormModel
    {
        public int? Id { get; set; }

        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = string.Empty;
        public StudentCreateDto ToCreateDto() => new()
        {
            FirstName = FirstName,
            LastName = LastName,
            Email = Email
        };
        public StudentUpdateDto ToUpdateDto() => new()
        {
            Id = Id!.Value,
            FirstName = FirstName,
            LastName = LastName,
            Email = Email
        };
    }
}
