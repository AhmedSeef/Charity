using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Charity.Application.DTOs.Beneficiaries;
using Charity.Application.DTOs.Distributions;
using Charity.Application.DTOs.Donations;
using Charity.Application.DTOs.Inventory;
using Charity.Application.DTOs.Lookups;
using Microsoft.EntityFrameworkCore;

namespace Charity.IntegrationTests;

[Collection("API")]
public sealed class OverdraftTests(CharityApiFactory app)
{
    private static long _identity = 29301010100000;
    private static async Task<T> Read<T>(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"HTTP {(int)response.StatusCode}: {body}");
        return JsonSerializer.Deserialize<T>(body, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
    }

    private static async Task<BeneficiaryDto> Family(HttpClient client) => await Read<BeneficiaryDto>(
        await client.PostAsJsonAsync("/api/beneficiaries", new CreateBeneficiaryDto
        { Name = "QA overdraft", NationalId = Interlocked.Increment(ref _identity).ToString() }));

    [Theory]
    [InlineData(false, 2)]
    [InlineData(false, 0)]
    [InlineData(true, 2)]
    [InlineData(true, 0)]
    public async Task Stock_shortage_warns_before_writing_then_confirmation_persists_negative_balance(bool bulk, double initial)
    {
        using var client = await app.Login();
        var family = await Family(client);
        var item = await Read<InventoryItemDto>(await client.PostAsJsonAsync("/api/inventory",
            new CreateInventoryItemDto { ItemName = "QA overdraft stock", Quantity = initial, Kind = "صنف ملموس", Unit = "kg" }));
        var path = bulk ? "/api/distributions/bulk" : "/api/distributions";
        object Request(bool confirmed) => bulk
            ? new BulkDistributionRequestDto { BeneficiaryIds = [family.Id], InventoryItemId = item.Id, QuantityPerBeneficiary = 3, AllowNegativeBalance = confirmed }
            : new CreateDistributionDto { BeneficiaryId = family.Id, InventoryItemId = item.Id, Quantity = 3, AllowNegativeBalance = confirmed };

        var warning = await client.PostAsJsonAsync(path, Request(false));
        Assert.Equal(HttpStatusCode.Conflict, warning.StatusCode);
        Assert.Equal("balance_confirmation_required", (await warning.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
        Assert.Equal(initial, (await Read<InventoryItemDto>(await client.GetAsync($"/api/inventory/{item.Id}"))).Quantity);
        Assert.False(await app.WithDb(db => db.Distributions.AnyAsync(d => d.InventoryItemId == item.Id)));

        var confirmed = await client.PostAsJsonAsync(path, Request(true));
        if (bulk) Assert.Single((await Read<BulkDistributionResultDto>(confirmed)).Warnings);
        else Assert.Single((await Read<DistributionDto>(confirmed)).Warnings);
        Assert.Equal(initial - 3, (await Read<InventoryItemDto>(await client.GetAsync($"/api/inventory/{item.Id}"))).Quantity);

        // A later incoming donation reconciles the shortage without erasing prior disbursements.
        await Read<DonationDto>(await client.PostAsJsonAsync("/api/donations", new CreateDonationDto
        { DonorName = "QA late receipt", DonationKind = "عيني", InventoryItemId = item.Id, Quantity = 5 }));
        Assert.Equal(initial + 2, (await Read<InventoryItemDto>(await client.GetAsync($"/api/inventory/{item.Id}"))).Quantity);
        Assert.Equal(1, await app.WithDb(db => db.Distributions.CountAsync(d => d.InventoryItemId == item.Id)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Cash_shortage_requires_confirmation_and_reports_the_negative_ledger_balance(bool bulk)
    {
        using var client = await app.Login("amal", "Amal@123");
        var family = await Family(client);
        var categories = await Read<List<ExpenseCategoryDto>>(await client.GetAsync("/api/expense-categories"));
        var donations = await Read<List<DonationDto>>(await client.GetAsync("/api/donations"));
        var distributions = await Read<List<DistributionDto>>(await client.GetAsync("/api/distributions/my-charity"));
        var before = donations.Where(d => d.DonationKind == "نقدي" || string.IsNullOrEmpty(d.DonationKind)).Sum(d => d.Amount ?? 0)
            - distributions.Where(d => d.Kind == "إعانة نقدية").Sum(d => d.Amount ?? 0);
        var amount = Math.Max(0, before) + 100;
        object Request(bool confirmed) => bulk
            ? new BulkDistributionRequestDto { BeneficiaryIds = [family.Id], Kind = "إعانة نقدية", ExpenseCategoryId = categories[0].Id, AmountPerBeneficiary = amount, AllowNegativeBalance = confirmed }
            : new CreateDistributionDto { BeneficiaryId = family.Id, Kind = "إعانة نقدية", ExpenseCategoryId = categories[0].Id, Amount = amount, AllowNegativeBalance = confirmed };
        var path = bulk ? "/api/distributions/bulk" : "/api/distributions";
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync(path, Request(false))).StatusCode);
        Assert.False(await app.WithDb(db => db.Distributions.AnyAsync(d => d.BeneficiaryId == family.Id)));
        var response = await client.PostAsJsonAsync(path, Request(true));
        var balance = bulk ? (await Read<BulkDistributionResultDto>(response)).RemainingCashBalance
            : (await Read<DistributionDto>(response)).RemainingCashBalance;
        Assert.Equal(before - amount, balance);
        Assert.Equal(1, await app.WithDb(db => db.Distributions.CountAsync(d => d.BeneficiaryId == family.Id)));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Concurrent_disbursements_preserve_stock_and_require_confirmation_for_overdraft(bool confirmed)
    {
        using var client = await app.Login();
        var family = await Family(client);
        var item = await Read<InventoryItemDto>(await client.PostAsJsonAsync("/api/inventory",
            new CreateInventoryItemDto { ItemName = "QA concurrent overdraft", Quantity = 10, Kind = "صنف ملموس" }));
        var responses = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => client.PostAsJsonAsync("/api/distributions",
            new CreateDistributionDto { BeneficiaryId = family.Id, InventoryItemId = item.Id, Quantity = 3, AllowNegativeBalance = confirmed })));
        Assert.Equal(confirmed ? 8 : 3, responses.Count(r => r.IsSuccessStatusCode));
        foreach (var response in responses.Where(r => !r.IsSuccessStatusCode))
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(confirmed ? -14 : 1, (await Read<InventoryItemDto>(await client.GetAsync($"/api/inventory/{item.Id}"))).Quantity);
        Assert.Equal(confirmed ? 24 : 9, await app.WithDb(db => db.Distributions.Where(d => d.InventoryItemId == item.Id).SumAsync(d => d.Quantity)));
    }

    [Fact]
    public async Task Concurrent_cash_disbursements_do_not_silently_overdraw_without_confirmation()
    {
        using var client = await app.Login("amal", "Amal@123");
        var family = await Family(client);
        var categories = await Read<List<ExpenseCategoryDto>>(await client.GetAsync("/api/expense-categories"));
        await Read<DonationDto>(await client.PostAsJsonAsync("/api/donations", new CreateDonationDto
        { DonorName = "QA cash concurrency funding", DonationKind = "نقدي", Amount = 1000000 }));
        var donations = await Read<List<DonationDto>>(await client.GetAsync("/api/donations"));
        var distributions = await Read<List<DistributionDto>>(await client.GetAsync("/api/distributions/my-charity"));
        var balance = donations.Where(d => d.DonationKind == "نقدي" || string.IsNullOrEmpty(d.DonationKind)).Sum(d => d.Amount ?? 0)
            - distributions.Where(d => d.Kind == "إعانة نقدية").Sum(d => d.Amount ?? 0);
        Assert.True(balance > 0);
        var responses = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => client.PostAsJsonAsync("/api/distributions",
            new CreateDistributionDto { BeneficiaryId = family.Id, Kind = "إعانة نقدية", Amount = balance, ExpenseCategoryId = categories[0].Id })));
        Assert.Single(responses, r => r.IsSuccessStatusCode);
        foreach (var response in responses.Where(r => !r.IsSuccessStatusCode)) Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(0m, (await Read<DistributionDto>(responses.Single(r => r.IsSuccessStatusCode))).RemainingCashBalance);
        Assert.Equal(1, await app.WithDb(db => db.Distributions.CountAsync(d => d.BeneficiaryId == family.Id)));
    }
}
