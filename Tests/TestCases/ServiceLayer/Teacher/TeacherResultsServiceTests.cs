using Microsoft.EntityFrameworkCore;
using TestSystem.Data;
using TestSystem.Entities;
using TestSystem.MainContext;
using TestSystem.ServiceLayer.Services.Teacher;
using Tests.Infrastructure;

using TeacherEntity = TestSystem.Entities.Teacher;
using StudentEntity = TestSystem.Entities.Student;

namespace Tests.TestCases.ServiceLayer.Teacher
{
    public class TeacherResultsServiceTests
    {
        private readonly DbContextOptions<BusinessDbContext> _options;
        private readonly ApplicationDbContext _identityContext;
        private readonly TeacherResultsService _service;

        public TeacherResultsServiceTests()
        {
            _options = new DbContextOptionsBuilder<BusinessDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var factory = new TestDbContextFactory(_options);
            _identityContext = CreateIdentityContext();

            _service = new TeacherResultsService(factory, _identityContext);
        }

        [Fact]
        public async Task GetMyResultsAsync_ShouldReturnEmpty_WhenTeacherNotExists()
        {
            var results = await _service.GetMyResultsAsync("missing-teacher");

            Assert.NotNull(results);
            Assert.Empty(results);
        }

        [Fact]
        public async Task GetMyResultsAsync_ShouldReturnEmpty_WhenNoResults()
        {
            var teacherUserId = "teacher-1";

            await using (var ctx = new BusinessDbContext(_options))
            {
                ctx.Teachers.Add(new TeacherEntity { UserId = teacherUserId });
                await ctx.SaveChangesAsync();
            }

            var results = await _service.GetMyResultsAsync(teacherUserId);

            Assert.Empty(results);
        }

        [Fact]
        public async Task GetMyResultsAsync_ShouldReturnCorrectResult()
        {
            var teacherUserId = "teacher-2";
            var studentUserId = "student-1";

            // identity users
            _identityContext.Users.Add(new ApplicationUser
            {
                Id = studentUserId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@test.com"
            });
            await _identityContext.SaveChangesAsync();

            await using (var ctx = new BusinessDbContext(_options))
            {
                var teacher = new TeacherEntity { UserId = teacherUserId };
                var student = new StudentEntity { UserId = studentUserId };

                var test = new Test
                {
                    Title = "Test",
                    Description = "Desc",
                    MaxScore = 10,
                    Questions =
                    {
                        new Question { Text = "Q1", Points = 5 },
                        new Question { Text = "Q2", Points = 5 }
                    }
                };

                teacher.Tests.Add(test);
                teacher.Students.Add(student);

                ctx.AddRange(teacher, student);

                ctx.TestResults.Add(new TestResult
                {
                    Student = student,
                    Test = test,
                    Score = 7,
                    CompletedDate = DateTime.UtcNow
                });

                await ctx.SaveChangesAsync();
            }

            var results = await _service.GetMyResultsAsync(teacherUserId);

            Assert.Single(results);

            var r = results[0];
            Assert.Equal("Test", r.TestTitle);
            Assert.Equal("John Doe", r.StudentName);
            Assert.Equal("john@test.com", r.StudentEmail);
            Assert.Equal(7, r.Score);
            Assert.Equal(10, r.MaxScore);
        }

        [Fact]
        public async Task ResetStudentTestAsync_ShouldThrow_WhenTeacherDoesNotOwnTest()
        {
            var teacherUserId = "teacher-3";

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.ResetStudentTestAsync(teacherUserId, 1, 1));
        }

        [Fact]
        public async Task ResetStudentTestAsync_ShouldRemoveResult()
        {
            var teacherUserId = "teacher-4";
            var studentUserId = "student-2";

            int testId;
            int studentId;

            await using (var ctx = new BusinessDbContext(_options))
            {
                var teacher = new TeacherEntity { UserId = teacherUserId };
                var student = new StudentEntity { UserId = studentUserId };

                var test = new Test
                {
                    Title = "Test",
                    Description = "Desc",
                    MaxScore = 5,
                    Questions = { new Question { Text = "Q", Points = 5 } }
                };

                teacher.Tests.Add(test);
                teacher.Students.Add(student);

                ctx.AddRange(teacher, student);
                await ctx.SaveChangesAsync();

                testId = test.Id;
                studentId = student.Id;

                ctx.TestResults.Add(new TestResult
                {
                    StudentId = studentId,
                    TestId = testId,
                    Score = 5
                });

                await ctx.SaveChangesAsync();
            }

            await _service.ResetStudentTestAsync(teacherUserId, studentId, testId);

            await using (var ctx = new BusinessDbContext(_options))
            {
                var exists = await ctx.TestResults.AnyAsync();
                Assert.False(exists);
            }
        }

        private static ApplicationDbContext CreateIdentityContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }
    }
}
