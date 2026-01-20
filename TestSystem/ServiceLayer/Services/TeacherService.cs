using TestSystem.Entities.DTOs.Teacher;
using TestSystem.Entities.Interfaces;
using TestSystem.RepoLayer.Interfaces;

namespace TestSystem.Entities.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly ITeacherRepository _repository;

        public TeacherService(ITeacherRepository repository)
        {
            _repository = repository;
        }
    }
}
