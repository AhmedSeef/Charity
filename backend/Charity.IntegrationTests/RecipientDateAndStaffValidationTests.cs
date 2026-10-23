using System.Net;
using System.Net.Http.Json;
using Charity.Application.DTOs.Beneficiaries;
using Charity.Application.DTOs.Distributions;
using Charity.Application.DTOs.Inventory;
using Charity.Application.DTOs.Staff;
using Charity.Domain.Validation;
using Microsoft.EntityFrameworkCore;

namespace Charity.IntegrationTests;

[Collection("API")]
public sealed class RecipientDateAndStaffValidationTests(CharityApiFactory app)
{
    private static long _identity = 28501010110000;
    private static string Identity() => Interlocked.Increment(ref _identity).ToString();

    private static async Task<(BeneficiaryDto Family, InventoryItemDto Item)> SetupAid(HttpClient client)
    {
        var familyResponse = await client.PostAsJsonAsync("/api/beneficiaries", new CreateBeneficiaryDto
        {
            Name = "QA recipient family", NationalId = Identity(), FamilyCount = 2,
            FamilyMembers = [new CreateFamilyMemberDto { Name = "QA family member", NationalId = Identity() }]
        });
        Assert.Equal(HttpStatusCode.Created, familyResponse.StatusCode);
        var family = (await familyResponse.Content.ReadFromJsonAsync<BeneficiaryDto>())!;
        var itemResponse = await client.PostAsJsonAsync("/api/inventory", new CreateInventoryItemDto
        { ItemName = "QA recipient rice", Kind = "صنف ملموس", Quantity = 100, Unit = "كيلو" });
        Assert.Equal(HttpStatusCode.Created, itemResponse.StatusCode);
        return (family, (await itemResponse.Content.ReadFromJsonAsync<InventoryItemDto>())!);
    }

    [Fact]
    public async Task Only_head_or_current_family_member_can_receive_aid()
    {
        using var client = await app.Login();
        var (family, item) = await SetupAid(client);
        var other = (await SetupAid(client)).Family;
        var member = Assert.Single(family.FamilyMembers);
        var before = await app.WithDb(db => db.Distributions.CountAsync());

        foreach (var recipient in new[] { "missing", other.FamilyMembers[0].Id })
        {
            var rejected = await client.PostAsJsonAsync("/api/distributions", new CreateDistributionDto
            { BeneficiaryId = family.Id, InventoryItemId = item.Id, Quantity = 1, ReceivedByMemberId = recipient });
            Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
        }
        var external = await client.PostAsJsonAsync("/api/distributions", new CreateDistributionDto
        { BeneficiaryId = family.Id, InventoryItemId = item.Id, Quantity = 1, ReceivedByName = "شخص خارجي" });
        Assert.Equal(HttpStatusCode.BadRequest, external.StatusCode);
        Assert.Equal(before, await app.WithDb(db => db.Distributions.CountAsync()));

        var memberResponse = await client.PostAsJsonAsync("/api/distributions", new CreateDistributionDto
        { BeneficiaryId = family.Id, InventoryItemId = item.Id, Quantity = 1,
          ReceivedByMemberId = member.Id, ReceivedByName = "اسم مزيف" });
        Assert.Equal(HttpStatusCode.OK, memberResponse.StatusCode);
        var savedMember = (await memberResponse.Content.ReadFromJsonAsync<DistributionDto>())!;
        Assert.Equal(member.Id, savedMember.ReceivedByMemberId);
        Assert.Equal(member.Name, savedMember.ReceivedByName);
        Assert.Equal(member.NationalId, savedMember.ReceivedByNationalId);

        await app.WithDb(async db =>
        {
            var stored = await db.BeneficiaryFamilyMembers.SingleAsync(m => m.Id == member.Id);
            stored.IsSeparated = true;
            await db.SaveChangesAsync();
            return true;
        });
        var separated = await client.PostAsJsonAsync("/api/distributions", new CreateDistributionDto
        { BeneficiaryId = family.Id, InventoryItemId = item.Id, Quantity = 1, ReceivedByMemberId = member.Id });
        Assert.Equal(HttpStatusCode.BadRequest, separated.StatusCode);

        var headResponse = await client.PostAsJsonAsync("/api/distributions", new CreateDistributionDto
        { BeneficiaryId = family.Id, InventoryItemId = item.Id, Quantity = 1 });
        Assert.Equal(HttpStatusCode.OK, headResponse.StatusCode);
        Assert.Equal(family.Name, (await headResponse.Content.ReadFromJsonAsync<DistributionDto>())!.ReceivedByName);
    }

