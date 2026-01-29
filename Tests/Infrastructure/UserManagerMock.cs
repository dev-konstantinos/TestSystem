using Microsoft.AspNetCore.Identity;
using Moq;
using TestSystem.Data;

namespace Tests.Infrastructure
{
    // Class for creating a mock UserManager for testing of Identity-related functionality
    internal static class UserManagerMock
    {
        public static Mock<UserManager<ApplicationUser>> Create()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();

            return new Mock<UserManager<ApplicationUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!
            );
        }
    }
}
