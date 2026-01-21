using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TestSystem.Data;
using TestSystem.Entities.DTOs.Student;
using TestSystem.Entities.Interfaces;
using TestSystem.RepoLayer.Interfaces;

namespace TestSystem.ServiceLayer.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentService(IStudentRepository studentRepository, UserManager<ApplicationUser> userManager)
        {
            _studentRepository = studentRepository;
            _userManager = userManager;
        }

        public async Task<List<StudentDto>> GetAllAsync()
        {
            // 1. Business data
            var students = await _studentRepository.Query().ToListAsync();

            if (students.Count == 0)
                return new();

            // 2. Identity data
            var userIds = students.Select(s => s.UserId).Distinct().ToList();

            var users = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email
                })
                .ToListAsync();

            var userDict = users.ToDictionary(u => u.Id);

            // 3. DTO
            return students.Select(s =>
            {
                var u = userDict[s.UserId];

                return new StudentDto
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    EnrolledDate = s.EnrolledDate
                };
            }).ToList();
        }
    }
}
