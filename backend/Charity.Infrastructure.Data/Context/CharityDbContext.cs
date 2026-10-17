using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Charity.Domain.Entities;

namespace Charity.Infrastructure.Data.Context;

public class CharityDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public CharityDbContext(DbContextOptions<CharityDbContext> options) : base(options)
    {
    }

    public DbSet<Charity.Domain.Entities.Charity> Charities => Set<Charity.Domain.Entities.Charity>();
    public DbSet<Beneficiary> Beneficiaries => Set<Beneficiary>();
    public DbSet<BeneficiaryFamilyMember> BeneficiaryFamilyMembers => Set<BeneficiaryFamilyMember>();
    public DbSet<BeneficiaryHeadHistory> BeneficiaryHeadHistories => Set<BeneficiaryHeadHistory>();
    public DbSet<SuspensionReason> SuspensionReasons => Set<SuspensionReason>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<Donation> Donations => Set<Donation>();
    public DbSet<Distribution> Distributions => Set<Distribution>();
    public DbSet<StaffMember> StaffMembers => Set<StaffMember>();
    public DbSet<CharitySettings> CharitySettings => Set<CharitySettings>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Rename Identity tables for clean schema
        builder.Entity<ApplicationUser>(b =>
        {
            b.ToTable("Users");
            b.Property(u => u.CharityId).IsRequired(false);
            b.HasOne(u => u.Charity)
             .WithMany(c => c.Users)
             .HasForeignKey(u => u.CharityId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<IdentityRole>(b => b.ToTable("Roles"));
        builder.Entity<IdentityUserRole<string>>(b => b.ToTable("UserRoles"));
        builder.Entity<IdentityUserClaim<string>>(b => b.ToTable("UserClaims"));
        builder.Entity<IdentityUserLogin<string>>(b => b.ToTable("UserLogins"));
        builder.Entity<IdentityRoleClaim<string>>(b => b.ToTable("RoleClaims"));
        builder.Entity<IdentityUserToken<string>>(b => b.ToTable("UserTokens"));

        // Charity entity configuration
        builder.Entity<Charity.Domain.Entities.Charity>(b =>
        {
            b.ToTable("Charities");
            b.HasKey(c => c.Id);
            b.Property(c => c.Name).IsRequired().HasMaxLength(200);
            b.Property(c => c.Phone).HasMaxLength(50);
            b.Property(c => c.Color).HasMaxLength(20);
        });

        // Beneficiary entity configuration (Shared network entity)
        builder.Entity<Beneficiary>(b =>
        {
            b.ToTable("Beneficiaries");
            b.HasKey(x => x.Id);
            b.Property(x => x.FamilyCode).HasMaxLength(50);
            b.HasIndex(x => x.FamilyCode);
            b.Property(x => x.FamilyName).HasMaxLength(250);
            b.Property(x => x.NationalId).IsRequired().HasMaxLength(20);
            b.HasIndex(x => x.NationalId).IsUnique().HasFilter("[IsDeleted] = 0");
            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.MonthlyIncome).HasPrecision(18, 2);
            b.Property(x => x.TotalDebtAmount).HasPrecision(18, 2);
            
            // RegisteredByCharity relation (Stewardship ownership)
            b.HasOne(x => x.RegisteredByCharity)
             .WithMany(c => c.RegisteredBeneficiaries)
             .HasForeignKey(x => x.RegisteredByCharityId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // BeneficiaryFamilyMember configuration (Dependents & Supporters)
        builder.Entity<BeneficiaryFamilyMember>(b =>
        {
            b.ToTable("BeneficiaryFamilyMembers");
            b.HasKey(x => x.Id);
            b.Property(x => x.NationalId).IsRequired().HasMaxLength(20);
            b.HasIndex(x => x.NationalId).IsUnique().HasFilter("[IsDeleted] = 0");
            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.Relationship).HasMaxLength(100);
            b.Property(x => x.MemberType).HasMaxLength(50);
            b.Property(x => x.IncomeAmount).HasPrecision(18, 2);
            b.Property(x => x.SupportType).HasMaxLength(100);

            b.HasOne(x => x.Beneficiary)
             .WithMany(ben => ben.FamilyMembers)
             .HasForeignKey(x => x.BeneficiaryId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // BeneficiaryHeadHistory configuration (Succession Audit Trail)
        builder.Entity<BeneficiaryHeadHistory>(b =>
        {
            b.ToTable("BeneficiaryHeadHistories");
            b.HasKey(x => x.Id);
            b.Property(x => x.PreviousHeadName).IsRequired().HasMaxLength(200);
            b.Property(x => x.PreviousHeadNationalId).IsRequired().HasMaxLength(20);
            b.Property(x => x.NewHeadName).IsRequired().HasMaxLength(200);
            b.Property(x => x.NewHeadNationalId).IsRequired().HasMaxLength(20);
            b.Property(x => x.Reason).HasMaxLength(200);
            b.Property(x => x.Notes).HasMaxLength(1000);
            b.Property(x => x.TransferredByCharityId).HasMaxLength(50);
            b.Property(x => x.TransferredByCharityName).HasMaxLength(200);

            b.HasOne(x => x.Beneficiary)
             .WithMany(ben => ben.HeadHistories)
             .HasForeignKey(x => x.BeneficiaryId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // SuspensionReason configuration (Tenant / System-wide)
        builder.Entity<SuspensionReason>(b =>
        {
            b.ToTable("SuspensionReasons");
            b.HasKey(x => x.Id);
            b.Property(x => x.Title).IsRequired().HasMaxLength(200);
            b.Property(x => x.Description).HasMaxLength(500);
            b.Property(x => x.CharityId).HasMaxLength(50).IsRequired(false);
            b.Property(x => x.CharityName).HasMaxLength(200).IsRequired(false);
        });

        // InventoryItem configuration (Tenant isolated)
        builder.Entity<InventoryItem>(b =>
        {
            b.ToTable("InventoryItems");
            b.HasKey(x => x.Id);
            b.Property(x => x.Code).HasMaxLength(50);
            b.Property(x => x.ItemName).IsRequired().HasMaxLength(200);
            b.Property(x => x.Kind).HasMaxLength(50);

            b.HasOne(x => x.Charity)
             .WithMany(c => c.InventoryItems)
             .HasForeignKey(x => x.CharityId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Donation configuration (Tenant isolated)
        builder.Entity<Donation>(b =>
        {
            b.ToTable("Donations");
            b.HasKey(x => x.Id);
            b.Property(x => x.Amount).HasPrecision(18, 2);
            b.Property(x => x.EstimatedValue).HasPrecision(18, 2);

            b.HasOne(x => x.Charity)
             .WithMany(c => c.Donations)
             .HasForeignKey(x => x.CharityId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.InventoryItem)
             .WithMany()
             .HasForeignKey(x => x.InventoryItemId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Distribution configuration (Shared in beneficiary history, stamped with granting charity)
        builder.Entity<Distribution>(b =>
        {
            b.ToTable("Distributions");
            b.HasKey(x => x.Id);
            b.Property(x => x.Amount).HasPrecision(18, 2);
            b.Property(x => x.ReceivedByMemberId).HasMaxLength(100);
            b.Property(x => x.ReceivedByName).HasMaxLength(200);
            b.Property(x => x.ReceivedByNationalId).HasMaxLength(20);
            b.Property(x => x.ReceivedByRelationship).HasMaxLength(100);

            b.HasOne(x => x.Beneficiary)
             .WithMany(ben => ben.Distributions)
             .HasForeignKey(x => x.BeneficiaryId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Charity)
             .WithMany(c => c.Distributions)
             .HasForeignKey(x => x.CharityId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.InventoryItem)
             .WithMany()
             .HasForeignKey(x => x.InventoryItemId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // StaffMember configuration (Tenant isolated)
        builder.Entity<StaffMember>(b =>
        {
            b.ToTable("StaffMembers");
            b.HasKey(x => x.Id);
            b.Property(x => x.Salary).HasPrecision(18, 2);
            b.Property(x => x.NationalId).HasMaxLength(20);

            b.HasOne(x => x.Charity)
             .WithMany(c => c.StaffMembers)
             .HasForeignKey(x => x.CharityId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // CharitySettings configuration
        builder.Entity<CharitySettings>(b =>
        {
            b.ToTable("CharitySettings");
            b.HasKey(x => x.Id);

            b.HasOne(x => x.Charity)
             .WithMany()
             .HasForeignKey(x => x.CharityId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ==========================================
        // Soft Delete: Global Query Filters
        // الداتا التاريخية محفوظة دائماً ولا يتم حذفها فعلياً من قاعدة البيانات
        // ==========================================
        builder.Entity<Beneficiary>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<BeneficiaryFamilyMember>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<BeneficiaryHeadHistory>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Distribution>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Donation>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<InventoryItem>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<StaffMember>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<SuspensionReason>().HasQueryFilter(x => !x.IsDeleted);
    }
}
