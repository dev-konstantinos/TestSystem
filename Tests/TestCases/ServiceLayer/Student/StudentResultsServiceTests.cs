using Microsoft.EntityFrameworkCore;
using Tests.Infrastructure;
using TestSystem.Entities;
using TestSystem.MainContext;
using TestSystem.ServiceLayer.Services.Student;
using StudentEntity = TestSystem.Entities.Student;

namespace Tests.TestCases.ServiceLayer.Student
{
    public class StudentResultsServiceTests
    {
        private readonly DbContextOptions<BusinessDbContext> _options;
        private readonly StudentResultsService _service;

        public StudentResultsServiceTests()
        {
            _options = new DbContextOptionsBuilder<BusinessDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var factory = new TestDbContextFactory(_options);
            _service = new StudentResultsService(factory);
        }

        [Fact]
        public async Task GetMyResultsAsync_ShouldReturnEmpty_WhenStudentDoesNotExist()
        {
            var results = await _service.GetMyResultsAsync("missing-user");

            Assert.NotNull(results);
            Assert.Empty(results);
        }

        [Fact]
        public async Task GetMyResultsAsync_ShouldReturnEmpty_WhenStudentHasNoResults()
        {
            var userId = "student-1";

            await using (var ctx = new BusinessDbContext(_options))
            {
                ctx.Students.Add(new StudentEntity { UserId = userId });
                await ctx.SaveChangesAsync();
            }

            var results = await _service.GetMyResultsAsync(userId);

            Assert.NotNull(results);
            Assert.Empty(results);
        }

        [Fact]
        public async Task GetMyResultsAsync_ShouldReturnSingleResult()
        {
            var userId = "student-2";

            await using (var ctx = new BusinessDbContext(_options))
            {
                var student = new StudentEntity { UserId = userId };

                var test = new Test
                {
                    Title = "Test 1",
                    Description = "Desc",
                    Questions =
                    {
                        new Question { Text = "Q1", Points = 5 },
                        new Question { Text = "Q2", Points = 3 }
                    }
                };

                ctx.TestResults.Add(new TestResult
                {
                    Student = student,
                    Test = test,
                    Score = 6,
                    CompletedDate = DateTime.UtcNow
                });

                await ctx.SaveChangesAsync();
            }

            var results = await _service.GetMyResultsAsync(userId);

            Assert.Single(results);

            var result = results[0];
            Assert.Equal("Test 1", result.TestTitle);
            Assert.Equal(6, result.Score); // out of 8
            Assert.Equal(8, result.MaxScore); // 5 + 3
        }

        [Fact]
        public async Task GetMyResultsAsync_ShouldOrderByCompletedDateDesc()
        {
            var userId = "student-3";

            var older = DateTime.UtcNow.AddDays(-1);
            var newer = DateTime.UtcNow;

            await using (var ctx = new BusinessDbContext(_options))
            {
                var student = new StudentEntity { UserId = userId };

                var test1 = new Test
                {
                    Title = "Old Test",
                    Description = "Desc",
                    Questions = { new Question { Text = "Q", Points = 1 } }
                };

                var test2 = new Test
                {
                    Title = "New Test",
                    Description = "Desc",
                    Questions = { new Question { Text = "Q", Points = 1 } }
                };

                ctx.TestResults.AddRange(
                    new TestResult
                    {
                        Student = student,
                        Test = test1,
                        Score = 1,
                        CompletedDate = older
                    },
                    new TestResult
                    {
                        Student = student,
                        Test = test2,
                        Score = 1,
                        CompletedDate = newer
                    });

                await ctx.SaveChangesAsync();
            }

            var results = await _service.GetMyResultsAsync(userId);

            Assert.Equal(2, results.Count);
            Assert.Equal("New Test", results[0].TestTitle);
            Assert.Equal("Old Test", results[1].TestTitle);
        }
    }
}
