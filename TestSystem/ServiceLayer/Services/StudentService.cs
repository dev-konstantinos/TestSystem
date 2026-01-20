using TestSystem.Entities.Interfaces;
using TestSystem.RepoLayer.Interfaces;

namespace TestSystem.ServiceLayer.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _repository;
        public StudentService(IStudentRepository repository)
        {
            _repository = repository;
        }
    }
}
