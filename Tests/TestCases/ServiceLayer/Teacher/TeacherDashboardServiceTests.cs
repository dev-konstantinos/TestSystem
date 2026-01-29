using Microsoft.EntityFrameworkCore;
using TestSystem.Entities;
using TestSystem.MainContext;
using TestSystem.ServiceLayer.Services.Teacher;
using Tests.Infrastructure;

using TeacherEntity = TestSystem.Entities.Teacher;
using StudentEntity = TestSystem.Entities.Student;

namespace Tests.TestCases.ServiceLayer.Teacher
{
    public class TeacherDashboardServiceTests
    {
        private readonly DbContextOptions<BusinessDbContext> _options;
        private readonly TeacherDashboardService _service;

        public TeacherDashboardServiceTests()
        {
            _options = new DbContextOptionsBuilder<BusinessDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var factory = new TestDbContextFactory(_options);
            _service = new TeacherDashboardService(factory);
        }

        [Fact]
        public async Task GetDashboardAsync_ShouldReturnNull_WhenTeacherNotExists()
        {
            var result = await _service.GetDashboardAsync("missing-teacher");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetDashboardAsync_ShouldReturnZeroCounts_WhenTeacherHasNoData()
        {
            var userId = "teacher-1";

            await using (var ctx = new BusinessDbContext(_options))
            {
                ctx.Teachers.Add(new TeacherEntity { UserId = userId });
                await ctx.SaveChangesAsync();
            }

            var result = await _service.GetDashboardAsync(userId);

            Assert.NotNull(result);
            Assert.Equal(0, result!.StudentsCount);
            Assert.Equal(0, result.TestsCount);
            Assert.Equal(0, result.ResultsCount);
        }

        [Fact]
        public async Task GetDashboardAsync_ShouldCountStudentsCorrectly()
        {
            var userId = "teacher-2";

            await using (var ctx = new BusinessDbContext(_options))
            {
                var teacher = new TeacherEntity { UserId = userId };

                teacher.Students.Add(new StudentEntity { UserId = "student-1" });
                teacher.Students.Add(new StudentEntity { UserId = "student-2" });

                ctx.Teachers.Add(teacher);
                await ctx.SaveChangesAsync();
            }

            var result = await _service.GetDashboardAsync(userId);

            Assert.NotNull(result);
            Assert.Equal(2, result!.StudentsCount);
            Assert.Equal(0, result.TestsCount);
            Assert.Equal(0, result.ResultsCount);
        }

        [Fact]
        public async Task GetDashboardAsync_ShouldCountTestsCorrectly()
        {
            var userId = "teacher-3";

            await using (var ctx = new BusinessDbContext(_options))
            {
                var teacher = new TeacherEntity { UserId = userId };

                teacher.Tests.Add(new Test
                {
                    Title = "Test 1",
                    Description = "Desc",
                    MaxScore = 10
                });

                teacher.Tests.Add(new Test
                {
                    Title = "Test 2",
                    Description = "Desc",
                    MaxScore = 20
                });

                ctx.Teachers.Add(teacher);
                await ctx.SaveChangesAsync();
            }

            var result = await _service.GetDashboardAsync(userId);

            Assert.NotNull(result);
            Assert.Equal(0, result!.StudentsCount);
            Assert.Equal(2, result.TestsCount);
            Assert.Equal(0, result.ResultsCount);
        }

        [Fact]
        public async Task GetDashboardAsync_ShouldCountResultsCorrectly()
        {
            var userId = "teacher-4";

            await using (var ctx = new BusinessDbContext(_options))
            {
                var teacher = new TeacherEntity { UserId = userId };
                var student = new StudentEntity { UserId = "student-1" };

                var test = new Test
                {
                    Title = "Test",
                    Description = "Desc",
                    MaxScore = 10
                };

                teacher.Tests.Add(test);
                teacher.Students.Add(student);

                ctx.Teachers.Add(teacher);

                ctx.TestResults.Add(new TestResult
                {
                    Student = student,
                    Test = test,
                    Score = 7,
                    CompletedDate = DateTime.UtcNow
                });

                await ctx.SaveChangesAsync();
            }

            var result = await _service.GetDashboardAsync(userId);

            Assert.NotNull(result);
            Assert.Equal(1, result!.StudentsCount);
            Assert.Equal(1, result.TestsCount);
            Assert.Equal(1, result.ResultsCount);
        }
    }
}
