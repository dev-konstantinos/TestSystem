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
    public class TeacherStudentsServiceTests
    {
        private readonly DbContextOptions<BusinessDbContext> _options;
        private readonly ApplicationDbContext _identityContext;
        private readonly TeacherStudentsService _service;

        public TeacherStudentsServiceTests()
        {
            _options = new DbContextOptionsBuilder<BusinessDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var factory = new TestDbContextFactory(_options);
            _identityContext = CreateIdentityContext();

            _service = new TeacherStudentsService(factory, _identityContext);
        }

        // ---------- GetMyStudentsAsync ----------

        [Fact]
        public async Task GetMyStudentsAsync_ShouldReturnEmpty_WhenTeacherNotExists()
        {
            var result = await _service.GetMyStudentsAsync("missing-teacher");
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMyStudentsAsync_ShouldReturnEmpty_WhenNoStudents()
        {
            var teacherUserId = "teacher-1";

            await using (var ctx = new BusinessDbContext(_options))
            {
                ctx.Teachers.Add(new TeacherEntity { UserId = teacherUserId });
                await ctx.SaveChangesAsync();
            }

            var result = await _service.GetMyStudentsAsync(teacherUserId);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMyStudentsAsync_ShouldReturnStudents_WithStats()
        {
            var teacherUserId = "teacher-2";
            var studentUserId = "student-1";

            _identityContext.Users.Add(new ApplicationUser
            {
                Id = studentUserId,
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane@test.com"
            });
            await _identityContext.SaveChangesAsync();

            await using (var ctx = new BusinessDbContext(_options))
            {
                var teacher = new TeacherEntity { UserId = teacherUserId };
                var student = new StudentEntity { UserId = studentUserId };

                teacher.Students.Add(student);

                ctx.AddRange(teacher, student);

                ctx.TestResults.Add(new TestResult
                {
                    Student = student,
                    Score = 8
                });

                await ctx.SaveChangesAsync();
            }

            var result = await _service.GetMyStudentsAsync(teacherUserId);

            Assert.Single(result);
            var dto = result[0];

            Assert.Equal("Jane Doe", dto.FullName);
            Assert.Equal("jane@test.com", dto.Email);
            Assert.Equal(1, dto.TestsPassed);
            Assert.Equal(8, dto.AverageScore);
        }

        // ---------- GetAvailableStudentsAsync ----------

        [Fact]
        public async Task GetAvailableStudentsAsync_ShouldReturnEmpty_WhenTeacherNotExists()
        {
            var result = await _service.GetAvailableStudentsAsync("missing-teacher");
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAvailableStudentsAsync_ShouldExcludeAssignedStudents()
        {
            var teacherUserId = "teacher-3";

            _identityContext.Users.AddRange(
                new ApplicationUser { Id = "s1", FirstName = "A", LastName = "A", Email = "a@test.com" },
                new ApplicationUser { Id = "s2", FirstName = "B", LastName = "B", Email = "b@test.com" }
            );
            await _identityContext.SaveChangesAsync();

            await using (var ctx = new BusinessDbContext(_options))
            {
                var teacher = new TeacherEntity { UserId = teacherUserId };
                var student1 = new StudentEntity { UserId = "s1" };
                var student2 = new StudentEntity { UserId = "s2" };

                teacher.Students.Add(student1);

                ctx.AddRange(teacher, student1, student2);
                await ctx.SaveChangesAsync();
            }

            var result = await _service.GetAvailableStudentsAsync(teacherUserId);

            Assert.Single(result);
            Assert.Equal("B B", result[0].FullName);
        }

        // ---------- AttachStudentAsync ----------

        [Fact]
        public async Task AttachStudentAsync_ShouldThrow_WhenTeacherNotFound()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AttachStudentAsync("missing", 1));
        }

        [Fact]
        public async Task AttachStudentAsync_ShouldAttachStudent()
        {
            var teacherUserId = "teacher-4";

            int studentId;

            await using (var ctx = new BusinessDbContext(_options))
            {
                var teacher = new TeacherEntity { UserId = teacherUserId };
                var student = new StudentEntity { UserId = "student-x" };

                ctx.AddRange(teacher, student);
                await ctx.SaveChangesAsync();

                studentId = student.Id;
            }

            await _service.AttachStudentAsync(teacherUserId, studentId);

            await using (var ctx = new BusinessDbContext(_options))
            {
                var teacher = await ctx.Teachers
                    .Include(t => t.Students)
                    .FirstAsync();

                Assert.Single(teacher.Students);
            }
        }

        // ---------- DetachStudentAsync ----------

        [Fact]
        public async Task DetachStudentAsync_ShouldRemoveStudent()
        {
            var teacherUserId = "teacher-5";

            int studentId;

            await using (var ctx = new BusinessDbContext(_options))
            {
                var teacher = new TeacherEntity { UserId = teacherUserId };
                var student = new StudentEntity { UserId = "student-y" };

                teacher.Students.Add(student);

                ctx.AddRange(teacher, student);
                await ctx.SaveChangesAsync();

                studentId = student.Id;
            }

            await _service.DetachStudentAsync(teacherUserId, studentId);

            await using (var ctx = new BusinessDbContext(_options))
            {
                var teacher = await ctx.Teachers
                    .Include(t => t.Students)
                    .FirstAsync();

                Assert.Empty(teacher.Students);
            }
        }

        // ---------- helpers ----------

        private static ApplicationDbContext CreateIdentityContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }
    }
}
