using Microsoft.EntityFrameworkCore;
using PerformanceReviewBot.Data;

namespace PerformanceReviewBot.Tests.Helpers;

public static class TestDbContextFactory
{
    public static AppDbContext CreateInMemoryContext(string dbName = "TestDb")
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"DataSource={dbName};Mode=Memory;Cache=Shared")
            .Options;

        var context = new AppDbContext(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();
        return context;
    }

    public static void DisposeContext(AppDbContext context)
    {
        context.Database.CloseConnection();
        context.Dispose();
    }
}
