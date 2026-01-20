using TestSystem.Entities;

namespace TestSystem.RepoLayer.Interfaces
{
    public interface ITeacherRepository
    {
        Task<Teacher?> GetByIdAsync(int id);
        Task<List<Teacher>> GetAllAsync();
        Task<Teacher> AddAsync(Teacher teacher);
        Task UpdateAsync(Teacher teacher);
        Task DeleteAsync(Teacher teacher);
    }
}
