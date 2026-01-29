using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Tests.Infrastructure;
using TestSystem.Data;
using TestSystem.MainContext;
using TestSystem.ServiceLayer.Services.Admin;
using StudentEntity = TestSystem.Entities.Student;

namespace Tests.TestCases.ServiceLayer.Admin
{
    public class UserManagementServiceTests
    {
        [Fact]
        public async Task DeleteUser_ShouldThrow_WhenDeletingSelf()
        {
            var userId = "admin-id";

            var user = new ApplicationUser { Id = userId };

            var userManager = UserManagerMock.Create();
            userManager.Setup(m => m.FindByIdAsync(userId))
                .ReturnsAsync(user);

            var options = new DbContextOptionsBuilder<BusinessDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var factory = new TestDbContextFactory(options);
            var http = HttpContextMock.Create(userId);

            var service = new UserManagementService(
                userManager.Object,
                factory,
                http);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.DeleteUserAsync(userId));
        }

        [Fact]
        public async Task DeleteUser_ShouldThrow_WhenUserIsStudent()
        {
            var userId = "user-1";

            var user = new ApplicationUser { Id = userId };

            var userManager = UserManagerMock.Create();
            userManager.Setup(m => m.FindByIdAsync(userId))
                .ReturnsAsync(user);

            var options = new DbContextOptionsBuilder<BusinessDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using (var ctx = new BusinessDbContext(options))
            {
                ctx.Students.Add(new StudentEntity { UserId = userId });
                await ctx.SaveChangesAsync();
            }

            var factory = new TestDbContextFactory(options);
            var http = HttpContextMock.Create("admin-id");

            var service = new UserManagementService(
                userManager.Object,
                factory,
                http);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.DeleteUserAsync(userId));
        }

        [Fact]
        public async Task DeleteUser_ShouldDelete_WhenNoBusinessRoles()
        {
            var userId = "user-2";
            var user = new ApplicationUser { Id = userId };

            var userManager = UserManagerMock.Create();

            userManager.Setup(m => m.FindByIdAsync(userId))
                .ReturnsAsync(user);

            userManager.Setup(m => m.DeleteAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            var options = new DbContextOptionsBuilder<BusinessDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var factory = new TestDbContextFactory(options);
            var http = HttpContextMock.Create("admin-id");

            var service = new UserManagementService(
                userManager.Object,
                factory,
                http);

            await service.DeleteUserAsync(userId);

            userManager.Verify(m => m.DeleteAsync(user), Times.Once);
        }

        [Fact]
        public async Task DeleteUser_ShouldThrow_WhenUserNotFound()
        {
            var userId = "missing-user";

            var userManager = UserManagerMock.Create();

            userManager.Setup(m => m.FindByIdAsync(userId))
                .ReturnsAsync((ApplicationUser?)null);

            var options = new DbContextOptionsBuilder<BusinessDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var factory = new TestDbContextFactory(options);
            var http = HttpContextMock.Create("admin-id");

            var service = new UserManagementService(
                userManager.Object,
                factory,
                http);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.DeleteUserAsync(userId));

            Assert.Equal("User not found", ex.Message);
        }
    }
}
