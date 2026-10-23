using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Charity.Application.DTOs.Beneficiaries;
using Charity.Application.DTOs.Distributions;
using Charity.Application.DTOs.Inventory;
using Charity.Application.DTOs.Lookups;
using Charity.Application.DTOs.SuspensionReasons;
using Charity.Application.DTOs.SeparationReasons;
using Charity.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;

namespace Charity.IntegrationTests;

[Collection("API")]
public sealed class TenantIsolationTests(CharityApiFactory app)
{
    private static int _serial = 70000;
    private static string Identity() => "278010101" + Interlocked.Increment(ref _serial).ToString("D5");
    private static async Task<T> Read<T>(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"HTTP {(int)response.StatusCode}: {body}");
        return JsonSerializer.Deserialize<T>(body, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
    }
    private static async Task<BeneficiaryDto> Family(HttpClient client, bool member = false) => await Read<BeneficiaryDto>(
        await client.PostAsJsonAsync("/api/beneficiaries", new CreateBeneficiaryDto
        { Name = "QA shared Ahmed", NationalId = Identity(), FamilyMembers = member ? [new CreateFamilyMemberDto { Name = "QA child", NationalId = Identity() }] : [] }));
    private static async Task<InventoryItemDto> Stock(HttpClient client) => await Read<InventoryItemDto>(
        await client.PostAsJsonAsync("/api/inventory", new CreateInventoryItemDto { ItemName = "QA private stock", Quantity = 10, Kind = "صنف ملموس" }));

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Simultaneous_registration_in_different_charities_cannot_duplicate_the_identity(bool partnerRegistersMember)
    {
        using var owner = await app.Login("researcher", "Research@123");
        using var partner = await app.Login("amal", "Amal@123");
        var target = await Family(partner);
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var id = Identity();
            var responses = await Task.WhenAll(
                owner.PostAsJsonAsync("/api/beneficiaries", new CreateBeneficiaryDto { Name = "QA racing head", NationalId = id }),
                partnerRegistersMember
                    ? partner.PostAsJsonAsync($"/api/beneficiaries/{target.Id}/members", new CreateFamilyMemberDto { Name = "QA racing member", NationalId = id })
                    : partner.PostAsJsonAsync("/api/beneficiaries", new CreateBeneficiaryDto { Name = "QA racing head", NationalId = id }));
            Assert.Single(responses, response => response.IsSuccessStatusCode);
            Assert.Single(responses, response => response.StatusCode == HttpStatusCode.BadRequest);
            var registrations = await app.WithDb(async db => await db.Beneficiaries.CountAsync(b => b.NationalId == id)
                + await db.BeneficiaryFamilyMembers.CountAsync(m => m.NationalId == id));
            Assert.Equal(1, registrations);
        }
    }

    [Fact]
    public async Task Platform_admin_cannot_modify_a_file_registered_by_a_different_charity()
    {
        using var owner = await app.Login("amal", "Amal@123");
        using var admin = await app.Login();
        var family = await Family(owner);
        Assert.Equal(HttpStatusCode.Forbidden, (await admin.PutAsJsonAsync($"/api/beneficiaries/{family.Id}", new { name = "tampered" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await admin.DeleteAsync($"/api/beneficiaries/{family.Id}")).StatusCode);
    }

    [Fact]
    public async Task All_charities_see_Ahmed_and_shared_history_but_only_the_registering_charity_can_change_his_file()
    {
        using var owner = await app.Login("researcher", "Research@123");
        using var partner = await app.Login("amal", "Amal@123");
        var family = await Family(owner, true);
        var ownedByPartner = await Family(partner);
        var visible = await Read<List<BeneficiaryDto>>(await partner.GetAsync("/api/beneficiaries"));
        Assert.Contains(visible, b => b.Id == family.Id);
        Assert.Contains(visible, b => b.Id == ownedByPartner.Id);
        Assert.False(visible.Single(b => b.Id == family.Id).IsEditableByCurrentCharity);
        Assert.True(visible.Single(b => b.Id == ownedByPartner.Id).IsEditableByCurrentCharity);
        Assert.Equal(HttpStatusCode.OK, (await partner.GetAsync($"/api/beneficiaries/{family.Id}")).StatusCode);

        Assert.Equal(HttpStatusCode.Forbidden, (await partner.PutAsJsonAsync($"/api/beneficiaries/{family.Id}", new { name = "tampered" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await partner.DeleteAsync($"/api/beneficiaries/{family.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await partner.PostAsJsonAsync($"/api/beneficiaries/{family.Id}/suspend", new { suspensionReasonTitle = "tampered" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await partner.PostAsync($"/api/beneficiaries/{family.Id}/reactivate", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await partner.PostAsJsonAsync($"/api/beneficiaries/{family.Id}/members", new { name = "tampered", nationalId = Identity() })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await partner.PostAsJsonAsync($"/api/beneficiaries/{family.Id}/members/separate", new { familyMemberId = family.FamilyMembers[0].Id })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await partner.PostAsJsonAsync($"/api/beneficiaries/{family.Id}/transfer-headship", new { newHeadFamilyMemberId = family.FamilyMembers[0].Id })).StatusCode);

        var ownerStock = await Stock(owner);
        var partnerStock = await Stock(partner);
        var first = await Read<DistributionDto>(await owner.PostAsJsonAsync("/api/distributions", new CreateDistributionDto
        { BeneficiaryId = family.Id, InventoryItemId = ownerStock.Id, Quantity = 2 }));
        var second = await Read<DistributionDto>(await partner.PostAsJsonAsync("/api/distributions", new CreateDistributionDto
        { BeneficiaryId = family.Id, InventoryItemId = partnerStock.Id, Quantity = 3 }));
        var shared = await Read<BeneficiaryAidSummaryDto>(await partner.GetAsync($"/api/beneficiaries/{family.Id}/aid-history"));
        Assert.Equal(2, shared.TotalDistributionsCount);
        Assert.Contains(shared.AidHistory, d => d.Id == first.Id && !d.IsFromCurrentCharity);
        Assert.Contains(shared.AidHistory, d => d.Id == second.Id && d.IsFromCurrentCharity);
        var ownerView = await Read<BeneficiaryAidSummaryDto>(await owner.GetAsync($"/api/beneficiaries/{family.Id}/aid-history"));
        Assert.Equal(2, ownerView.TotalDistributionsCount);
        var privateList = await Read<List<DistributionDto>>(await partner.GetAsync($"/api/distributions/beneficiary/{family.Id}"));
        Assert.Equal(second.Id, Assert.Single(privateList).Id);
        Assert.Equal(1, (await Read<BeneficiaryAidSummaryDto>(await partner.GetAsync($"/api/distributions/beneficiary/{family.Id}/summary"))).TotalDistributionsCount);
        Assert.DoesNotContain(await Read<List<DistributionDto>>(await partner.GetAsync("/api/distributions/my-charity")), d => d.Id == first.Id);
        Assert.Equal(HttpStatusCode.Forbidden, (await partner.GetAsync("/api/distributions/all-network")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await owner.GetAsync("/api/distributions/all-network")).StatusCode);
        Assert.Equal(8, (await Read<InventoryItemDto>(await owner.GetAsync($"/api/inventory/{ownerStock.Id}"))).Quantity);
        Assert.Equal(7, (await Read<InventoryItemDto>(await partner.GetAsync($"/api/inventory/{partnerStock.Id}"))).Quantity);

        Assert.Equal(HttpStatusCode.BadRequest, (await partner.PostAsJsonAsync("/api/beneficiaries", new CreateBeneficiaryDto
        { Name = "Duplicate Ahmed", NationalId = family.NationalId })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await partner.PostAsJsonAsync($"/api/beneficiaries/{ownedByPartner.Id}/members", new CreateFamilyMemberDto
        { Name = "Duplicate Ahmed", NationalId = family.NationalId })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await owner.PutAsJsonAsync($"/api/beneficiaries/{family.Id}", new UpdateBeneficiaryDto
        { Name = "Updated Ahmed", NationalId = family.NationalId, FamilyCount = 2, Status = "مستحق فعال", FamilyMembers = family.FamilyMembers.Select(m => new CreateFamilyMemberDto { Id = m.Id, Name = m.Name, NationalId = m.NationalId }).ToList() })).StatusCode);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Expense_categories_cannot_be_read_changed_deleted_or_spent_by_another_charity(bool bulk)
    {
        using var owner = await app.Login("researcher", "Research@123");
        using var partner = await app.Login("amal", "Amal@123");
        var family = await Family(owner);
        var category = await Read<ExpenseCategoryDto>(await owner.PostAsJsonAsync("/api/expense-categories", new CreateExpenseCategoryDto { Name = "QA private " + Guid.NewGuid() }));
        Assert.All(await Read<List<ExpenseCategoryDto>>(await partner.GetAsync("/api/expense-categories")), c => Assert.Equal("c2", c.CharityId));
        Assert.Equal(HttpStatusCode.NotFound, (await partner.GetAsync($"/api/expense-categories/{category.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await partner.GetAsync($"/api/expense-categories/{category.Id}/beneficiaries")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await partner.PutAsJsonAsync($"/api/expense-categories/{category.Id}", new UpdateExpenseCategoryDto { Name = "tampered" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await partner.DeleteAsync($"/api/expense-categories/{category.Id}")).StatusCode);
        var path = bulk ? "/api/distributions/bulk" : "/api/distributions";
        object payload = bulk ? new BulkDistributionRequestDto { BeneficiaryIds = [family.Id], Kind = "إعانة نقدية", AmountPerBeneficiary = 100, ExpenseCategoryId = category.Id, AllowNegativeBalance = true }
            : new CreateDistributionDto { BeneficiaryId = family.Id, Kind = "إعانة نقدية", Amount = 100, ExpenseCategoryId = category.Id, AllowNegativeBalance = true };
        Assert.Equal(HttpStatusCode.Forbidden, (await partner.PostAsJsonAsync(path, payload)).StatusCode);
        Assert.False(await app.WithDb(db => db.Distributions.AnyAsync(d => d.BeneficiaryId == family.Id)));
        var ownCategory = await Read<ExpenseCategoryDto>(await partner.PostAsJsonAsync("/api/expense-categories", new CreateExpenseCategoryDto { Name = category.Name }));
        if (bulk) ((BulkDistributionRequestDto)payload).ExpenseCategoryId = ownCategory.Id;
        else ((CreateDistributionDto)payload).ExpenseCategoryId = ownCategory.Id;
        Assert.Equal(HttpStatusCode.OK, (await partner.PostAsJsonAsync(path, payload)).StatusCode);
        Assert.Equal(100m, (await Read<ExpenseCategoryDto>(await partner.GetAsync($"/api/expense-categories/{ownCategory.Id}"))).TotalDisbursedAmount);
        Assert.Equal(0m, (await Read<ExpenseCategoryDto>(await owner.GetAsync($"/api/expense-categories/{category.Id}"))).TotalDisbursedAmount);
    }

    [Fact]
    public async Task Reasons_are_private_and_foreign_references_are_rejected_before_changing_the_file()
    {
        using var owner = await app.Login("researcher", "Research@123");
        using var partner = await app.Login("amal", "Amal@123");
        var family = await Family(owner, true);
        var title = "QA reason " + Guid.NewGuid();
        var a = await Read<SuspensionReasonDto>(await owner.PostAsJsonAsync("/api/suspension-reasons", new { title }));
        var b = await Read<SuspensionReasonDto>(await partner.PostAsJsonAsync("/api/suspension-reasons", new { title }));
        var sep = await Read<SeparationReasonDto>(await partner.PostAsJsonAsync("/api/separation-reasons", new { title }));
        Assert.All(await Read<List<SuspensionReasonDto>>(await partner.GetAsync("/api/suspension-reasons")), r => Assert.Equal("c2", r.CharityId));
        Assert.All(await Read<List<SeparationReasonDto>>(await owner.GetAsync("/api/separation-reasons")), r => Assert.Equal("c1", r.CharityId));
        Assert.Equal(HttpStatusCode.Forbidden, (await partner.DeleteAsync($"/api/suspension-reasons/{a.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await owner.DeleteAsync($"/api/separation-reasons/{sep.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await owner.PostAsJsonAsync($"/api/beneficiaries/{family.Id}/suspend", new SuspendBeneficiaryDto { SuspensionReasonId = b.Id })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await owner.PostAsJsonAsync($"/api/beneficiaries/{family.Id}/members/separate", new SeparateFamilyMemberDto
        { FamilyMemberId = family.FamilyMembers[0].Id, SeparationReasonId = sep.Id })).StatusCode);
        Assert.False((await Read<BeneficiaryDto>(await owner.GetAsync($"/api/beneficiaries/{family.Id}"))).IsSuspended);
        Assert.Equal(HttpStatusCode.OK, (await owner.PostAsJsonAsync($"/api/beneficiaries/{family.Id}/suspend", new SuspendBeneficiaryDto { SuspensionReasonId = a.Id })).StatusCode);
        var partnerReason = (await Read<List<SuspensionReasonDto>>(await partner.GetAsync("/api/suspension-reasons"))).Single(r => r.Id == b.Id);
        Assert.Equal(0, partnerReason.LinkedSuspendedBeneficiariesCount);
        Assert.True(partnerReason.CanDelete);
        Assert.Equal(HttpStatusCode.NoContent, (await partner.DeleteAsync($"/api/suspension-reasons/{b.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await owner.DeleteAsync($"/api/suspension-reasons/{a.Id}")).StatusCode);
    }

    [Fact]
    public async Task Private_geographic_records_and_address_usage_are_isolated()
    {
        using var owner = await app.Login("researcher", "Research@123");
        using var partner = await app.Login("amal", "Amal@123");
        var district = (await Read<List<DistrictDto>>(await owner.GetAsync("/api/lookups/districts"))).First();
        var village = await Read<VillageDto>(await owner.PostAsJsonAsync("/api/lookups/villages", new CreateVillageDto { Name = "QA village " + Guid.NewGuid(), DistrictId = district.Id }));
        var area = await Read<VillageAreaDto>(await owner.PostAsJsonAsync("/api/lookups/village-areas", new CreateVillageAreaDto { Name = "QA area", VillageId = village.Id }));
        Assert.DoesNotContain(await Read<List<VillageDto>>(await partner.GetAsync("/api/lookups/villages")), v => v.Id == village.Id);
        Assert.DoesNotContain(await Read<List<VillageAreaDto>>(await partner.GetAsync("/api/lookups/village-areas")), v => v.Id == area.Id);
        Assert.Equal(HttpStatusCode.NotFound, (await partner.GetAsync($"/api/lookups/villages/{village.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await partner.GetAsync($"/api/lookups/village-areas/{area.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await partner.PutAsJsonAsync($"/api/lookups/villages/{village.Id}", new { name = "tampered" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await partner.PutAsJsonAsync($"/api/lookups/village-areas/{area.Id}", new { name = "tampered" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await partner.DeleteAsync($"/api/lookups/villages/{village.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await partner.DeleteAsync($"/api/lookups/village-areas/{area.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await partner.PostAsJsonAsync("/api/lookups/village-areas", new CreateVillageAreaDto { Name = "tampered", VillageId = village.Id })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await partner.PostAsJsonAsync("/api/beneficiaries", new CreateBeneficiaryDto { Name = "tampered geography", NationalId = Identity(), VillageId = village.Id })).StatusCode);
        var address = new CreateAddressRecordDto { GovernorateId = district.GovernorateId, DistrictId = district.Id, AddressText = "QA isolated address " + Guid.NewGuid() };
        var first = await Read<AddressRecordDto>(await owner.PostAsJsonAsync("/api/lookups/addresses", address));
        Assert.DoesNotContain(await Read<List<AddressAutocompleteItemDto>>(await partner.GetAsync("/api/lookups/addresses/autocomplete?limit=100")), a => a.Id == first.Id);
        var second = await Read<AddressRecordDto>(await partner.PostAsJsonAsync("/api/lookups/addresses", address));
        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(1, second.UsageCount);
    }

    [Fact]
    public async Task Private_defaults_remain_independent_and_reseeding_does_not_recreate_deleted_copies()
    {
        using var owner = await app.Login("researcher", "Research@123");
        using var partner = await app.Login("amal", "Amal@123");
        var a = await Read<List<ExpenseCategoryDto>>(await owner.GetAsync("/api/expense-categories"));
        var b = await Read<List<ExpenseCategoryDto>>(await partner.GetAsync("/api/expense-categories"));
        var first = a.First(c => c.Id.StartsWith("preset_") && c.CanDelete);
        var other = b.Single(c => c.Name == first.Name);
        Assert.NotEqual(first.Id, other.Id);
        Assert.Equal(HttpStatusCode.NoContent, (await owner.DeleteAsync($"/api/expense-categories/{first.Id}")).StatusCode);
        await app.WithDb(async db => { await TenantDefaults.EnsureAsync(db); return true; });
        Assert.Equal(HttpStatusCode.NotFound, (await owner.GetAsync($"/api/expense-categories/{first.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await partner.GetAsync($"/api/expense-categories/{other.Id}")).StatusCode);
    }

    [Fact]
    public async Task Existing_movements_using_a_shared_default_are_relinked_to_each_charitys_private_copy()
    {
        using var owner = await app.Login("researcher", "Research@123");
        using var partner = await app.Login("amal", "Amal@123");
        var family = await Family(owner);
        var a = await Read<List<ExpenseCategoryDto>>(await owner.GetAsync("/api/expense-categories"));
        var b = await Read<List<ExpenseCategoryDto>>(await partner.GetAsync("/api/expense-categories"));
        var legacyIds = await app.WithDb(async db =>
        {
            var templates = await db.ExpenseCategories.Where(c => c.CharityId == null).ToListAsync();
            var template = templates.First(t => a.Any(c => c.Name == t.Name) && b.Any(c => c.Name == t.Name));
            var first = new Charity.Domain.Entities.Distribution { BeneficiaryId = family.Id, CharityId = "c1", Kind = "إعانة نقدية", Amount = 125,
                ExpenseCategoryId = template.Id, ExpenseCategoryName = template.Name, Quantity = 1, Date = "2026-10-23" };
            var second = new Charity.Domain.Entities.Distribution { BeneficiaryId = family.Id, CharityId = "c2", Kind = "إعانة نقدية", Amount = 250,
                ExpenseCategoryId = template.Id, ExpenseCategoryName = template.Name, Quantity = 1, Date = "2026-10-23" };
            db.Distributions.AddRange(first, second);
            await db.SaveChangesAsync();
            await TenantDefaults.EnsureAsync(db);
            return new[] { first.Id, second.Id, template.Id };
        });
        await app.WithDb(async db =>
        {
            var first = await db.Distributions.SingleAsync(d => d.Id == legacyIds[0]);
            var second = await db.Distributions.SingleAsync(d => d.Id == legacyIds[1]);
            Assert.NotEqual(legacyIds[2], first.ExpenseCategoryId);
            Assert.NotEqual(first.ExpenseCategoryId, second.ExpenseCategoryId);
            Assert.Equal("c1", (await db.ExpenseCategories.SingleAsync(c => c.Id == first.ExpenseCategoryId)).CharityId);
            Assert.Equal("c2", (await db.ExpenseCategories.SingleAsync(c => c.Id == second.ExpenseCategoryId)).CharityId);
            Assert.Equal(125m, first.Amount);
            Assert.Equal(250m, second.Amount);
            return true;
        });
        var history = await Read<BeneficiaryAidSummaryDto>(await partner.GetAsync($"/api/beneficiaries/{family.Id}/aid-history"));
        Assert.Equal(2, history.TotalDistributionsCount);
    }
}
