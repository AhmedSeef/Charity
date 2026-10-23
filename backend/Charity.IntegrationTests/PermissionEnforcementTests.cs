using System.Net;
using System.Net.Http.Json;
using Charity.Application.DTOs.Staff;
using Charity.Domain.Security;

namespace Charity.IntegrationTests;

[Collection("API")]
public sealed class PermissionEnforcementTests(CharityApiFactory app)
{
    [Fact]
    public async Task RevokedPermissionInvalidatesExistingSessionAndProtectsApiRoutes()
    {
        using var admin = await app.Login();
        var username = "permissions" + Guid.NewGuid().ToString("N")[..10];
        var createdResponse = await admin.PostAsJsonAsync("/api/staff/users", new CreateStaffUserDto
        {
            Username = username,
            FullName = "موظف اختبار الصلاحيات",
            Password = "Test@123",
            Permissions = [AppPermissions.BeneficiariesView]
        });
        Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
        var staff = await createdResponse.Content.ReadFromJsonAsync<StaffUserDto>();
        Assert.NotNull(staff);

        using var employee = await app.Login(username, "Test@123");
        Assert.Equal(HttpStatusCode.OK, (await employee.GetAsync("/api/beneficiaries")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await employee.GetAsync("/api/staff/users")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await employee.GetAsync("/api/expense-categories")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await employee.PostAsJsonAsync("/api/expense-categories", new { name = "غير مصرح" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await employee.PostAsJsonAsync("/api/beneficiaries/unknown/suspend", new { suspensionReasonTitle = "اختبار" })).StatusCode);

        var changeResponse = await admin.PutAsJsonAsync($"/api/staff/users/{staff.Id}/permissions",
            new UpdateStaffUserPermissionsDto { RoleTemplate = JobRoleTemplates.Custom });
        Assert.Equal(HttpStatusCode.OK, changeResponse.StatusCode);

        Assert.Equal(HttpStatusCode.Unauthorized, (await employee.GetAsync("/api/beneficiaries")).StatusCode);
        using var renewed = await app.Login(username, "Test@123");
        Assert.Equal(HttpStatusCode.Forbidden, (await renewed.GetAsync("/api/beneficiaries")).StatusCode);
    }
}
