using TestSystem.Entities.DTOs.User;

namespace TestSystem.ServiceLayer.Interfaces
{
    public interface IUserManagementService
    {
        Task<List<UserAdminDto>> GetAllAsync();
        Task SetRoleAsync(string userId, string role, bool enabled);
    }
}