    [Theory]
    [InlineData("not-a-date")]
    [InlineData("2026-02-30")]
    [InlineData("future")]
    public async Task Invalid_or_future_distribution_date_is_rejected_without_writes(string date)
    {
        var submittedDate = date == "future" ? BusinessDate.TodayInCairo().AddDays(1).ToString("yyyy-MM-dd") : date;
        using var client = await app.Login();
        var (family, item) = await SetupAid(client);
        var before = await app.WithDb(db => db.Distributions.CountAsync());
        var single = await client.PostAsJsonAsync("/api/distributions", new CreateDistributionDto
        { BeneficiaryId = family.Id, InventoryItemId = item.Id, Quantity = 1, Date = submittedDate });
        Assert.Equal(HttpStatusCode.BadRequest, single.StatusCode);
        var bulk = await client.PostAsJsonAsync("/api/distributions/bulk", new BulkDistributionRequestDto
        { BeneficiaryIds = [family.Id], InventoryItemId = item.Id, QuantityPerBeneficiary = 1, Date = submittedDate });
        Assert.Equal(HttpStatusCode.BadRequest, bulk.StatusCode);
        Assert.Equal(before, await app.WithDb(db => db.Distributions.CountAsync()));
    }

    [Fact]
    public async Task Today_and_past_distribution_dates_are_accepted()
    {
        using var client = await app.Login();
        var (family, item) = await SetupAid(client);
        foreach (var date in new[] { BusinessDate.TodayInCairo(), BusinessDate.TodayInCairo().AddDays(-30) })
        {
            var response = await client.PostAsJsonAsync("/api/distributions", new CreateDistributionDto
            { BeneficiaryId = family.Id, InventoryItemId = item.Id, Quantity = 1, Date = date.ToString("yyyy-MM-dd") });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(date.ToString("yyyy-MM-dd"), (await response.Content.ReadFromJsonAsync<DistributionDto>())!.Date);
        }
        var defaultDate = await client.PostAsJsonAsync("/api/distributions", new
        { beneficiaryId = family.Id, inventoryItemId = item.Id, quantity = 1 });
        Assert.Equal(HttpStatusCode.OK, defaultDate.StatusCode);
        Assert.Equal(BusinessDate.TodayInCairo().ToString("yyyy-MM-dd"),
            (await defaultDate.Content.ReadFromJsonAsync<DistributionDto>())!.Date);
    }

    [Fact]
    public async Task Staff_requires_valid_identity_phone_name_date_and_nonnegative_salary()
    {
        using var client = await app.Login();
        var before = await app.WithDb(db => db.StaffMembers.CountAsync());
        var id = Identity();
        foreach (var bad in new object[]
        {
            new { name = " ", nationalId = id, phone = "01012345678", type = "متطوع" },
            new { name = "Valid", nationalId = "x", phone = "01012345678", type = "متطوع" },
            new { name = "Valid", nationalId = id, phone = "x", type = "متطوع" },
            new { name = "Valid", nationalId = id, phone = "01012345678", type = "موظف براتب", salary = -100 },
            new { name = "Valid", nationalId = id, phone = "01012345678", type = "موظف براتب", salary = 0, joinDate = "wrong" }
        })
            Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/staff", bad)).StatusCode);
        Assert.Equal(before, await app.WithDb(db => db.StaffMembers.CountAsync()));

        var paid = await client.PostAsJsonAsync("/api/staff", new CreateStaffMemberDto
        { Name = "Paid", NationalId = id, Phone = "01012345678", Type = "موظف براتب", Salary = 0 });
        Assert.Equal(HttpStatusCode.Created, paid.StatusCode);
        var paidMember = (await paid.Content.ReadFromJsonAsync<StaffMemberDto>())!;
        Assert.Equal(0, paidMember.Salary);
        var invalidUpdate = await client.PutAsJsonAsync($"/api/staff/{paidMember.Id}", new UpdateStaffMemberDto
        { Name = "Changed", NationalId = id, Phone = "01012345678", Type = "موظف براتب",
          Role = "محاسب", Salary = -1, JoinDate = paidMember.JoinDate });
        Assert.Equal(HttpStatusCode.BadRequest, invalidUpdate.StatusCode);
        Assert.Equal("Paid", (await (await client.GetAsync($"/api/staff/{paidMember.Id}"))
            .Content.ReadFromJsonAsync<StaffMemberDto>())!.Name);
        var volunteer = await client.PostAsJsonAsync("/api/staff", new CreateStaffMemberDto
        { Name = "Volunteer", NationalId = Identity(), Phone = "01112345678", Type = "متطوع" });
        Assert.Equal(HttpStatusCode.Created, volunteer.StatusCode);
        Assert.Null((await volunteer.Content.ReadFromJsonAsync<StaffMemberDto>())!.Salary);
    }
}
