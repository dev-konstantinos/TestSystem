using Microsoft.EntityFrameworkCore;
using TestSystem.Data;
using TestSystem.Entities.DTOs.Teacher;
using TestSystem.MainContext;
using TestSystem.ServiceLayer.Interfaces.Teacher;

namespace TestSystem.ServiceLayer.Services.Teacher
{
    // Class provides services related to retrieving students assigned to a specific teacher
    public class TeacherStudentsService : ITeacherStudentsService
    {
        private readonly BusinessDbContext _businessContext;
        private readonly ApplicationDbContext _identityContext;

        public TeacherStudentsService(
            BusinessDbContext db,
            ApplicationDbContext identityDb)
        {
            _businessContext = db;
            _identityContext = identityDb;
        }

        // Retrieves a list of students assigned to the specified teacher at Teacher's Dashboard
        public async Task<List<TeacherStudentDto>> GetMyStudentsAsync(string teacherUserId)
        {
            var teacherId = await _businessContext.Teachers
                .Where(t => t.UserId == teacherUserId)
                .Select(t => t.Id)
                .FirstOrDefaultAsync();

            if (teacherId == 0)
                return new();

            // 1️) Getting students from business DB
            var students = await _businessContext.Students
                .Where(s => s.Teachers.Any(t => t.Id == teacherId))
                .Select(s => new
                {
                    s.Id,
                    s.UserId,
                    s.EnrolledDate,
                    TestsPassed = s.TestResults.Count,
                    AverageScore = s.TestResults.Any()
                        ? s.TestResults.Average(r => r.Score)
                        : (double?)null
                })
                .AsNoTracking()
                .ToListAsync();

            var userIds = students.Select(s => s.UserId).ToList();

            // 2️) Getting users from identity DB
            var users = await _identityContext.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FirstName,
                    u.LastName
                })
                .ToListAsync();

            // 3️) Combining data from both DBs
            return students.Select(s =>
            {
                var user = users.First(u => u.Id == s.UserId);

                return new TeacherStudentDto
                {
                    StudentId = s.Id,
                    UserId = user.Id,
                    Email = user.Email!,
                    FullName = $"{user.FirstName} {user.LastName}",
                    EnrolledDate = s.EnrolledDate,
                    TestsPassed = s.TestsPassed,
                    AverageScore = s.AverageScore
                };
            }).ToList();
        }

        // Retrieves a list of students not assigned to the specified teacher at Students page
        public async Task<List<TeacherStudentDto>> GetAvailableStudentsAsync(string teacherUserId)
        {
            var teacherId = await _businessContext.Teachers
                .Where(t => t.UserId == teacherUserId)
                .Select(t => t.Id)
                .FirstOrDefaultAsync();

            if (teacherId == 0)
                return new();

            // id of students already assigned to the teacher
            var assignedStudentIds = await _businessContext.Students
                .Where(s => s.Teachers.Any(t => t.Id == teacherId))
                .Select(s => s.Id)
                .ToListAsync();

            // all students not assigned to the teacher
            var students = await _businessContext.Students
                .Where(s => !assignedStudentIds.Contains(s.Id))
                .Select(s => new
                {
                    s.Id,
                    s.UserId,
                    s.EnrolledDate
                })
                .AsNoTracking()
                .ToListAsync();

            var userIds = students.Select(s => s.UserId).ToList();

            var users = await _identityContext.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FirstName,
                    u.LastName
                })
                .ToListAsync();

            return students.Select(s =>
            {
                var user = users.First(u => u.Id == s.UserId);

                return new TeacherStudentDto
                {
                    StudentId = s.Id,
                    UserId = user.Id,
                    Email = user.Email!,
                    FullName = $"{user.FirstName} {user.LastName}",
                    EnrolledDate = s.EnrolledDate,
                    TestsPassed = 0,
                    AverageScore = null
                };
            }).ToList();
        }

        // Attaches a student to the specified teacher
        public async Task AttachStudentAsync(string teacherUserId, int studentId)
        {
            // 1) Getting teacher
            var teacher = await _businessContext.Teachers
                .Include(t => t.Students)
                .FirstOrDefaultAsync(t => t.UserId == teacherUserId);

            if (teacher == null)
                throw new InvalidOperationException("Teacher not found");

            // 2) Getting student
            var student = await _businessContext.Students
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
                throw new InvalidOperationException("Student not found");

            // 3) Checking existing association
            if (teacher.Students.Any(s => s.Id == studentId))
                return; // silently ignore or throw exception based on requirements

            // 4) Attaching student to teacher
            teacher.Students.Add(student);
            await _businessContext.SaveChangesAsync();
        }

        // Detaches a student from the specified teacher
        public async Task DetachStudentAsync(string teacherUserId, int studentId)
        {
            var teacher = await _businessContext.Teachers
                .Include(t => t.Students)
                .FirstOrDefaultAsync(t => t.UserId == teacherUserId);

            if (teacher == null)
                throw new InvalidOperationException("Teacher not found");

            var student = teacher.Students.FirstOrDefault(s => s.Id == studentId);
            if (student == null)
                return;

            teacher.Students.Remove(student);
            await _businessContext.SaveChangesAsync();
        }
    }
}
