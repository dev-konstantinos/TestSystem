using TestSystem.Entities.DTOs.Student;

namespace TestSystem.Entities.Interfaces
{
    public interface IStudentService
    {
        Task<StudentDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<StudentDto>> GetAllAsync();
        Task<StudentDto> AddAsync(StudentCreateDto dto);
        Task UpdateAsync(StudentUpdateDto dto);
        Task DeleteAsync(int id);
    }
}
