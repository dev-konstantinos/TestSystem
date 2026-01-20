using TestSystem.Entities;
using TestSystem.Entities.DTOs.Student;
using TestSystem.Entities.Interfaces;

namespace TestSystem.ServiceLayer.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _repository;
        public StudentService(IStudentRepository repository)
        {
            _repository = repository;
        }

        public async Task<StudentDto> AddAsync(StudentCreateDto dto)
        {
            var student = new Student
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                EnrolledDate = DateTime.UtcNow
            };
            await _repository.AddAsync(student);
            return MapToDto(student);
        }

        public async Task DeleteAsync(int id)
        {
            var student = await _repository.GetByIdAsync(id);
            if (student == null) return;
            await _repository.DeleteAsync(student);
        }

        public async Task<IReadOnlyList<StudentDto>> GetAllAsync()
        {
            var students = await _repository.GetAllAsync();
            return students.Select(MapToDto).ToList();
        }

        public async Task<StudentDto?> GetByIdAsync(int id)
        {
            var student = await _repository.GetByIdAsync(id);
            if (student == null) return null;
            return MapToDto(student);
        }

        public async Task UpdateAsync(StudentUpdateDto dto)
        {
            var student = await _repository.GetByIdAsync(dto.Id);
            if (student == null)
                throw new KeyNotFoundException();
            student.FirstName = dto.FirstName;
            student.LastName = dto.LastName;
            student.Email = dto.Email;
            await _repository.UpdateAsync(student);
        }

        private static StudentDto MapToDto(Student s) =>
            new()
            {
                Id = s.Id,
                FullName = $"{s.FirstName} {s.LastName}",
                Email = s.Email,
                EnrolledDate = s.EnrolledDate,
                TestResultsCount = s.TestResults?.Count ?? 0
            };
    }
}
