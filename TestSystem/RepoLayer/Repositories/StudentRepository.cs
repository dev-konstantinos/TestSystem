using Microsoft.EntityFrameworkCore;
using TestSystem.Data;
using TestSystem.Entities;
using TestSystem.Entities.DTOs.Student;
using TestSystem.MainContext;
using TestSystem.RepoLayer.Interfaces;

namespace TestSystem.RepoLayer.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly BusinessDbContext _businessContext;

        public StudentRepository(BusinessDbContext db)
        {
            _businessContext = db;
        }

        public IQueryable<Student> Query()
            => _businessContext.Students.AsNoTracking();
    }
}
