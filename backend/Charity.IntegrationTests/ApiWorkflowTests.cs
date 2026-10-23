using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Charity.Application.DTOs.Auth;
using Charity.Application.DTOs.Beneficiaries;
using Charity.Application.DTOs.Dashboard;
using Charity.Application.DTOs.Distributions;
using Charity.Application.DTOs.Donations;
using Charity.Application.DTOs.Inventory;
using Charity.Application.DTOs.Staff;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Charity.Domain.Entities;
using Charity.Infrastructure.Data.Context;
using Charity.Infrastructure.Data.Seed;

namespace Charity.IntegrationTests;

[Collection("API")]
public sealed class ApiWorkflowTests(CharityApiFactory app)
{
    private static long _identity = 29001010100000;
    private static string NationalId() => Interlocked.Increment(ref _identity).ToString();

    private static async Task<T> Read<T>(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"HTTP {(int)response.StatusCode}: {body}");
        return JsonSerializer.Deserialize<T>(body, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
    }

    private static async Task<InventoryItemDto> Item(HttpClient client, double quantity = 10, string kind = "صنف ملموس") =>
        await Read<InventoryItemDto>(await client.PostAsJsonAsync("/api/inventory", new CreateInventoryItemDto
        { ItemName = "QA rice " + Guid.NewGuid().ToString("N"), Kind = kind, Quantity = quantity, Unit = "كيلو", MinAlert = 2 }));

    private static async Task<BeneficiaryDto> Family(HttpClient client, bool withMember = false) =>
        await Read<BeneficiaryDto>(await client.PostAsJsonAsync("/api/beneficiaries", new CreateBeneficiaryDto
        {
            Name = "QA household " + Guid.NewGuid().ToString("N"), NationalId = NationalId(),
            FamilyCount = withMember ? 2 : 1,
            FamilyMembers = withMember ? [new CreateFamilyMemberDto { Name = "QA dependent", NationalId = NationalId() }] : []
        }));

