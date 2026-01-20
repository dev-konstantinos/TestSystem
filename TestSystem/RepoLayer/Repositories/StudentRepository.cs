using Microsoft.EntityFrameworkCore;
using TestSystem.Entities;
using TestSystem.MainContext;
using TestSystem.RepoLayer.Interfaces;

namespace TestSystem.RepoLayer.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly BusinessDbContext _context;

        public StudentRepository(BusinessDbContext context)
        {
            _context = context;
        }

        public Task<Student?> GetByIdAsync(int id)
        {
            return _context.Students
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public Task<List<Student>> GetAllAsync()
        {
            return _context.Students.AsNoTracking().ToListAsync();
        }

        public async Task<Student> AddAsync(Student student)
        {
            _context.Students.Add(student);
            await _context.SaveChangesAsync();
            return student;
        }

        public async Task UpdateAsync(Student student)
        {
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Student student)
        {
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
        }
    }
}
