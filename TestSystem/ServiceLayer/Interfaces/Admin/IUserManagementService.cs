using TestSystem.Entities.DTOs.Admin;

namespace TestSystem.ServiceLayer.Interfaces.Admin
{
    public interface IUserManagementService
    {
        Task<List<UserAdminDto>> GetAllAsync();
        Task SetRoleAsync(string userId, string role, bool enabled);
    }
}