    [Theory]
    [InlineData("/api/beneficiaries")]
    [InlineData("/api/inventory")]
    [InlineData("/api/donations")]
    [InlineData("/api/distributions/my-charity")]
    [InlineData("/api/staff")]
    [InlineData("/api/analytics/bi")]
    [InlineData("/api/charities/my-settings")]
    [InlineData("/api/charities")]
    public async Task Protected_data_requires_authentication(string path)
    {
        using var client = app.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(path)).StatusCode);
    }

    [Fact]
    public async Task Real_identity_login_and_JWT_resolve_the_current_tenant()
    {
        using var client = await app.Login("amal", "Amal@123");
        var user = await Read<CurrentUserDto>(await client.GetAsync("/api/auth/me"));
        Assert.Equal("amal", user.Username);
        Assert.Equal("c2", user.CharityId);
        Assert.Equal("CharityManager", user.Role);
        var charities = await Read<List<CharityDto>>(await client.GetAsync("/api/charities"));
        Assert.Single(charities);
        Assert.Equal("c2", charities[0].Id);
    }

    [Theory]
    [InlineData("admin", "wrong-password")]
    [InlineData("non-existent-user", "admin")]
    [InlineData("admin", "Admin@123")]
    public async Task Invalid_credentials_are_rejected(string username, string password)
    {
        using var client = app.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { username, password });
        Assert.True(response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Demo_admin_password_is_stored_in_identity()
    {
        using var client = app.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { username = "admin", password = "admin" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await app.WithDb(db => db.Users.SingleAsync(u => u.UserName == "admin"));
        Assert.NotEqual("admin", user.PasswordHash);
    }

    [Fact]
    public async Task Tampered_bearer_token_is_rejected()
    {
        using var client = await app.Login();
        var token = client.DefaultRequestHeaders.Authorization!.Parameter!;
        var parts = token.Split('.');
        parts[2] = (parts[2][0] == 'A' ? "B" : "A") + parts[2][1..];
        client.DefaultRequestHeaders.Authorization = new("Bearer", string.Join('.', parts));
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode);
    }

    [Fact]
    public async Task Inventory_is_tenant_scoped_for_list_read_update_adjust_and_delete()
    {
        using var owner = await app.Login();
        using var other = await app.Login("amal", "Amal@123");
        var item = await Item(owner);
        var items = await Read<List<InventoryItemDto>>(await other.GetAsync("/api/inventory"));
        Assert.DoesNotContain(items, i => i.Id == item.Id);
        Assert.All(items, i => Assert.Equal("c2", i.CharityId));
        Assert.Equal(HttpStatusCode.NotFound, (await other.GetAsync($"/api/inventory/{item.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.PutAsJsonAsync($"/api/inventory/{item.Id}", new UpdateInventoryItemDto { ItemName = "tampered", Quantity = 100 })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.PostAsJsonAsync($"/api/inventory/{item.Id}/adjust-stock", new AdjustStockDto { NewQuantity = 100 })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.DeleteAsync($"/api/inventory/{item.Id}")).StatusCode);
        Assert.Equal(10, (await Read<InventoryItemDto>(await owner.GetAsync($"/api/inventory/{item.Id}"))).Quantity);
    }

    [Fact]
    public async Task Donation_inventory_distribution_lifecycle_persists_consistent_stock_and_shared_history()
    {
        using var owner = await app.Login();
        using var other = await app.Login("amal", "Amal@123");
        var item = await Item(owner, 10);
        var family = await Family(owner);
        var donation = await Read<DonationDto>(await owner.PostAsJsonAsync("/api/donations", new CreateDonationDto
        { DonorName = "QA donor", DonationKind = "عيني", InventoryItemId = item.Id, Quantity = 5 }));
        Assert.Equal("c1", donation.CharityId);
        Assert.Equal(15, (await Read<InventoryItemDto>(await owner.GetAsync($"/api/inventory/{item.Id}"))).Quantity);
        var distribution = await Read<DistributionDto>(await owner.PostAsJsonAsync("/api/distributions", new CreateDistributionDto
        { BeneficiaryId = family.Id, InventoryItemId = item.Id, Quantity = 4 }));
        Assert.Equal("c1", distribution.CharityId);
        Assert.Equal(11, (await Read<InventoryItemDto>(await owner.GetAsync($"/api/inventory/{item.Id}"))).Quantity);
        var summary = await Read<BeneficiaryAidSummaryDto>(await other.GetAsync($"/api/beneficiaries/{family.Id}/aid-history"));
        Assert.Equal(1, summary.OtherCharitiesDistributionsCount);
        Assert.Contains(summary.AidHistory, d => d.Id == distribution.Id && !d.IsFromCurrentCharity);
        Assert.DoesNotContain(await Read<List<DonationDto>>(await other.GetAsync("/api/donations")), d => d.Id == donation.Id);
        Assert.DoesNotContain(await Read<List<DistributionDto>>(await other.GetAsync("/api/distributions/my-charity")), d => d.Id == distribution.Id);
    }

    [Fact]
    public async Task Insufficient_stock_requires_confirmation_without_partial_writes()
    {
        using var client = await app.Login();
        var item = await Item(client, 2);
        var family = await Family(client);
        var response = await client.PostAsJsonAsync("/api/distributions", new CreateDistributionDto
        { BeneficiaryId = family.Id, InventoryItemId = item.Id, Quantity = 3 });
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(2, (await Read<InventoryItemDto>(await client.GetAsync($"/api/inventory/{item.Id}"))).Quantity);
        Assert.Empty(await Read<List<DistributionDto>>(await client.GetAsync($"/api/distributions/beneficiary/{family.Id}")));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public async Task Nonpositive_distribution_quantity_is_rejected_without_changing_stock(double quantity)
    {
        using var client = await app.Login();
        var item = await Item(client);
        var family = await Family(client);
        var response = await client.PostAsJsonAsync("/api/distributions", new CreateDistributionDto
        { BeneficiaryId = family.Id, InventoryItemId = item.Id, Quantity = quantity });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(10, (await Read<InventoryItemDto>(await client.GetAsync($"/api/inventory/{item.Id}"))).Quantity);
    }

    [Fact]
    public async Task Donation_cannot_reference_another_charitys_inventory()
    {
        using var owner = await app.Login();
        using var other = await app.Login("amal", "Amal@123");
        var item = await Item(owner);
        var response = await other.PostAsJsonAsync("/api/donations", new CreateDonationDto
        { DonorName = "QA foreign donor", DonationKind = "عيني", InventoryItemId = item.Id, Quantity = 5 });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(10, (await Read<InventoryItemDto>(await owner.GetAsync($"/api/inventory/{item.Id}"))).Quantity);
        Assert.False(await app.WithDb(db => db.Donations.AnyAsync(d => d.InventoryItemId == item.Id)));
    }

    [Fact]
    public async Task Cash_donation_does_not_retain_a_stale_inventory_reference()
    {
        using var client = await app.Login();
        var item = await Item(client);
        var donation = await Read<DonationDto>(await client.PostAsJsonAsync("/api/donations", new CreateDonationDto
        { DonorName = "QA donor", DonationKind = "نقدي", Amount = 50, InventoryItemId = item.Id }));
        Assert.Null(donation.InventoryItemId);
        Assert.Equal(10, (await Read<InventoryItemDto>(await client.GetAsync($"/api/inventory/{item.Id}"))).Quantity);
    }

    [Fact]
    public async Task Distribution_cannot_reference_another_charitys_inventory()
    {
        using var owner = await app.Login();
        using var other = await app.Login("amal", "Amal@123");
        var item = await Item(owner);
        var family = await Family(owner);
        var response = await other.PostAsJsonAsync("/api/distributions", new CreateDistributionDto
        { BeneficiaryId = family.Id, InventoryItemId = item.Id, Quantity = 1 });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(10, (await Read<InventoryItemDto>(await owner.GetAsync($"/api/inventory/{item.Id}"))).Quantity);
    }

    [Fact]
    public async Task Missing_inventory_reference_returns_not_found_instead_of_database_error()
    {
        using var client = await app.Login();
        var family = await Family(client);
        var response = await client.PostAsJsonAsync("/api/distributions", new CreateDistributionDto
        { BeneficiaryId = family.Id, InventoryItemId = Guid.NewGuid().ToString(), Quantity = 1 });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task Nonpositive_cash_donation_is_rejected(decimal amount)
    {
        using var client = await app.Login();
        var response = await client.PostAsJsonAsync("/api/donations", new CreateDonationDto
        { DonorName = "QA donor", DonationKind = "نقدي", Amount = amount });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Negative_stock_adjustment_is_rejected()
    {
        using var client = await app.Login();
        var item = await Item(client);
        var response = await client.PostAsJsonAsync($"/api/inventory/{item.Id}/adjust-stock", new AdjustStockDto { NewQuantity = -1 });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(10, (await Read<InventoryItemDto>(await client.GetAsync($"/api/inventory/{item.Id}"))).Quantity);
    }

    [Fact]
    public async Task Inventory_delete_is_soft_and_prevents_subsequent_distribution()
    {
        using var client = await app.Login();
        var item = await Item(client);
        var family = await Family(client);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/inventory/{item.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/inventory/{item.Id}")).StatusCode);
        var saved = await app.WithDb(db => db.InventoryItems.IgnoreQueryFilters().SingleAsync(i => i.Id == item.Id));
        Assert.True(saved.IsDeleted);
        Assert.NotNull(saved.DeletedAt);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsJsonAsync("/api/distributions", new CreateDistributionDto
        { BeneficiaryId = family.Id, InventoryItemId = item.Id, Quantity = 1 })).StatusCode);
    }

    [Fact]
    public async Task Shared_beneficiary_is_readable_but_only_owner_can_edit_suspend_or_delete()
    {
        using var owner = await app.Login();
        using var other = await app.Login("amal", "Amal@123");
        var family = await Family(owner);
        var shared = await Read<BeneficiaryDto>(await other.GetAsync($"/api/beneficiaries/{family.Id}"));
        Assert.False(shared.IsEditableByCurrentCharity);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.PutAsJsonAsync($"/api/beneficiaries/{family.Id}", new UpdateBeneficiaryDto
        { Name = family.Name, NationalId = family.NationalId, FamilyCount = 1 })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.PostAsJsonAsync($"/api/beneficiaries/{family.Id}/suspend", new SuspendBeneficiaryDto
        { SuspensionReasonTitle = "QA reason" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.DeleteAsync($"/api/beneficiaries/{family.Id}")).StatusCode);
    }

    [Fact]
    public async Task Duplicate_national_identity_is_rejected_without_second_beneficiary()
    {
        using var client = await app.Login();
        var family = await Family(client);
        var response = await client.PostAsJsonAsync("/api/beneficiaries", new CreateBeneficiaryDto { Name = "Duplicate", NationalId = family.NationalId });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(1, await app.WithDb(db => db.Beneficiaries.CountAsync(b => b.NationalId == family.NationalId)));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Adding_a_member_rejects_an_identity_in_another_active_file(bool existingDependent, bool otherCharity)
    {
        using var owner = await app.Login();
        using var targetClient = otherCharity ? await app.Login("amal", "Amal@123") : await app.Login();
        var source = await Family(owner, withMember: existingDependent);
        var target = await Family(targetClient);
        var nationalId = existingDependent ? Assert.Single(source.FamilyMembers).NationalId : source.NationalId;

        var response = await targetClient.PostAsJsonAsync($"/api/beneficiaries/{target.Id}/members",
            new CreateFamilyMemberDto { Name = "QA duplicate identity", NationalId = $" {nationalId} " });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains("مسجل مسبقاً", error.GetProperty("message").GetString());
        Assert.False(await app.WithDb(db => db.BeneficiaryFamilyMembers.AnyAsync(m => m.BeneficiaryId == target.Id)));
        var persisted = await Read<BeneficiaryDto>(await targetClient.GetAsync($"/api/beneficiaries/{target.Id}"));
        Assert.Equal(target.FamilyCount, persisted.FamilyCount);
        Assert.Equal(source.NationalId, (await Read<BeneficiaryDto>(await owner.GetAsync($"/api/beneficiaries/{source.Id}"))).NationalId);
    }

    [Fact]
    public async Task Adding_a_member_with_an_unused_identity_persists_normally()
    {
        using var client = await app.Login();
        var family = await Family(client);
        var nationalId = NationalId();
        var member = await Read<FamilyMemberDto>(await client.PostAsJsonAsync($"/api/beneficiaries/{family.Id}/members",
            new CreateFamilyMemberDto { Name = "QA new dependent", NationalId = nationalId }));

        var persisted = await Read<BeneficiaryDto>(await client.GetAsync($"/api/beneficiaries/{family.Id}"));
        Assert.Equal(member.Id, Assert.Single(persisted.FamilyMembers).Id);
        Assert.Equal(nationalId, persisted.FamilyMembers[0].NationalId);
        Assert.Equal(2, persisted.FamilyCount);
    }

    [Fact]
    public async Task Editing_an_existing_family_member_preserves_identity_without_EF_tracking_errors()
    {
        using var client = await app.Login();
        var family = await Family(client, withMember: true);
        var member = Assert.Single(family.FamilyMembers);
        var updated = await Read<BeneficiaryDto>(await client.PutAsJsonAsync($"/api/beneficiaries/{family.Id}", new UpdateBeneficiaryDto
        {
            Name = family.Name, NationalId = family.NationalId, FamilyCount = 2, Status = "مستحق فعال",
            FamilyMembers = [new CreateFamilyMemberDto { Id = member.Id, Name = "QA updated dependent", NationalId = member.NationalId }]
        }));
        Assert.Equal(member.Id, Assert.Single(updated.FamilyMembers).Id);
        Assert.Equal("QA updated dependent", Assert.Single(updated.FamilyMembers).Name);
        Assert.Equal(1, await app.WithDb(db => db.BeneficiaryFamilyMembers.CountAsync(m => m.BeneficiaryId == family.Id)));
    }

    [Fact]
    public async Task Suspension_blocks_aid_and_reactivation_restores_it()
    {
        using var client = await app.Login();
        var family = await Family(client);
        var suspended = await Read<BeneficiaryDto>(await client.PostAsJsonAsync($"/api/beneficiaries/{family.Id}/suspend", new SuspendBeneficiaryDto
        { SuspensionReasonTitle = "QA review" }));
        Assert.True(suspended.IsSuspended);
        var categories = await Read<List<Charity.Application.DTOs.Lookups.ExpenseCategoryDto>>(await client.GetAsync("/api/expense-categories"));
        var aid = new CreateDistributionDto { BeneficiaryId = family.Id, Kind = "إعانة نقدية", ItemName = "QA cash aid", Amount = 100, Quantity = 1,
            ExpenseCategoryId = categories.First().Id, AllowNegativeBalance = true };
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/distributions", aid)).StatusCode);
        var active = await Read<BeneficiaryDto>(await client.PostAsync($"/api/beneficiaries/{family.Id}/reactivate", null));
        Assert.False(active.IsSuspended);
        await Read<DistributionDto>(await client.PostAsJsonAsync("/api/distributions", aid));
    }

    [Fact]
    public async Task Cash_donation_updates_admin_dashboard_while_charity_user_is_denied()
    {
        using var owner = await app.Login();
        using var other = await app.Login("amal", "Amal@123");
        var before = await Read<DashboardStatsDto>(await owner.GetAsync("/api/dashboard/stats"));
        Assert.Equal(HttpStatusCode.Forbidden, (await other.GetAsync("/api/dashboard/stats")).StatusCode);
        await Read<DonationDto>(await owner.PostAsJsonAsync("/api/donations", new CreateDonationDto
        { DonorName = "QA cash donor", DonationKind = "نقدي", Amount = 123.45m }));
        var after = await Read<DashboardStatsDto>(await owner.GetAsync("/api/dashboard/stats"));
        Assert.Equal(before.TotalCashDonations + 123.45m, after.TotalCashDonations);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.GetAsync("/api/analytics/bi")).StatusCode);
    }

    [Fact]
    public async Task Staff_CRUD_is_tenant_scoped_and_soft_deleted()
    {
        using var owner = await app.Login();
        using var other = await app.Login("amal", "Amal@123");
        var staff = await Read<StaffMemberDto>(await owner.PostAsJsonAsync("/api/staff", new CreateStaffMemberDto
        { Name = "QA volunteer", NationalId = NationalId(), Phone = "01012345678" }));
        Assert.Equal("c1", staff.CharityId);
        Assert.Equal(HttpStatusCode.NotFound, (await other.GetAsync($"/api/staff/{staff.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.DeleteAsync($"/api/staff/{staff.Id}")).StatusCode);
        var updated = await Read<StaffMemberDto>(await owner.PutAsJsonAsync($"/api/staff/{staff.Id}", new UpdateStaffMemberDto
        { Name = "QA senior volunteer", NationalId = staff.NationalId, Phone = staff.Phone,
          Role = staff.Role, JoinDate = staff.JoinDate }));
        Assert.Equal("QA senior volunteer", updated.Name);
        Assert.Equal(HttpStatusCode.NoContent, (await owner.DeleteAsync($"/api/staff/{staff.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await owner.GetAsync($"/api/staff/{staff.Id}")).StatusCode);
        Assert.True(await app.WithDb(db => db.StaffMembers.IgnoreQueryFilters().AnyAsync(s => s.Id == staff.Id && s.IsDeleted)));
    }

    [Fact]
    public async Task Settings_payload_cannot_change_another_tenant()
    {
        using var owner = await app.Login();
        using var other = await app.Login("amal", "Amal@123");
        var otherBefore = await Read<CharitySettingsDto>(await other.GetAsync("/api/charities/my-settings"));
        var changed = await Read<CharitySettingsDto>(await owner.PutAsJsonAsync("/api/charities/my-settings", new CharitySettingsDto
        { CharityId = "c2", CharityName = "QA current charity", Phone = "01000000000", Details = "QA isolated settings" }));
        Assert.Equal("c1", changed.CharityId);
        var otherAfter = await Read<CharitySettingsDto>(await other.GetAsync("/api/charities/my-settings"));
        Assert.Equal(otherBefore.CharityName, otherAfter.CharityName);
        Assert.Equal(otherBefore.Phone, otherAfter.Phone);
    }

    [Fact]
    public async Task Analytics_executes_real_SQL_and_returns_a_report()
    {
        using var client = await app.Login();
        var report = await Read<JsonElement>(await client.GetAsync("/api/analytics/bi"));
        Assert.Equal("شبكة الجمعيات بالكامل", report.GetProperty("charityName").GetString());
        var c2 = await Read<JsonElement>(await client.GetAsync("/api/analytics/bi?charityId=c2"));
        Assert.Equal("c2", c2.GetProperty("charityId").GetString());
    }

    [Fact]
    public async Task Representative_records_and_staff_accounts_survive_reseeding_without_duplicates()
    {
        using var client = await app.Login("researcher", "Research@123");
        var me = await Read<CurrentUserDto>(await client.GetAsync("/api/auth/me"));
        Assert.Equal("c1", me.CharityId);
        Assert.Equal("Staff", me.Role);

        var before = await app.WithDb(async db => new
        {
            Families = await db.Beneficiaries.CountAsync(x => x.Id.StartsWith("demo-family-")),
            Donations = await db.Donations.CountAsync(x => x.Id.StartsWith("demo-donation-")),
            Staff = await db.StaffMembers.CountAsync(x => x.Id.StartsWith("demo-staff-")),
            PasswordHash = (await db.Users.SingleAsync(u => u.UserName == "researcher")).PasswordHash
        });
        Assert.Equal(6, before.Families);
        Assert.Equal(7, before.Donations);
        Assert.Equal(3, before.Staff);

        using (var scope = app.Services.CreateScope())
            await DbInitializer.SeedAsync(
                scope.ServiceProvider.GetRequiredService<CharityDbContext>(),
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
                scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>());

        var after = await app.WithDb(async db => new
        {
            Families = await db.Beneficiaries.CountAsync(x => x.Id.StartsWith("demo-family-")),
            Donations = await db.Donations.CountAsync(x => x.Id.StartsWith("demo-donation-")),
            Staff = await db.StaffMembers.CountAsync(x => x.Id.StartsWith("demo-staff-")),
            PasswordHash = (await db.Users.SingleAsync(u => u.UserName == "researcher")).PasswordHash
        });
        Assert.Equal(before.Families, after.Families);
        Assert.Equal(before.Donations, after.Donations);
        Assert.Equal(before.Staff, after.Staff);
        Assert.Equal(before.PasswordHash, after.PasswordHash);
        Assert.NotEqual("Research@123", after.PasswordHash);
    }

    [Fact]
    public async Task Catalog_edit_without_quantity_keeps_persisted_stock()
    {
        using var client = await app.Login();
        var item = await Item(client, 17);
        var changed = await Read<InventoryItemDto>(await client.PutAsJsonAsync($"/api/inventory/{item.Id}",
            new { itemName = "QA renamed item", kind = "صنف ملموس", unit = "كيلو", type = "أغذية جافة", minAlert = 3 }));
        Assert.Equal(17, changed.Quantity);
        Assert.Equal(17, (await Read<InventoryItemDto>(await client.GetAsync($"/api/inventory/{item.Id}"))).Quantity);
    }

    [Fact]
    public async Task Charity_user_cannot_read_analytics_even_with_a_charity_query()
    {
        using var client = await app.Login("amal", "Amal@123");
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/analytics/bi?charityId=c1")).StatusCode);
    }

    [Fact]
    public async Task Only_admin_can_create_charity_with_an_isolated_primary_user()
    {
        using var manager = await app.Login("amal", "Amal@123");
        var request = new CreateCharityDto
        {
            Name = "جمعية اختبار " + Guid.NewGuid().ToString("N"),
            ManagerUsername = "qa" + Guid.NewGuid().ToString("N")[..12],
            ManagerPassword = "Secure@123", ManagerFullName = "مدير الاختبار",
            Address = "القاهرة", Phone = "01000000000", RoleDesc = "رعاية أسر"
        };
        Assert.Equal(HttpStatusCode.Forbidden, (await manager.PostAsJsonAsync("/api/charities", request)).StatusCode);

        using var admin = await app.Login();
        var beforeUsers = await app.WithDb(db => db.Users.CountAsync());
        var created = await Read<CharityDto>(await admin.PostAsJsonAsync("/api/charities", request));
        Assert.Equal(request.Name, created.Name);
        Assert.True(await app.WithDb(db => db.Charities.AnyAsync(c => c.Id == created.Id)));
        using var newManager = await app.Login(request.ManagerUsername, request.ManagerPassword);
        var me = await Read<CurrentUserDto>(await newManager.GetAsync("/api/auth/me"));
        Assert.Equal(created.Id, me.CharityId);
        Assert.Equal("CharityManager", me.Role);
        var donation = await Read<DonationDto>(await newManager.PostAsJsonAsync("/api/donations", new CreateDonationDto
        { DonorName = "QA network admin", DonationKind = "نقدي", Amount = 10 }));
        Assert.Equal(created.Id, donation.CharityId);
        Assert.Equal(HttpStatusCode.Forbidden, (await newManager.GetAsync("/api/dashboard/stats")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await newManager.GetAsync("/api/analytics/bi")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await newManager.PostAsJsonAsync("/api/charities", request)).StatusCode);
        Assert.Equal(beforeUsers + 1, await app.WithDb(db => db.Users.CountAsync()));
    }

    [Fact]
    public async Task Charity_creation_rejects_empty_or_duplicate_name_without_orphan_records()
    {
        using var admin = await app.Login();
        var name = "جمعية اختبار " + Guid.NewGuid().ToString("N");
        var before = await app.WithDb(db => db.Charities.CountAsync());
        var invalid = new CreateCharityDto
        {
            Name = "  ", ManagerUsername = "qa" + Guid.NewGuid().ToString("N")[..12],
            ManagerPassword = "Secure@123", ManagerFullName = "مدير الاختبار"
        };
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.PostAsJsonAsync("/api/charities", invalid)).StatusCode);
        Assert.Equal(before, await app.WithDb(db => db.Charities.CountAsync()));

        invalid.Name = name;
        await Read<CharityDto>(await admin.PostAsJsonAsync("/api/charities", invalid));
        var after = await app.WithDb(db => db.Charities.CountAsync());
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.PostAsJsonAsync("/api/charities", invalid)).StatusCode);
        Assert.Equal(after, await app.WithDb(db => db.Charities.CountAsync()));
    }

    [Fact]
    public async Task Admin_can_assign_one_primary_user_to_an_existing_charity()
    {
        var request = new CreateCharityManagerDto
        {
            Username = "sedakaqa" + Guid.NewGuid().ToString("N")[..8],
            Password = "Secure@123", FullName = "مسؤول الجمعية"
        };
        using var admin = await app.Login();
        Assert.Equal(HttpStatusCode.Created,
            (await admin.PostAsJsonAsync("/api/charities/c3/manager", request)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await admin.PostAsJsonAsync("/api/charities/c3/manager", new CreateCharityManagerDto
            { Username = "other" + Guid.NewGuid().ToString("N")[..8], Password = "Secure@123", FullName = "آخر" })).StatusCode);

        using var manager = await app.Login(request.Username, request.Password);
        manager.DefaultRequestHeaders.Add("X-Charity-Id", "c1");
        var me = await Read<CurrentUserDto>(await manager.GetAsync("/api/auth/me"));
        Assert.Equal("c3", me.CharityId);
        var donation = await Read<DonationDto>(await manager.PostAsJsonAsync("/api/donations", new CreateDonationDto
        { DonorName = "متبرع الجمعية الثالثة", DonationKind = "نقدي", Amount = 15 }));
        Assert.Equal("c3", donation.CharityId);
        Assert.Equal(HttpStatusCode.Forbidden, (await manager.GetAsync("/api/analytics/bi")).StatusCode);
    }

    [Fact]
    public async Task Deleting_a_user_revokes_their_existing_token()
    {
        var username = "deleted" + Guid.NewGuid().ToString("N")[..10];
        using (var scope = app.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var created = await users.CreateAsync(new ApplicationUser
            { UserName = username, FullName = "اختبار حذف", CharityId = "c1" }, "Secure@123");
            Assert.True(created.Succeeded);
            var user = await users.FindByNameAsync(username);
            Assert.NotNull(user);
            Assert.True((await users.AddToRoleAsync(user, "CharityManager")).Succeeded);
        }

        using var client = await app.Login(username, "Secure@123");
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/auth/me")).StatusCode);
        using (var scope = app.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await users.FindByNameAsync(username);
            Assert.NotNull(user);
            Assert.True((await users.DeleteAsync(user)).Succeeded);
        }
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode);
    }

    [Fact]
    public async Task Headship_transfer_supports_multiple_sequential_transfers_without_unique_constraint_violation()
    {
        using var client = await app.Login();

        var originalHeadNid = NationalId();
        var wifeNid = NationalId();
        var sonNid = NationalId();

        var createDto = new CreateBeneficiaryDto
        {
            Name = "عائل الأسرة الأول " + Guid.NewGuid().ToString("N")[..6],
            NationalId = originalHeadNid,
            FamilyName = "عائلة فلان",
            FamilyCount = 3,
            FamilyMembers =
            [
                new CreateFamilyMemberDto { Name = "الزوجة", NationalId = wifeNid, Relationship = "زوجة", MemberType = "معال" },
                new CreateFamilyMemberDto { Name = "الابن الأكبر", NationalId = sonNid, Relationship = "ابن", MemberType = "معيل" }
            ]
        };

        var family = await Read<BeneficiaryDto>(await client.PostAsJsonAsync("/api/beneficiaries", createDto));
        Assert.Equal(2, family.FamilyMembers.Count);
        var wifeMember = family.FamilyMembers.First(m => m.NationalId == wifeNid);

        // 1. Transfer to Wife (first transfer)
        var transferToWife = new TransferHeadshipDto
        {
            NewHeadFamilyMemberId = wifeMember.Id,
            Reason = "وفاة رب الأسرة الأول",
            FormerHeadRelationship = "رب الأسرة السابق (متوفى)",
            NewFamilyName = "عائلة المرحوم عائل الأسرة الأول"
        };
        var res1 = await client.PostAsJsonAsync($"/api/beneficiaries/{family.Id}/transfer-headship", transferToWife);
        var familyAfterWife = await Read<BeneficiaryDto>(res1);

        Assert.Equal(wifeNid, familyAfterWife.NationalId);
        Assert.Equal("الزوجة", familyAfterWife.Name);
        Assert.Single(familyAfterWife.HeadHistories);
        Assert.Equal(originalHeadNid, familyAfterWife.HeadHistories[0].PreviousHeadNationalId);
        Assert.Equal(wifeNid, familyAfterWife.HeadHistories[0].NewHeadNationalId);

        // 2. Transfer to Son (second transfer - ensures former head wife does not collide with existing or deleted records)
        var sonMember = familyAfterWife.FamilyMembers.First(m => m.NationalId == sonNid);
        var transferToSon = new TransferHeadshipDto
        {
            NewHeadFamilyMemberId = sonMember.Id,
            Reason = "سفر الأم وانتقال الإعالة للابن الأكبر",
            FormerHeadRelationship = "الأم"
        };
        var res2 = await client.PostAsJsonAsync($"/api/beneficiaries/{family.Id}/transfer-headship", transferToSon);
        var familyAfterSon = await Read<BeneficiaryDto>(res2);

        Assert.Equal(sonNid, familyAfterSon.NationalId);
        Assert.Equal("الابن الأكبر", familyAfterSon.Name);
        Assert.Equal(2, familyAfterSon.HeadHistories.Count);

        // 3. Transfer BACK to Wife (third transfer - cyclic transfer ensures complete stability)
        var wifeMemberAgain = familyAfterSon.FamilyMembers.First(m => m.NationalId == wifeNid);
        var transferBackToWife = new TransferHeadshipDto
        {
            NewHeadFamilyMemberId = wifeMemberAgain.Id,
            Reason = "عودة الأم للإعالة",
            FormerHeadRelationship = "الابن"
        };
        var res3 = await client.PostAsJsonAsync($"/api/beneficiaries/{family.Id}/transfer-headship", transferBackToWife);
        var familyFinal = await Read<BeneficiaryDto>(res3);

        Assert.Equal(wifeNid, familyFinal.NationalId);
        Assert.Equal(3, familyFinal.HeadHistories.Count);
    }
}
