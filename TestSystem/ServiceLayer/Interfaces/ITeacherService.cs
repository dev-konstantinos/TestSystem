using TestSystem.Entities.DTOs.Teacher;

namespace TestSystem.Entities.Interfaces
{
    public interface ITeacherService
    {
        Task<TeacherDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<TeacherDto>> GetAllAsync();
        Task<TeacherDto> AddAsync(TeacherCreateDto dto);
        Task UpdateAsync(TeacherUpdateDto dto);
        Task DeleteAsync(int id);
    }
}
