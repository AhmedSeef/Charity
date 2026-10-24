using System.Net;
using System.Net.Http.Json;
using Charity.Application.DTOs.Staff;
using Charity.Domain.Security;

namespace Charity.IntegrationTests;

[Collection("API")]
public sealed class LookupPermissionTests(CharityApiFactory app)
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Unprivileged_or_read_only_staff_cannot_manage_settings_reasons_or_geography(bool readOnly)
    {
        using var manager = await app.Login("amal", "Amal@123");
        var username = "lookupqa" + Guid.NewGuid().ToString("N")[..10];
        var created = await manager.PostAsJsonAsync("/api/staff/users", new CreateStaffUserDto
        {
            Username = username, FullName = "اختبار حجب الإدارة", Password = "Test@123",
            Permissions = readOnly ? [AppPermissions.BeneficiariesView] : []
        });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        using var staff = await app.Login(username, "Test@123");
        foreach (var path in new[] { "/api/charities/my-settings", "/api/audit-logs" })
            Assert.Equal(HttpStatusCode.Forbidden, (await staff.GetAsync(path)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await staff.PutAsJsonAsync("/api/charities/my-settings", new { charityName = "unauthorized" })).StatusCode);
        foreach (var path in new[] { "/api/suspension-reasons", "/api/separation-reasons" })
        {
            Assert.Equal(HttpStatusCode.Forbidden, (await staff.GetAsync(path)).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await staff.PostAsJsonAsync(path, new { title = "unauthorized" })).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await staff.DeleteAsync(path + "/unknown")).StatusCode);
        }
        foreach (var path in new[] { "/api/lookups/villages", "/api/lookups/village-areas" })
        {
            Assert.Equal(readOnly ? HttpStatusCode.OK : HttpStatusCode.Forbidden, (await staff.GetAsync(path)).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await staff.PostAsJsonAsync(path, new { name = "unauthorized" })).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await staff.PutAsJsonAsync(path + "/unknown", new { name = "unauthorized" })).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await staff.DeleteAsync(path + "/unknown")).StatusCode);
        }
        Assert.Equal(HttpStatusCode.Forbidden, (await staff.PostAsJsonAsync("/api/lookups/addresses", new { addressText = "unauthorized" })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await manager.GetAsync("/api/charities/my-settings")).StatusCode);
        foreach (var path in new[] { "/api/suspension-reasons", "/api/separation-reasons" })
            Assert.Equal(HttpStatusCode.Created, (await manager.PostAsJsonAsync(path, new { title = username })).StatusCode);
    }
}
