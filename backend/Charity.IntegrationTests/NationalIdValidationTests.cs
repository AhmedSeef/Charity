using System.Net;
using System.Net.Http.Json;
using Charity.Application.DTOs.Beneficiaries;
using Microsoft.EntityFrameworkCore;

namespace Charity.IntegrationTests;

[Collection("API")]
public sealed class NationalIdValidationTests(CharityApiFactory app)
{
    private static int _serial = 70000;
    private static string Identity() => "280010101" + Interlocked.Increment(ref _serial).ToString("D5");
    private static string Arabic(string value) => new(value.Select(c => (char)('٠' + c - '0')).ToArray());

    private static async Task<BeneficiaryDto> Family(HttpClient client, string? nationalId = null)
    {
        var response = await client.PostAsJsonAsync("/api/beneficiaries", new CreateBeneficiaryDto
        { Name = "QA Egyptian identity", NationalId = nationalId ?? Identity() });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<BeneficiaryDto>())!;
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("2850101123456")]
    [InlineData("285010112345678")]
    [InlineData("18501011234567")]
    [InlineData("28513011234567")]
    [InlineData("29902291234567")]
    [InlineData("28504311234567")]
    [InlineData("39901010112345")]
    [InlineData("28501010512345")]
    public async Task Invalid_identity_is_rejected_on_create_update_nested_member_and_add_member_without_partial_writes(string? invalid)
    {
        using var client = await app.Login();
        var family = await Family(client);
        var countBefore = await app.WithDb(db => db.Beneficiaries.CountAsync());
        var create = await client.PostAsJsonAsync("/api/beneficiaries", new { name = "Invalid head", nationalId = invalid });
        Assert.Equal(HttpStatusCode.BadRequest, create.StatusCode);

        var nested = await client.PostAsJsonAsync("/api/beneficiaries", new
        { name = "Invalid nested member", nationalId = Identity(), familyMembers = new[] { new { name = "Invalid member", nationalId = invalid } } });
        Assert.Equal(HttpStatusCode.BadRequest, nested.StatusCode);

        var update = await client.PutAsJsonAsync($"/api/beneficiaries/{family.Id}", new { name = family.Name, nationalId = invalid, familyCount = 1 });
        Assert.Equal(HttpStatusCode.BadRequest, update.StatusCode);

        var updateMember = await client.PutAsJsonAsync($"/api/beneficiaries/{family.Id}", new
        { name = family.Name, nationalId = family.NationalId, familyCount = 1, familyMembers = new[] { new { name = "Invalid member", nationalId = invalid } } });
        Assert.Equal(HttpStatusCode.BadRequest, updateMember.StatusCode);

        var add = await client.PostAsJsonAsync($"/api/beneficiaries/{family.Id}/members", new { name = "Invalid member", nationalId = invalid });
        Assert.Equal(HttpStatusCode.BadRequest, add.StatusCode);
        Assert.Equal(countBefore, await app.WithDb(db => db.Beneficiaries.CountAsync()));
        Assert.False(await app.WithDb(db => db.BeneficiaryFamilyMembers.AnyAsync(m => m.BeneficiaryId == family.Id)));
        var persisted = (await client.GetFromJsonAsync<BeneficiaryDto>($"/api/beneficiaries/{family.Id}"))!;
        Assert.Equal(family.NationalId, persisted.NationalId);
        Assert.Equal(family.FamilyCount, persisted.FamilyCount);
    }

    [Fact]
    public async Task Arabic_digits_are_normalized_for_persistence_and_cannot_bypass_duplicate_identity_checks()
    {
        using var client = await app.Login();
        var id = Identity();
        var family = await Family(client, Arabic(id));
        Assert.Equal(id, family.NationalId);
        var memberId = Identity();
        var response = await client.PostAsJsonAsync($"/api/beneficiaries/{family.Id}/members", new CreateFamilyMemberDto
        { Name = "Arabic identity", NationalId = Arabic(memberId) });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(memberId, (await response.Content.ReadFromJsonAsync<FamilyMemberDto>())!.NationalId);
        var duplicate = await client.PostAsJsonAsync("/api/beneficiaries", new CreateBeneficiaryDto
        { Name = "Duplicate Arabic head", NationalId = Arabic(id) });
        Assert.Equal(HttpStatusCode.BadRequest, duplicate.StatusCode);
        var other = await Family(client);
        var duplicateMember = await client.PostAsJsonAsync($"/api/beneficiaries/{other.Id}/members", new CreateFamilyMemberDto
        { Name = "Duplicate Arabic member", NationalId = Arabic(memberId) });
        Assert.Equal(HttpStatusCode.BadRequest, duplicateMember.StatusCode);
    }
}
