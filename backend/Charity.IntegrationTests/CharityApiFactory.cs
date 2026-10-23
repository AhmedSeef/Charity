using System.Net.Http.Headers;
using System.Net.Http.Json;
using Charity.Application.DTOs.Auth;
using Charity.Infrastructure.Data.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Charity.Domain.Entities;
using Charity.Infrastructure.Data.Seed;

namespace Charity.IntegrationTests;

// Real SQL Server, real migrations, Identity password hashes and signed JWTs.
// Every run owns one randomly named database; the configured application DB is never used.
public sealed class CharityApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public string DatabaseName { get; } = $"CharityQA_Integration_{Guid.NewGuid():N}";
    private string TestConnectionString => new SqlConnectionStringBuilder
    {
        DataSource = Environment.GetEnvironmentVariable("CHARITY_TEST_SQL_SERVER") ?? ".",
        InitialCatalog = DatabaseName,
        IntegratedSecurity = true,
        TrustServerCertificate = true,
        MultipleActiveResultSets = true
    }.ConnectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Error));
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<CharityDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<CharityDbContext>>();
            services.AddDbContext<CharityDbContext>(options => options.UseSqlServer(TestConnectionString));
        });
    }

    public async Task InitializeAsync()
    {
        using var client = CreateClient();
        await WithDb(async db =>
        {
            await db.Database.MigrateAsync();
            using var scope = Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await DbInitializer.SeedAsync(db, userManager, roleManager);
            await DbInitializer.SeedTestFixturesAsync(db);

            var adminUser = await userManager.FindByNameAsync("admin");
            if (adminUser != null)
            {
                adminUser.CharityId = "c1";
                await userManager.UpdateAsync(adminUser);
            }

            // Test-only accounts exercise tenant isolation; application seeding creates admin only.
            foreach (var sample in new[]
            {
                (Username: "amal", Password: "Amal@123", CharityId: "c2", Role: "CharityManager"),
                (Username: "researcher", Password: "Research@123", CharityId: "c1", Role: "Staff")
            })
            {
                if (await userManager.FindByNameAsync(sample.Username) != null) continue;
                var user = new ApplicationUser
                {
                    UserName = sample.Username, FullName = sample.Username,
                    CharityId = sample.CharityId, EmailConfirmed = true
                };
                var created = await userManager.CreateAsync(user, sample.Password);
                Assert.True(created.Succeeded);
                var assigned = await userManager.AddToRoleAsync(user, sample.Role);
                Assert.True(assigned.Succeeded);

                if (sample.Role == "Staff")
                {
                    var permissions = new[]
                    {
                        Charity.Domain.Security.AppPermissions.BeneficiariesView,
                        Charity.Domain.Security.AppPermissions.BeneficiariesViewFull,
                        Charity.Domain.Security.AppPermissions.BeneficiariesCreate,
                        Charity.Domain.Security.AppPermissions.BeneficiariesEdit,
                        Charity.Domain.Security.AppPermissions.InventoryView,
                        Charity.Domain.Security.AppPermissions.InventoryManage,
                        Charity.Domain.Security.AppPermissions.DisbursementExecute,
                        Charity.Domain.Security.AppPermissions.DisbursementView,
                        Charity.Domain.Security.AppPermissions.ExpensesManage
                    };
                    foreach (var p in permissions)
                    {
                        await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("permission", p));
                    }
                }
            }

            Assert.Equal(DatabaseName, db.Database.GetDbConnection().Database);
            Assert.Empty(await db.Database.GetPendingMigrationsAsync());
            Assert.True(await db.Users.AnyAsync(), "Startup must seed real Identity users in the disposable DB.");
            return true;
        });
    }

    public async Task<HttpClient> Login(string username = "admin", string password = "admin")
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto { Username = username, Password = password });
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(auth);
        Assert.False(string.IsNullOrWhiteSpace(auth.Token));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        return client;
    }

    public async Task<T> WithDb<T>(Func<CharityDbContext, Task<T>> action)
    {
        using var scope = Services.CreateScope();
        return await action(scope.ServiceProvider.GetRequiredService<CharityDbContext>());
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await WithDb(async db =>
        {
            var database = db.Database.GetDbConnection().Database;
            if (database != DatabaseName || !database.StartsWith("CharityQA_Integration_", StringComparison.Ordinal))
                throw new InvalidOperationException("Refusing to delete a database not owned by this test run.");
            return await db.Database.EnsureDeletedAsync();
        });
        await base.DisposeAsync();
    }
}

[CollectionDefinition("API", DisableParallelization = true)]
public sealed class ApiCollection : ICollectionFixture<CharityApiFactory> { }
