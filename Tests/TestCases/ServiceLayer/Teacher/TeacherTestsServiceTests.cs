using Microsoft.EntityFrameworkCore;
using TestSystem.Entities;
using TestSystem.MainContext;
using TestSystem.ServiceLayer.Services.Teacher;
using Tests.Infrastructure;

using TeacherEntity = TestSystem.Entities.Teacher;

namespace Tests.TestCases.ServiceLayer.Teacher
{
    public class TeacherTestsServiceTests
    {
        private readonly DbContextOptions<BusinessDbContext> _options;
        private readonly TeacherTestsService _service;

        public TeacherTestsServiceTests()
        {
            _options = new DbContextOptionsBuilder<BusinessDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var factory = new TestDbContextFactory(_options);
            _service = new TeacherTestsService(factory);
        }

        [Fact]
        public async Task GetMyTestsAsync_ShouldReturnEmpty_WhenTeacherNotExists()
        {
            var result = await _service.GetMyTestsAsync("missing-teacher");

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMyTestsAsync_ShouldReturnEmpty_WhenTeacherHasNoTests()
        {
            var teacherUserId = "teacher-1";

            await using (var ctx = new BusinessDbContext(_options))
            {
                ctx.Teachers.Add(new TeacherEntity { UserId = teacherUserId });
                await ctx.SaveChangesAsync();
            }

            var result = await _service.GetMyTestsAsync(teacherUserId);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMyTestsAsync_ShouldReturnOnlyTeachersTests()
        {
            var teacherUserId = "teacher-2";

            await using (var ctx = new BusinessDbContext(_options))
            {
                var teacher = new TeacherEntity { UserId = teacherUserId };
                var otherTeacher = new TeacherEntity { UserId = "other-teacher" };

                var myTest = new Test
                {
                    Title = "My Test",
                    Description = "Desc",
                    MaxScore = 5,
                    Questions = { new Question { Text = "Q", Points = 5 } }
                };

                var otherTest = new Test
                {
                    Title = "Other Test",
                    Description = "Desc",
                    MaxScore = 1,
                    Questions = { new Question { Text = "Q", Points = 1 } }
                };

                teacher.Tests.Add(myTest);
                otherTeacher.Tests.Add(otherTest);

                ctx.AddRange(teacher, otherTeacher);
                await ctx.SaveChangesAsync();
            }

            var result = await _service.GetMyTestsAsync(teacherUserId);

            Assert.Single(result);

            var test = result[0];
            Assert.Equal("My Test", test.Title);
            Assert.Equal(1, test.QuestionsCount);
            Assert.Equal(5, test.MaxScore);
        }

        [Fact]
        public async Task AddTestAsync_ShouldThrow_WhenTeacherNotFound()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AddTestAsync("missing-teacher", "Test", "Desc"));
        }

        [Fact]
        public async Task AddTestAsync_ShouldCreateTest_AndAttachToTeacher()
        {
            var teacherUserId = "teacher-3";

            await using (var ctx = new BusinessDbContext(_options))
            {
                ctx.Teachers.Add(new TeacherEntity { UserId = teacherUserId });
                await ctx.SaveChangesAsync();
            }

            await _service.AddTestAsync(teacherUserId, "New Test", "Desc");

            await using (var ctx = new BusinessDbContext(_options))
            {
                var test = await ctx.Tests
                    .Include(t => t.Teachers)
                    .FirstAsync();

                Assert.Equal("New Test", test.Title);
                Assert.Equal(0, test.MaxScore);
                Assert.Single(test.Teachers);
                Assert.Equal(teacherUserId, test.Teachers.First().UserId);
            }
        }

        [Fact]
        public async Task DeleteTestAsync_ShouldThrow_WhenTestNotFound()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.DeleteTestAsync("teacher-4", 999));
        }

        [Fact]
        public async Task DeleteTestAsync_ShouldThrow_WhenAccessDenied()
        {
            int testId;

            await using (var ctx = new BusinessDbContext(_options))
            {
                var owner = new TeacherEntity { UserId = "owner" };
                var test = new Test { Title = "Test", Description = "Desc" };

                owner.Tests.Add(test);
                ctx.Add(owner);
                await ctx.SaveChangesAsync();

                testId = test.Id;
            }

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.DeleteTestAsync("other-teacher", testId));
        }

        [Fact]
        public async Task DeleteTestAsync_ShouldDelete_WhenOwnedByTeacher()
        {
            var teacherUserId = "teacher-5";
            int testId;

            await using (var ctx = new BusinessDbContext(_options))
            {
                var teacher = new TeacherEntity { UserId = teacherUserId };
                var test = new Test { Title = "Test", Description = "Desc" };

                teacher.Tests.Add(test);
                ctx.Add(teacher);
                await ctx.SaveChangesAsync();

                testId = test.Id;
            }

            await _service.DeleteTestAsync(teacherUserId, testId);

            await using (var ctx = new BusinessDbContext(_options))
            {
                Assert.False(await ctx.Tests.AnyAsync());
            }
        }
    }
}
