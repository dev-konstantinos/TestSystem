namespace TestSystem.Entities.DTOs.Admin
{
    public class UserAdminDto
    {
        public string UserId { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;

        public bool IsAdmin { get; set; }
        public bool IsStudent { get; set; }
        public bool IsTeacher { get; set; }

        public bool CanBeDeleted => !IsStudent && !IsTeacher && !IsAdmin;
    }
}
