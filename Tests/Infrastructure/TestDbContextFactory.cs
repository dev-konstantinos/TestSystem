using Microsoft.EntityFrameworkCore;
using TestSystem.MainContext;

namespace Tests.Infrastructure
{
    // Class for creating instances of BusinessDbContext for testing purposes with specified options
    public sealed class TestDbContextFactory : IDbContextFactory<BusinessDbContext>
    {
        private readonly DbContextOptions<BusinessDbContext> _options;

        public TestDbContextFactory(DbContextOptions<BusinessDbContext> options)
        {
            _options = options;
        }

        public BusinessDbContext CreateDbContext()
            => new BusinessDbContext(_options);

        public ValueTask<BusinessDbContext> CreateDbContextAsync(
            CancellationToken cancellationToken = default)
            => ValueTask.FromResult(new BusinessDbContext(_options));
    }

}
