using Microsoft.EntityFrameworkCore;
using TestSystem.Entities;
using TestSystem.MainContext;
using TestSystem.ServiceLayer.Services.Teacher;
using Tests.Infrastructure;

using TeacherEntity = TestSystem.Entities.Teacher;

namespace Tests.TestCases.ServiceLayer.Teacher
{
    public class TeacherTestEditorServiceTests
    {
        private readonly DbContextOptions<BusinessDbContext> _options;
        private readonly TeacherTestEditorService _service;

        public TeacherTestEditorServiceTests()
        {
            _options = new DbContextOptionsBuilder<BusinessDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var factory = new TestDbContextFactory(_options);
            _service = new TeacherTestEditorService(factory);
        }

        [Fact]
        public async Task GetQuestionsAsync_ShouldThrow_WhenTestNotOwned()
        {
            var teacherId = "teacher-1";

            await using (var ctx = new BusinessDbContext(_options))
            {
                ctx.Tests.Add(new Test { Title = "Test", Description = "Desc" });
                await ctx.SaveChangesAsync();
            }

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.GetQuestionsAsync(teacherId, 1));
        }

        [Fact]
        public async Task AddQuestionAsync_ShouldAddQuestion_AndRecalculateMaxScore()
        {
            var teacherId = "teacher-2";
            int testId;

            await using (var ctx = new BusinessDbContext(_options))
            {
                var teacher = new TeacherEntity { UserId = teacherId };
                var test = new Test
                {
                    Title = "Test",
                    Description = "Desc",
                    MaxScore = 0
                };

                teacher.Tests.Add(test);
                ctx.Add(teacher);
                await ctx.SaveChangesAsync();

                testId = test.Id;
            }

            await _service.AddQuestionAsync(teacherId, testId, "Q1", 5);

            await using (var ctx = new BusinessDbContext(_options))
            {
                var test = await ctx.Tests
                    .Include(t => t.Questions)
                    .FirstAsync();

                Assert.Single(test.Questions);
                Assert.Equal(5, test.MaxScore);
            }
        }

        [Fact]
        public async Task DeleteQuestionAsync_ShouldRemoveQuestion_AndRecalculateMaxScore()
        {
            var teacherId = "teacher-3";
            int questionId;
            int testId;

            await using (var ctx = new BusinessDbContext(_options))
            {
                var teacher = new TeacherEntity { UserId = teacherId };
                var test = new Test
                {
                    Title = "Test",
                    Description = "Desc",
                    MaxScore = 10
                };

                var question = new Question
                {
                    Text = "Q",
                    Points = 10
                };

                test.Questions.Add(question);
                teacher.Tests.Add(test);

                ctx.Add(teacher);
                await ctx.SaveChangesAsync();

                questionId = question.Id;
                testId = test.Id;
            }

            await _service.DeleteQuestionAsync(teacherId, questionId);

            await using (var ctx = new BusinessDbContext(_options))
            {
                var test = await ctx.Tests.FirstAsync();
                var questionsCount = await ctx.Questions.CountAsync();

                Assert.Equal(0, questionsCount);
                Assert.Equal(0, test.MaxScore);
            }
        }

        [Fact]
        public async Task AddOptionAsync_ShouldAddOption()
        {
            var teacherId = "teacher-4";
            int questionId;

            await using (var ctx = new BusinessDbContext(_options))
            {
                var teacher = new TeacherEntity { UserId = teacherId };
                var test = new Test
                {
                    Title = "Test",
                    Description = "Desc"
                };

                var question = new Question
                {
                    Text = "Q",
                    Points = 1
                };

                test.Questions.Add(question);
                teacher.Tests.Add(test);

                ctx.Add(teacher);
                await ctx.SaveChangesAsync();

                questionId = question.Id;
            }

            await _service.AddOptionAsync(teacherId, questionId, "Option", true);

            await using (var ctx = new BusinessDbContext(_options))
            {
                var options = await ctx.Options.ToListAsync();
                Assert.Single(options);
                Assert.True(options[0].IsCorrect);
            }
        }

        [Fact]
        public async Task DeleteOptionAsync_ShouldRemoveOption()
        {
            var teacherId = "teacher-5";
            int optionId;

            await using (var ctx = new BusinessDbContext(_options))
            {
                var teacher = new TeacherEntity { UserId = teacherId };
                var test = new Test
                {
                    Title = "Test",
                    Description = "Desc"
                };

                var question = new Question
                {
                    Text = "Q",
                    Points = 1
                };

                var option = new Option
                {
                    Text = "Opt",
                    IsCorrect = true
                };

                question.Options.Add(option);
                test.Questions.Add(question);
                teacher.Tests.Add(test);

                ctx.Add(teacher);
                await ctx.SaveChangesAsync();

                optionId = option.Id;
            }

            await _service.DeleteOptionAsync(teacherId, optionId);

            await using (var ctx = new BusinessDbContext(_options))
            {
                Assert.False(await ctx.Options.AnyAsync());
            }
        }
    }
}
