using Microsoft.EntityFrameworkCore;
using TestSystem.Entities;
using TestSystem.MainContext;
using Tests.Infrastructure;
using TestSystem.ServiceLayer.Services.Student;

using StudentEntity = TestSystem.Entities.Student;
using TeacherEntity = TestSystem.Entities.Teacher;

namespace Tests.TestCases.ServiceLayer.Student
{
    public class StudentDashboardServiceTests
    {
        private readonly DbContextOptions<BusinessDbContext> _options;
        private readonly StudentDashboardService _service;

        public StudentDashboardServiceTests()
        {
            _options = new DbContextOptionsBuilder<BusinessDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var factory = new TestDbContextFactory(_options);
            _service = new StudentDashboardService(factory);
        }

        [Fact]
        public async Task GetDashboardAsync_ShouldReturnNull_WhenStudentNotExists()
        {
            var result = await _service.GetDashboardAsync("missing-user");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetDashboardAsync_ShouldReturnZeroCounts_WhenStudentHasNoData()
        {
            var userId = "student-1";

            await using (var ctx = new BusinessDbContext(_options))
            {
                ctx.Students.Add(new StudentEntity { UserId = userId });
                await ctx.SaveChangesAsync();
            }

            var result = await _service.GetDashboardAsync(userId);

            Assert.NotNull(result);
            Assert.Equal(0, result!.TeachersCount);
            Assert.Equal(0, result.AvailableTestsCount);
            Assert.Equal(0, result.ResultsCount);
        }

        [Fact]
        public async Task GetDashboardAsync_ShouldReturnCorrectCounts()
        {
            var userId = "student-2";

            await using (var ctx = new BusinessDbContext(_options))
            {
                var teacher1 = new TeacherEntity { UserId = "teacher-1" };
                var teacher2 = new TeacherEntity { UserId = "teacher-2" };

                var test1 = new Test { Title = "Test 1", Description = "Desc", MaxScore = 10 };
                var test2 = new Test { Title = "Test 2", Description = "Desc", MaxScore = 10 };

                teacher1.Tests.Add(test1);
                teacher2.Tests.Add(test2);

                var student = new StudentEntity { UserId = userId };
                student.Teachers.Add(teacher1);
                student.Teachers.Add(teacher2);

                ctx.AddRange(student, teacher1, teacher2, test1, test2);

                ctx.TestResults.Add(new TestResult
                {
                    Student = student,
                    Test = test1,
                    Score = 8
                });

                ctx.TestResults.Add(new TestResult
                {
                    Student = student,
                    Test = test2,
                    Score = 6
                });

                await ctx.SaveChangesAsync();
            }

            var result = await _service.GetDashboardAsync(userId);

            Assert.NotNull(result);
            Assert.Equal(2, result!.TeachersCount);
            Assert.Equal(2, result.AvailableTestsCount);
            Assert.Equal(2, result.ResultsCount);
        }
    }
}
