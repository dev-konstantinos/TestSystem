using Microsoft.EntityFrameworkCore;
using TestSystem.Entities;

namespace TestSystem.MainContext
{
    public class BusinessDbContext : DbContext
    {
        public BusinessDbContext(DbContextOptions<BusinessDbContext> options) : base(options) { }

        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Option> Options { get; set; }
        public DbSet<TestResult> TestResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== UserId uniqueness =====
            modelBuilder.Entity<Student>()
                .HasIndex(s => s.UserId)
                .IsUnique();

            modelBuilder.Entity<Teacher>()
                .HasIndex(t => t.UserId)
                .IsUnique();

            // ===== Teacher <-> Student (M2M) =====
            modelBuilder.Entity<Teacher>()
                .HasMany(t => t.Students)
                .WithMany(s => s.Teachers)
                .UsingEntity(j =>
                {
                    j.ToTable("TeacherStudents");
                });

            // ===== Teacher <-> Test (M2M) =====
            modelBuilder.Entity<Teacher>()
                .HasMany(t => t.Tests)
                .WithMany(t => t.Teachers)
                .UsingEntity(j =>
                {
                    j.ToTable("TeacherTests");
                });

            // ===== Test -> Question (1:M) =====
            modelBuilder.Entity<Question>()
                .HasOne(q => q.Test)
                .WithMany(t => t.Questions)
                .HasForeignKey(q => q.TestId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== Question -> Option (1:M) =====
            modelBuilder.Entity<Option>()
                .HasOne(o => o.Question)
                .WithMany(q => q.Options)
                .HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== Student -> TestResult (1:M) =====
            modelBuilder.Entity<TestResult>()
                .HasOne(tr => tr.Student)
                .WithMany(s => s.TestResults)
                .HasForeignKey(tr => tr.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== Test -> TestResult (1:M) =====
            modelBuilder.Entity<TestResult>()
                .HasOne(tr => tr.Test)
                .WithMany(t => t.TestResults)
                .HasForeignKey(tr => tr.TestId)
                .OnDelete(DeleteBehavior.Restrict);
        }


    }
}
