using TestSystem.Entities;

namespace TestSystem.RepoLayer.Interfaces
{
    public interface IStudentRepository
    {
        Task<Student?> GetByIdAsync(int id);
        Task<List<Student>> GetAllAsync();
        Task<Student> AddAsync(Student student);
        Task UpdateAsync(Student student);
        Task DeleteAsync(Student student);
    }
}
