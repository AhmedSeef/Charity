using System.Net;
using System.Net.Http.Json;
using Charity.Application.DTOs.Audit;
using Charity.Application.DTOs.Lookups;
using Charity.Application.DTOs.Inventory;
using Charity.Application.DTOs.Donations;
using Charity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Charity.IntegrationTests;

[Collection("API")]
public sealed class AuditTrailTests(CharityApiFactory app)
{
    [Fact]
    public async Task ManagerSeesOnlyOwnCharityAndCreateUpdateDeleteCarryActor()
    {
        using var manager = await app.Login("amal", "Amal@123");
        using var otherEmployee = await app.Login("researcher", "Research@123");
        var name = "QA audit " + Guid.NewGuid().ToString("N");

        var create = await manager.PostAsJsonAsync("/api/expense-categories", new CreateExpenseCategoryDto { Name = name });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var category = await create.Content.ReadFromJsonAsync<ExpenseCategoryDto>();
        Assert.NotNull(category);
        var edit = await manager.PutAsJsonAsync($"/api/expense-categories/{category.Id}",
            new UpdateExpenseCategoryDto { Name = name + " edited", IsActive = true });
        Assert.Equal(HttpStatusCode.OK, edit.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await manager.DeleteAsync($"/api/expense-categories/{category.Id}")).StatusCode);

        var record = await app.WithDb(db => db.ExpenseCategories.IgnoreQueryFilters()
            .AsNoTracking().SingleAsync(x => x.Id == category.Id));
        Assert.False(string.IsNullOrWhiteSpace(record.CreatedBy));
        Assert.Equal("amal", record.CreatedByName);
        Assert.Equal(record.CreatedBy, record.UpdatedBy);
        Assert.Equal(record.CreatedBy, record.DeletedBy);
        Assert.NotNull(record.UpdatedAt);
        Assert.NotNull(record.DeletedAt);

        var response = await manager.GetAsync($"/api/audit-logs?name=amal&entityType=ExpenseCategory&pageSize=100");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuditLogPageDto>();
        Assert.NotNull(result);
        var events = result.Items.Where(x => x.EntityId == category.Id).ToList();
        Assert.Equal(3, events.Count);
        Assert.Equal(new[] { "Create", "Update", "Delete" }, events.OrderBy(x => x.OccurredAt).ThenBy(x => x.Id).Select(x => x.Action));
        Assert.All(events, x => Assert.Equal("amal", x.Username));
        var deletedOnly = await manager.GetFromJsonAsync<AuditLogPageDto>(
            "/api/audit-logs?name=amal&action=Delete&entityType=ExpenseCategory&from=2020-01-01&to=2030-12-31&pageSize=100");
        Assert.NotNull(deletedOnly);
        Assert.Contains(deletedOnly.Items, x => x.EntityId == category.Id && x.Action == "Delete");
        Assert.DoesNotContain(deletedOnly.Items, x => x.Action != "Delete");
        Assert.Equal(HttpStatusCode.BadRequest,
            (await manager.GetAsync("/api/audit-logs?from=2026-10-24&to=2026-10-23")).StatusCode);

        var itemResponse = await manager.PostAsJsonAsync("/api/inventory", new CreateInventoryItemDto
        { ItemName = "صنف اختبار التتبع", Kind = "صنف ملموس", Quantity = 2, Unit = "قطعة" });
        Assert.Equal(HttpStatusCode.Created, itemResponse.StatusCode);
        var item = await itemResponse.Content.ReadFromJsonAsync<InventoryItemDto>();
        Assert.NotNull(item);
        var donationResponse = await manager.PostAsJsonAsync("/api/donations", new CreateDonationDto
        { DonorName = "متبرع اختبار", DonationKind = "عيني", InventoryItemId = item.Id, Quantity = 3 });
        Assert.Equal(HttpStatusCode.OK, donationResponse.StatusCode);
        var stock = await app.WithDb(db => db.InventoryItems.AsNoTracking().SingleAsync(x => x.Id == item.Id));
        Assert.Equal(5, stock.Quantity);
        Assert.Equal("amal", stock.UpdatedByName);
        Assert.NotNull(stock.UpdatedAt);
        var stockEvents = await manager.GetFromJsonAsync<AuditLogPageDto>("/api/audit-logs?entityType=InventoryItem&pageSize=100");
        Assert.NotNull(stockEvents);
        Assert.Contains(stockEvents.Items, x => x.EntityId == item.Id && x.Action == "Update");

        Assert.Equal(HttpStatusCode.Forbidden, (await otherEmployee.GetAsync("/api/audit-logs")).StatusCode);
        using var otherManager = await app.Login();
        Assert.Equal(HttpStatusCode.Forbidden, (await otherManager.GetAsync("/api/audit-logs")).StatusCode);

        var otherName = "auditmanager" + Guid.NewGuid().ToString("N")[..10];
        using (var scope = app.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var other = new ApplicationUser { UserName = otherName, FullName = "مدير جمعية أخرى", CharityId = "c1" };
            Assert.True((await users.CreateAsync(other, "Manager@123")).Succeeded);
            Assert.True((await users.AddToRoleAsync(other, "CharityManager")).Succeeded);
        }
        using var differentCharity = await app.Login(otherName, "Manager@123");
        var isolated = await differentCharity.GetFromJsonAsync<AuditLogPageDto>("/api/audit-logs?entityType=ExpenseCategory&pageSize=100");
        Assert.NotNull(isolated);
        Assert.DoesNotContain(isolated.Items, x => x.EntityId == category.Id);
    }
}
