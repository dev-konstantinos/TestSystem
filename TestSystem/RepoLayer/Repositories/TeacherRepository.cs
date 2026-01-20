using Microsoft.EntityFrameworkCore;
using TestSystem.Entities;
using TestSystem.MainContext;
using TestSystem.RepoLayer.Interfaces;

namespace TestSystem.RepoLayer.Repositories
{
    public class TeacherRepository : ITeacherRepository
    {
        private readonly BusinessDbContext _context;

        public TeacherRepository(BusinessDbContext context)
        {
            _context = context;
        }

        public Task<Teacher?> GetByIdAsync(int id)
        {
            return _context.Teachers
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public Task<List<Teacher>> GetAllAsync()
        {
            return _context.Teachers.AsNoTracking().ToListAsync();
        }

        public async Task<Teacher> AddAsync(Teacher teacher)
        {
            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();
            return teacher;
        }

        public async Task UpdateAsync(Teacher teacher)
        {
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Teacher teacher)
        {
            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();
        }
    }
}
