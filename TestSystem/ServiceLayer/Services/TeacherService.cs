using TestSystem.Entities.DTOs.Teacher;
using TestSystem.Entities.Interfaces;

namespace TestSystem.Entities.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly ITeacherRepository _repository;

        public TeacherService(ITeacherRepository repository)
        {
            _repository = repository;
        }

        public async Task<TeacherDto?> GetByIdAsync(int id)
        {
            var teacher = await _repository.GetByIdAsync(id);
            if (teacher == null) return null;
            return MapToDto(teacher);
        }

        public async Task<IReadOnlyList<TeacherDto>> GetAllAsync()
        {
            var teachers = await _repository.GetAllAsync();
            return teachers.Select(MapToDto).ToList();
        }

        public async Task<TeacherDto> AddAsync(TeacherCreateDto dto)
        {
            var teacher = new Teacher
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                JoinedDate = DateTime.UtcNow
            };
            await _repository.AddAsync(teacher);
            return MapToDto(teacher);
        }

        public async Task UpdateAsync(TeacherUpdateDto dto)
        {
            var teacher = await _repository.GetByIdAsync(dto.Id);
            if (teacher == null)
                throw new KeyNotFoundException();
            teacher.FirstName = dto.FirstName;
            teacher.LastName = dto.LastName;
            teacher.Email = dto.Email;
            await _repository.UpdateAsync(teacher);
        }

        public async Task DeleteAsync(int id)
        {
            var teacher = await _repository.GetByIdAsync(id);
            if (teacher == null) return;
            await _repository.DeleteAsync(teacher);
        }

        private static TeacherDto MapToDto(Teacher t) =>
            new()
            {
                Id = t.Id,
                FullName = $"{t.FirstName} {t.LastName}",
                Email = t.Email,
                JoinedDate = t.JoinedDate,
                StudentsCount = t.Students?.Count ?? 0
            };
    }
}
