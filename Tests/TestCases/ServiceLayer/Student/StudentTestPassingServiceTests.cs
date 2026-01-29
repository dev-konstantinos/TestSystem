using Microsoft.EntityFrameworkCore;
using Tests.Infrastructure;
using TestSystem.Entities;
using TestSystem.Entities.DTOs.Student;
using TestSystem.MainContext;
using TestSystem.ServiceLayer.Services.Student;

using StudentEntity = TestSystem.Entities.Student;
using TeacherEntity = TestSystem.Entities.Teacher;

namespace Tests.TestCases.ServiceLayer.Student
{
    public class StudentTestPassingServiceTests
    {
        private readonly DbContextOptions<BusinessDbContext> _options;

        public StudentTestPassingServiceTests()
        {
            _options = new DbContextOptionsBuilder<BusinessDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private StudentTestPassingService CreateService()
        {
            var factory = new TestDbContextFactory(_options);
            return new StudentTestPassingService(factory);
        }

        // ---------- GetTestAsync ----------

        [Fact]
        public async Task GetTestAsync_ShouldThrow_WhenStudentNotFound()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.GetTestAsync("missing-user", 1));
        }

        [Fact]
        public async Task GetTestAsync_ShouldThrow_WhenTestNotFound()
        {
            await using (var ctx = new BusinessDbContext(_options))
            {
                ctx.Students.Add(new StudentEntity { UserId = "student-1" });
                await ctx.SaveChangesAsync();
            }

            var service = CreateService();

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.GetTestAsync("student-1", 999));
        }

        [Fact]
        public async Task GetTestAsync_ShouldThrow_WhenAccessDenied()
        {
            await using (var ctx = new BusinessDbContext(_options))
            {
                var student = new StudentEntity { UserId = "student-1" };
                var teacher = new TeacherEntity { UserId = "teacher-1" };
                var test = new Test
                {
                    Title = "Test",
                    Description = "Desc",
                    Teachers = { teacher }
                };

                ctx.AddRange(student, teacher, test);
                await ctx.SaveChangesAsync();
            }

            var service = CreateService();

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.GetTestAsync("student-1", 1));
        }

        [Fact]
        public async Task GetTestAsync_ShouldReturnCompleted_WhenTestAlreadyPassed()
        {
            await using (var ctx = new BusinessDbContext(_options))
            {
                var student = new StudentEntity { UserId = "student-1" };
                var teacher = new TeacherEntity { UserId = "teacher-1" };
                var test = new Test
                {
                    Title = "Test",
                    Description = "Desc",
                    Teachers = { teacher }
                };

                student.Teachers.Add(teacher);

                ctx.AddRange(student, teacher, test);

                ctx.TestResults.Add(new TestResult
                {
                    Student = student,
                    Test = test,
                    Score = 5
                });

                await ctx.SaveChangesAsync();
            }

            var service = CreateService();
            var dto = await service.GetTestAsync("student-1", 1);

            Assert.True(dto.IsCompleted);
            Assert.Empty(dto.Questions);
        }

        [Fact]
        public async Task GetTestAsync_ShouldReturnQuestions_WhenNotCompleted()
        {
            await using (var ctx = new BusinessDbContext(_options))
            {
                var student = new StudentEntity { UserId = "student-1" };
                var teacher = new TeacherEntity { UserId = "teacher-1" };

                var test = new Test
                {
                    Title = "Test",
                    Description = "Desc",
                    Teachers = { teacher },
                    Questions =
                    {
                        new Question
                        {
                            Text = "Q1",
                            Points = 5,
                            Options =
                            {
                                new Option { Text = "A", IsCorrect = true },
                                new Option { Text = "B" }
                            }
                        }
                    }
                };

                student.Teachers.Add(teacher);
                ctx.AddRange(student, teacher, test);
                await ctx.SaveChangesAsync();
            }

            var service = CreateService();
            var dto = await service.GetTestAsync("student-1", 1);

            Assert.False(dto.IsCompleted);
            Assert.Single(dto.Questions);
            Assert.Equal(2, dto.Questions[0].Options.Count);
        }

        // ---------- SubmitAsync ----------

        [Fact]
        public async Task SubmitAsync_ShouldThrow_WhenAlreadyCompleted()
        {
            await using (var ctx = new BusinessDbContext(_options))
            {
                var student = new StudentEntity { UserId = "student-1" };
                var test = new Test { Title = "Test", Description = "Desc" };

                ctx.AddRange(student, test);

                ctx.TestResults.Add(new TestResult
                {
                    Student = student,
                    Test = test,
                    Score = 3
                });

                await ctx.SaveChangesAsync();
            }

            var service = CreateService();

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.SubmitAsync("student-1",
                    new StudentTestSubmitDto
                    {
                        TestId = 1,
                        Answers = new()
                    }));
        }

        [Fact]
        public async Task SubmitAsync_ShouldCalculateScoreCorrectly()
        {
            await using (var ctx = new BusinessDbContext(_options))
            {
                var student = new StudentEntity { UserId = "student-1" };

                var test = new Test
                {
                    Title = "Test",
                    Description = "Desc",
                    Questions =
                    {
                        new Question
                        {
                            Id = 1,
                            Text = "Q1",
                            Points = 5,
                            Options =
                            {
                                new Option { Id = 10, Text = "A", IsCorrect = true },
                                new Option { Id = 11, Text = "B" }
                            }
                        },
                        new Question
                        {
                            Id = 2,
                            Text = "Q2",
                            Points = 3,
                            Options =
                            {
                                new Option { Id = 20, Text = "A", IsCorrect = true },
                                new Option { Id = 21, Text = "B" }
                            }
                        }
                    }
                };

                ctx.AddRange(student, test);
                await ctx.SaveChangesAsync();
            }

            var service = CreateService();

            await service.SubmitAsync("student-1",
                new StudentTestSubmitDto
                {
                    TestId = 1,
                    Answers =
                    {
                        [1] = 10, // correct
                        [2] = 21  // wrong
                    }
                });

            await using var verify = new BusinessDbContext(_options);

            var result = await verify.TestResults.SingleAsync();

            Assert.Equal(5, result.Score);
        }
    }
}
