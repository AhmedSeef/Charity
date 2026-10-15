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
            b.HasOne(u => u.Charity)
             .WithMany(c => c.Users)
             .HasForeignKey(u => u.CharityId)
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
            b.Property(x => x.NationalId).IsRequired().HasMaxLength(20);
            b.HasIndex(x => x.NationalId).IsUnique();
            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.MonthlyIncome).HasPrecision(18, 2);
            
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
            b.HasIndex(x => x.NationalId).IsUnique();
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

        // SuspensionReason configuration (Tenant / System-wide)
        builder.Entity<SuspensionReason>(b =>
        {
            b.ToTable("SuspensionReasons");
            b.HasKey(x => x.Id);
            b.Property(x => x.Title).IsRequired().HasMaxLength(200);
            b.Property(x => x.Description).HasMaxLength(500);
            b.Property(x => x.CharityId).HasMaxLength(50);
            b.Property(x => x.CharityName).HasMaxLength(200);
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
    }
}
