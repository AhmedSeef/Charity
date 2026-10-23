using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using Charity.Application.DTOs.Beneficiaries;
using Microsoft.EntityFrameworkCore;

namespace Charity.IntegrationTests;

[Collection("API")]
public sealed class BeneficiaryDetailsValidationTests(CharityApiFactory app)
{
    private static int _serial = 70000;
    private static string Identity() => "279010101" + Interlocked.Increment(ref _serial).ToString("D5");

    private static async Task<BeneficiaryDto> Family(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/beneficiaries", new CreateBeneficiaryDto
        { Name = "Original", NationalId = Identity(), MonthlyIncome = 55 });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<BeneficiaryDto>())!;
    }

    [Theory]
    [InlineData(null, "0")]
    [InlineData("", "0")]
    [InlineData("   ", "0")]
    [InlineData("\t\r\n", "0")]
    [InlineData("\u00a0", "0")]
    [InlineData("Valid name", "-0.01")]
    [InlineData("Valid name", "-100")]
    public async Task Invalid_name_or_income_is_rejected_for_heads_and_members_without_partial_writes(string? name, string amount)
    {
        using var client = await app.Login();
        var income = decimal.Parse(amount, CultureInfo.InvariantCulture);
        var family = await Family(client);
        var countBefore = await app.WithDb(db => db.Beneficiaries.CountAsync());
        var create = await client.PostAsJsonAsync("/api/beneficiaries", new
        { name, nationalId = Identity(), monthlyIncome = income, familyCount = 1 });
        Assert.Equal(HttpStatusCode.BadRequest, create.StatusCode);
        var update = await client.PutAsJsonAsync($"/api/beneficiaries/{family.Id}", new
        { name, nationalId = family.NationalId, monthlyIncome = income, familyCount = 1 });
        Assert.Equal(HttpStatusCode.BadRequest, update.StatusCode);

        var member = new { name, nationalId = Identity(), incomeAmount = income, hasIncome = false };
        var nested = await client.PostAsJsonAsync("/api/beneficiaries", new
        { name = "Valid head", nationalId = Identity(), familyMembers = new[] { member } });
        Assert.Equal(HttpStatusCode.BadRequest, nested.StatusCode);
        var updateMember = await client.PutAsJsonAsync($"/api/beneficiaries/{family.Id}", new
        { name = family.Name, nationalId = family.NationalId, monthlyIncome = 55, familyCount = 1, familyMembers = new[] { member } });
        Assert.Equal(HttpStatusCode.BadRequest, updateMember.StatusCode);
        var addMember = await client.PostAsJsonAsync($"/api/beneficiaries/{family.Id}/members", member);
        Assert.Equal(HttpStatusCode.BadRequest, addMember.StatusCode);

        var persisted = (await client.GetFromJsonAsync<BeneficiaryDto>($"/api/beneficiaries/{family.Id}"))!;
        Assert.Equal("Original", persisted.Name);
        Assert.Equal(55m, persisted.MonthlyIncome);
        Assert.Equal(1, persisted.FamilyCount);
        Assert.Empty(persisted.FamilyMembers);
        Assert.Equal(countBefore, await app.WithDb(db => db.Beneficiaries.CountAsync()));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("0")]
    [InlineData("0.01")]
    [InlineData("1234.56")]
    public async Task Zero_positive_and_unspecified_income_are_accepted_and_names_are_trimmed(string? amount)
    {
        using var client = await app.Login();
        decimal? income = amount == null ? null : decimal.Parse(amount, CultureInfo.InvariantCulture);
        var memberId = Identity();
        var response = await client.PostAsJsonAsync("/api/beneficiaries", new CreateBeneficiaryDto
        {
            Name = "  Valid head  ", NationalId = Identity(), MonthlyIncome = income,
            FamilyMembers = [new CreateFamilyMemberDto { Name = "  Valid child  ", NationalId = memberId, IncomeAmount = income }]
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var family = (await response.Content.ReadFromJsonAsync<BeneficiaryDto>())!;
        Assert.Equal("Valid head", family.Name);
        Assert.Equal(income, family.MonthlyIncome);
        var member = Assert.Single(family.FamilyMembers);
        Assert.Equal("Valid child", member.Name);
        Assert.Equal(income, member.IncomeAmount);

        var update = await client.PutAsJsonAsync($"/api/beneficiaries/{family.Id}", new UpdateBeneficiaryDto
        {
            Name = "  Updated head  ", NationalId = family.NationalId, MonthlyIncome = income, FamilyCount = 2,
            FamilyMembers = [new CreateFamilyMemberDto { Id = member.Id, Name = "  Updated child  ", NationalId = memberId, IncomeAmount = income }]
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var persisted = (await client.GetFromJsonAsync<BeneficiaryDto>($"/api/beneficiaries/{family.Id}"))!;
        Assert.Equal("Updated head", persisted.Name);
        Assert.Equal(income, persisted.MonthlyIncome);
        Assert.Equal("Updated child", Assert.Single(persisted.FamilyMembers).Name);
        Assert.Equal(income, persisted.FamilyMembers[0].IncomeAmount);

        var added = await client.PostAsJsonAsync($"/api/beneficiaries/{family.Id}/members", new CreateFamilyMemberDto
        { Name = "  Another child  ", NationalId = Identity(), IncomeAmount = income });
        Assert.Equal(HttpStatusCode.OK, added.StatusCode);
        var addedMember = (await added.Content.ReadFromJsonAsync<FamilyMemberDto>())!;
        Assert.Equal("Another child", addedMember.Name);
        Assert.Equal(income, addedMember.IncomeAmount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-9)]
    public async Task Invalid_family_count_is_rejected_on_create_and_update(int count)
    {
        using var client = await app.Login();
        var family = await Family(client);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/beneficiaries", new CreateBeneficiaryDto
        { Name = "Valid head", NationalId = Identity(), FamilyCount = count })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PutAsJsonAsync($"/api/beneficiaries/{family.Id}", new UpdateBeneficiaryDto
        { Name = family.Name, NationalId = family.NationalId, FamilyCount = count })).StatusCode);
        Assert.Equal(1, (await client.GetFromJsonAsync<BeneficiaryDto>($"/api/beneficiaries/{family.Id}"))!.FamilyCount);
    }
}
