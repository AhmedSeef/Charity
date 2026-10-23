using System.Net;
using System.Net.Http.Json;
using Charity.Application.DTOs.Staff;
using Charity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Charity.IntegrationTests;

[Collection("API")]
public sealed class StaffPasswordResetTests(CharityApiFactory app)
{
    [Fact]
    public async Task CharityManagerResetsOnlyTheirOwnStaffPassword()
    {
        var managerName = "resetmanager" + Guid.NewGuid().ToString("N")[..10];
        string managerId;
        using (var scope = app.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var created = await users.CreateAsync(new ApplicationUser
            { UserName = managerName, FullName = "مدير الجمعية للاختبار", CharityId = "c1" }, "Manager@123");
            Assert.True(created.Succeeded);
            var manager = await users.FindByNameAsync(managerName);
            Assert.NotNull(manager);
            managerId = manager.Id;
            Assert.True((await users.AddToRoleAsync(manager, "CharityManager")).Succeeded);
        }

        using var managerClient = await app.Login(managerName, "Manager@123");
        using var otherManager = await app.Login("amal", "Amal@123");
        using var admin = await app.Login();
        var staffName = "resetstaff" + Guid.NewGuid().ToString("N")[..10];
        var createdStaffResponse = await managerClient.PostAsJsonAsync("/api/staff/users", new CreateStaffUserDto
        { Username = staffName, FullName = "موظف اختبار", Password = "Before@123" });
        Assert.Equal(HttpStatusCode.Created, createdStaffResponse.StatusCode);
        var staff = await createdStaffResponse.Content.ReadFromJsonAsync<StaffUserDto>();
        Assert.NotNull(staff);

        var url = $"/api/staff/users/{staff.Id}/password";
        Assert.Equal(HttpStatusCode.Forbidden, (await otherManager.PutAsJsonAsync(url, new ChangeStaffUserPasswordDto { NewPassword = "Other@123" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await admin.PutAsJsonAsync(url, new ChangeStaffUserPasswordDto { NewPassword = "Admin@123" })).StatusCode);
        using (var staffClient = await app.Login(staffName, "Before@123"))
            Assert.Equal(HttpStatusCode.Forbidden, (await staffClient.PutAsJsonAsync(url, new ChangeStaffUserPasswordDto { NewPassword = "Self@123" })).StatusCode);

        Assert.Equal(HttpStatusCode.BadRequest, (await managerClient.PutAsJsonAsync(url, new ChangeStaffUserPasswordDto { NewPassword = "123" })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await managerClient.PutAsJsonAsync($"/api/staff/users/{managerId}/password", new ChangeStaffUserPasswordDto { NewPassword = "ManagerNew@123" })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await managerClient.PutAsJsonAsync(url, new ChangeStaffUserPasswordDto { NewPassword = "After@123" })).StatusCode);

        using var anonymous = app.CreateClient();
        Assert.Equal(HttpStatusCode.Forbidden, (await anonymous.PostAsJsonAsync("/api/auth/login", new { username = staffName, password = "Before@123" })).StatusCode);
        using var updatedStaff = await app.Login(staffName, "After@123");
        Assert.Equal(HttpStatusCode.OK, (await updatedStaff.GetAsync("/api/auth/me")).StatusCode);
    }
}
