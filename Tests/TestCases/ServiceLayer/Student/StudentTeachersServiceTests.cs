using Microsoft.EntityFrameworkCore;
using Tests.Infrastructure;
using TestSystem.Data;
using TestSystem.Entities;
using TestSystem.MainContext;
using TestSystem.ServiceLayer.Services.Student;

using StudentEntity = TestSystem.Entities.Student;
using TeacherEntity = TestSystem.Entities.Teacher;

namespace Tests.TestCases.ServiceLayer.Student
{
    public class StudentTeachersServiceTests
    {
        private readonly DbContextOptions<BusinessDbContext> _businessOptions;
        private readonly DbContextOptions<ApplicationDbContext> _identityOptions;

        public StudentTeachersServiceTests()
        {
            _businessOptions = new DbContextOptionsBuilder<BusinessDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _identityOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private StudentTeachersService CreateService()
        {
            var factory = new TestDbContextFactory(_businessOptions);
            var identity = new ApplicationDbContext(_identityOptions);

            return new StudentTeachersService(factory, identity);
        }

        [Fact]
        public async Task GetMyTeachersAsync_ShouldReturnEmpty_WhenStudentDoesNotExist()
        {
            var service = CreateService();

            var result = await service.GetMyTeachersAsync("missing-user");

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMyTeachersAsync_ShouldReturnEmpty_WhenStudentHasNoTeachers()
        {
            var userId = "student-1";

            await using (var ctx = new BusinessDbContext(_businessOptions))
            {
                ctx.Students.Add(new StudentEntity { UserId = userId });
                await ctx.SaveChangesAsync();
            }

            var service = CreateService();

            var result = await service.GetMyTeachersAsync(userId);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMyTeachersAsync_ShouldReturnSingleTeacher()
        {
            var studentUserId = "student-2";
            var teacherUserId = "teacher-1";

            await using (var business = new BusinessDbContext(_businessOptions))
            {
                var teacher = new TeacherEntity
                {
                    UserId = teacherUserId,
                    JoinedDate = DateTime.UtcNow
                };

                var student = new StudentEntity { UserId = studentUserId };
                student.Teachers.Add(teacher);

                business.AddRange(student, teacher);
                await business.SaveChangesAsync();
            }

            await using (var identity = new ApplicationDbContext(_identityOptions))
            {
                identity.Users.Add(new ApplicationUser
                {
                    Id = teacherUserId,
                    Email = "teacher@test.com",
                    FirstName = "John",
                    LastName = "Doe"
                });

                await identity.SaveChangesAsync();
            }

            var service = CreateService();
            var result = await service.GetMyTeachersAsync(studentUserId);

            Assert.Single(result);

            var t = result[0];
            Assert.Equal("teacher@test.com", t.Email);
            Assert.Equal("John Doe", t.FullName);
            Assert.Equal(0, t.TestsCount);
        }

        [Fact]
        public async Task GetMyTeachersAsync_ShouldReturnMultipleTeachers_WithCorrectTestCounts()
        {
            var studentUserId = "student-3";

            await using (var business = new BusinessDbContext(_businessOptions))
            {
                var teacher1 = new TeacherEntity { UserId = "t1" };
                var teacher2 = new TeacherEntity { UserId = "t2" };

                teacher1.Tests.Add(new Test
                {
                    Title = "Test 1",
                    Description = "Desc",
                    MaxScore = 10
                });

                teacher2.Tests.Add(new Test
                {
                    Title = "Test 2",
                    Description = "Desc",
                    MaxScore = 10
                });

                teacher2.Tests.Add(new Test
                {
                    Title = "Test 3",
                    Description = "Desc",
                    MaxScore = 10
                });

                var student = new StudentEntity { UserId = studentUserId };
                student.Teachers.Add(teacher1);
                student.Teachers.Add(teacher2);

                business.AddRange(student, teacher1, teacher2);
                await business.SaveChangesAsync();
            }

            await using (var identity = new ApplicationDbContext(_identityOptions))
            {
                identity.Users.AddRange(
                    new ApplicationUser
                    {
                        Id = "t1",
                        Email = "t1@test.com",
                        FirstName = "Alice",
                        LastName = "One"
                    },
                    new ApplicationUser
                    {
                        Id = "t2",
                        Email = "t2@test.com",
                        FirstName = "Bob",
                        LastName = "Two"
                    });

                await identity.SaveChangesAsync();
            }

            var service = CreateService();
            var result = await service.GetMyTeachersAsync(studentUserId);

            Assert.Equal(2, result.Count);

            var t1 = result.Single(t => t.UserId == "t1");
            var t2 = result.Single(t => t.UserId == "t2");

            Assert.Equal(1, t1.TestsCount);
            Assert.Equal(2, t2.TestsCount);
        }
    }
}
