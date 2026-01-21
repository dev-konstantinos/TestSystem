using TestSystem.Entities;
using TestSystem.Entities.DTOs.Student;

namespace TestSystem.RepoLayer.Interfaces
{
    public interface IStudentRepository
    {
        IQueryable<Student> Query();
    }
}
