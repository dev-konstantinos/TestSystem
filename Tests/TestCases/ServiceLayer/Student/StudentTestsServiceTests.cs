using Microsoft.EntityFrameworkCore;
using TestSystem.Data;
using TestSystem.Entities;
using TestSystem.MainContext;
using TestSystem.ServiceLayer.Services.Student;
using Tests.Infrastructure;

using StudentEntity = TestSystem.Entities.Student;
using TeacherEntity = TestSystem.Entities.Teacher;

namespace Tests.TestCases.ServiceLayer.Student
{
    public class StudentTestsServiceTests
    {
        private readonly DbContextOptions<BusinessDbContext> _businessOptions;
        private readonly ApplicationDbContext _identityContext;
        private readonly StudentTestsService _service;

        public StudentTestsServiceTests()
        {
            _businessOptions = new DbContextOptionsBuilder<BusinessDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _identityContext = new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options);

            var factory = new TestDbContextFactory(_businessOptions);
            _service = new StudentTestsService(factory, _identityContext);
        }

        [Fact]
        public async Task GetAvailableTestsAsync_ShouldReturnEmpty_WhenStudentNotExists()
        {
            var result = await _service.GetAvailableTestsAsync("missing");

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAvailableTestsAsync_ShouldReturnEmpty_WhenStudentHasNoTeachers()
        {
            var userId = "student-1";

            await using (var ctx = new BusinessDbContext(_businessOptions))
            {
                ctx.Students.Add(new StudentEntity { UserId = userId });
                await ctx.SaveChangesAsync();
            }

            var result = await _service.GetAvailableTestsAsync(userId);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAvailableTestsAsync_ShouldReturnTests_WhenAssigned()
        {
            var studentId = "student-2";
            var teacherId = "teacher-1";

            await _identityContext.Users.AddAsync(new ApplicationUser
            {
                Id = teacherId,
                FirstName = "John",
                LastName = "Smith"
            });
            await _identityContext.SaveChangesAsync();

            await using (var ctx = new BusinessDbContext(_businessOptions))
            {
                var test = new Test
                {
                    Title = "Math Test",
                    Description = "Desc",
                    Questions =
                    {
                        new Question { Text = "Q1", Points = 2 },
                        new Question { Text = "Q2", Points = 3 }
                    }
                };

                var teacher = new TeacherEntity { UserId = teacherId };
                teacher.Tests.Add(test);

                var student = new StudentEntity { UserId = studentId };
                student.Teachers.Add(teacher);

                ctx.AddRange(student, teacher, test);
                await ctx.SaveChangesAsync();
            }

            var result = await _service.GetAvailableTestsAsync(studentId);

            Assert.Single(result);

            var dto = result[0];
            Assert.Equal("Math Test", dto.Title);
            Assert.Equal("John Smith", dto.TeacherName);
            Assert.Equal(2, dto.QuestionsCount);
            Assert.Equal(5, dto.MaxScore);
            Assert.False(dto.IsCompleted);
            Assert.Null(dto.Score);
        }

        [Fact]
        public async Task GetAvailableTestsAsync_ShouldMarkCompletedTests()
        {
            var studentId = "student-3";
            var teacherId = "teacher-2";

            await _identityContext.Users.AddAsync(new ApplicationUser
            {
                Id = teacherId,
                FirstName = "Anna",
                LastName = "Brown"
            });
            await _identityContext.SaveChangesAsync();

            int testId;

            await using (var ctx = new BusinessDbContext(_businessOptions))
            {
                var test = new Test
                {
                    Title = "History",
                    Description = "Desc",
                    Questions =
                    {
                        new Question { Text = "Q", Points = 4 }
                    }
                };

                var teacher = new TeacherEntity { UserId = teacherId };
                teacher.Tests.Add(test);

                var student = new StudentEntity { UserId = studentId };
                student.Teachers.Add(teacher);

                ctx.AddRange(student, teacher, test);
                await ctx.SaveChangesAsync();

                testId = test.Id;

                ctx.TestResults.Add(new TestResult
                {
                    StudentId = student.Id,
                    TestId = testId,
                    Score = 4
                });

                await ctx.SaveChangesAsync();
            }

            var result = await _service.GetAvailableTestsAsync(studentId);

            Assert.Single(result);

            var dto = result[0];
            Assert.True(dto.IsCompleted);
            Assert.Equal(4, dto.Score);
        }

        [Fact]
        public async Task GetAvailableTestsAsync_ShouldNotDuplicateTests_FromMultipleTeachers()
        {
            var studentId = "student-4";

            var teacher1Id = "teacher-a";
            var teacher2Id = "teacher-b";

            await _identityContext.Users.AddRangeAsync(
                new ApplicationUser { Id = teacher1Id, FirstName = "T1", LastName = "L1" },
                new ApplicationUser { Id = teacher2Id, FirstName = "T2", LastName = "L2" }
            );
            await _identityContext.SaveChangesAsync();

            await using (var ctx = new BusinessDbContext(_businessOptions))
            {
                var test = new Test
                {
                    Title = "Shared Test",
                    Description = "Desc",
                    Questions =
                    {
                        new Question { Text = "Q", Points = 1 }
                    }
                };

                var t1 = new TeacherEntity { UserId = teacher1Id };
                var t2 = new TeacherEntity { UserId = teacher2Id };

                t1.Tests.Add(test);
                t2.Tests.Add(test);

                var student = new StudentEntity { UserId = studentId };
                student.Teachers.Add(t1);
                student.Teachers.Add(t2);

                ctx.AddRange(student, t1, t2, test);
                await ctx.SaveChangesAsync();
            }

            var result = await _service.GetAvailableTestsAsync(studentId);

            Assert.Single(result);
        }
    }
}
