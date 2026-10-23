using System.Security.Cryptography;
using System.Text;
using Charity.Domain.Entities;
using Charity.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Charity.Infrastructure.Data.Seed;

public static class TenantDefaults
{
    // Defaults are copied once per charity. Editing/deleting a private copy never changes another charity.
    public static async Task EnsureAsync(CharityDbContext db, string? charityId = null)
    {
        var charities = await db.Charities.Where(c => charityId == null || c.Id == charityId).ToListAsync();
        var expenses = await db.ExpenseCategories.Where(c => c.CharityId == null).AsNoTracking().ToListAsync();
        var suspensions = await db.SuspensionReasons.Where(r => r.CharityId == null).AsNoTracking().ToListAsync();
        var separations = await db.SeparationReasons.Where(r => r.CharityId == null).AsNoTracking().ToListAsync();
        var expenseCopies = await db.ExpenseCategories.IgnoreQueryFilters().AsNoTracking().ToListAsync();
        var suspensionCopies = await db.SuspensionReasons.IgnoreQueryFilters().AsNoTracking().ToListAsync();
        var separationCopies = await db.SeparationReasons.IgnoreQueryFilters().AsNoTracking().ToListAsync();
        foreach (var charity in charities)
        {
            var expenseIds = new Dictionary<string, string>();
            var suspensionIds = new Dictionary<string, string>();
            var separationIds = new Dictionary<string, string>();
            foreach (var template in expenses)
            {
                var id = CopyId(charity.Id, template.Id);
                var existing = expenseCopies.FirstOrDefault(c => c.Id == id || (c.CharityId == charity.Id && c.Name == template.Name));
                expenseIds[template.Id] = existing?.Id ?? id;
                if (existing != null) continue;
                db.ExpenseCategories.Add(new ExpenseCategory
                { Id = id, Name = template.Name, Code = template.Code, Description = template.Description,
                    IsActive = template.IsActive, CharityId = charity.Id, CharityName = charity.Name });
            }
            foreach (var template in suspensions)
            {
                var id = CopyId(charity.Id, template.Id);
                var existing = suspensionCopies.FirstOrDefault(r => r.Id == id || (r.CharityId == charity.Id && r.Title == template.Title));
                suspensionIds[template.Id] = existing?.Id ?? id;
                if (existing != null) continue;
                db.SuspensionReasons.Add(new SuspensionReason
                { Id = id, Title = template.Title, Description = template.Description, IsActive = template.IsActive,
                    CharityId = charity.Id, CharityName = charity.Name });
            }
            foreach (var template in separations)
            {
                var id = CopyId(charity.Id, template.Id);
                var existing = separationCopies.FirstOrDefault(r => r.Id == id || (r.CharityId == charity.Id && r.Title == template.Title));
                separationIds[template.Id] = existing?.Id ?? id;
                if (existing != null) continue;
                db.SeparationReasons.Add(new SeparationReason
                { Id = id, Title = template.Title, Description = template.Description, IsActive = template.IsActive,
                    CharityId = charity.Id, CharityName = charity.Name });
            }
            // Preserve the charity's existing history when replacing shared defaults with private copies.
            var distributions = await db.Distributions.IgnoreQueryFilters().Where(d => d.CharityId == charity.Id && d.ExpenseCategoryId != null).ToListAsync();
            foreach (var distribution in distributions)
                if (expenseIds.TryGetValue(distribution.ExpenseCategoryId!, out var replacement)) distribution.ExpenseCategoryId = replacement;
            var beneficiaries = await db.Beneficiaries.IgnoreQueryFilters().Where(b => b.RegisteredByCharityId == charity.Id).ToListAsync();
            foreach (var beneficiary in beneficiaries)
                if (beneficiary.SuspensionReasonId != null && suspensionIds.TryGetValue(beneficiary.SuspensionReasonId, out var replacement))
                    beneficiary.SuspensionReasonId = replacement;
            var beneficiaryIds = beneficiaries.Select(b => b.Id).ToList();
            var members = await db.BeneficiaryFamilyMembers.IgnoreQueryFilters().Where(m => beneficiaryIds.Contains(m.BeneficiaryId) && m.SeparationReasonId != null).ToListAsync();
            foreach (var member in members)
                if (separationIds.TryGetValue(member.SeparationReasonId!, out var replacement)) member.SeparationReasonId = replacement;
        }
        await db.SaveChangesAsync();
    }

    private static string CopyId(string charityId, string templateId) =>
        "preset_" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{charityId}:{templateId}")))[..32];
}
