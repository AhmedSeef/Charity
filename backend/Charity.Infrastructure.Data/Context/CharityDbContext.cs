using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Charity.Domain.Entities;
using Charity.Application.Services.Interfaces;

namespace Charity.Infrastructure.Data.Context;

public class CharityDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    private readonly ICurrentUserService? _currentUser;

    public CharityDbContext(DbContextOptions<CharityDbContext> options, ICurrentUserService? currentUser = null) : base(options)
    {
        _currentUser = currentUser;
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
    public DbSet<SeparationReason> SeparationReasons => Set<SeparationReason>();
    public DbSet<Governorate> Governorates => Set<Governorate>();
    public DbSet<District> Districts => Set<District>();
    public DbSet<AddressRecord> AddressRecords => Set<AddressRecord>();
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
    public DbSet<Village> Villages => Set<Village>();
    public DbSet<VillageArea> VillageAreas => Set<VillageArea>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        TrackAudit();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        TrackAudit();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void TrackAudit()
    {
        if (_currentUser?.IsAuthenticated != true || string.IsNullOrWhiteSpace(_currentUser.UserId)) return;

        ChangeTracker.DetectChanges();
        var now = DateTime.UtcNow;
        var userId = _currentUser.UserId!;
        var displayName = string.IsNullOrWhiteSpace(_currentUser.FullName) ? _currentUser.Username ?? userId : _currentUser.FullName;
        foreach (var entry in ChangeTracker.Entries().ToList())
        {
            if (entry.Entity is not BaseEntity && entry.Entity is not ApplicationUser) continue;
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted)) continue;

            var entity = entry.Entity;
            var action = entry.State switch
            {
                EntityState.Added => "Create",
                EntityState.Deleted => "Delete",
                _ when entity is BaseEntity soft && soft.IsDeleted && !soft.DeletedAt.HasValue => "Delete",
                _ when entity is BaseEntity soft && soft.IsDeleted &&
                    entry.Property(nameof(BaseEntity.IsDeleted)).OriginalValue is false => "Delete",
                _ => "Update"
            };

            if (entity is BaseEntity record)
            {
                if (action == "Create")
                {
                    record.CreatedAt = now;
                    record.CreatedBy = userId;
                    record.CreatedByName = displayName;
                }
                else if (action == "Delete")
                {
                    record.DeletedAt ??= now;
                    record.DeletedBy = userId;
                    record.DeletedByName = displayName;
                }
                else
                {
                    record.UpdatedAt = now;
                    record.UpdatedBy = userId;
                    record.UpdatedByName = displayName;
                }
            }
            else if (entity is ApplicationUser account)
            {
                if (action == "Create")
                {
                    account.CreatedAt = now;
                    account.CreatedBy = userId;
                    account.CreatedByName = displayName;
                }
                else if (action == "Update")
                {
                    account.UpdatedAt = now;
                    account.UpdatedBy = userId;
                    account.UpdatedByName = displayName;
                }
            }

            var charityId = ResolveAuditCharityId(entity);
            if (string.IsNullOrWhiteSpace(charityId)) continue;
            AuditLogs.Add(new AuditLog
            {
                OccurredAt = now,
                CharityId = charityId,
                UserId = userId,
                Username = _currentUser.Username ?? userId,
                ActorName = displayName,
                Action = action,
                EntityType = entity.GetType().Name,
                EntityId = entity is BaseEntity baseEntity ? baseEntity.Id : ((ApplicationUser)entity).Id,
                EntityName = ResolveEntityName(entity)
            });
        }
    }

    private string? ResolveAuditCharityId(object entity)
    {
        if (entity is Charity.Domain.Entities.Charity charity) return charity.Id;
        if (entity is Beneficiary beneficiary) return beneficiary.RegisteredByCharityId;
        if (entity is BeneficiaryFamilyMember member)
            return ChangeTracker.Entries<Beneficiary>().FirstOrDefault(e => e.Entity.Id == member.BeneficiaryId)?.Entity.RegisteredByCharityId
                ?? Beneficiaries.IgnoreQueryFilters().Where(b => b.Id == member.BeneficiaryId)
                .Select(b => b.RegisteredByCharityId).FirstOrDefault();
        if (entity is BeneficiaryHeadHistory history)
            return ChangeTracker.Entries<Beneficiary>().FirstOrDefault(e => e.Entity.Id == history.BeneficiaryId)?.Entity.RegisteredByCharityId
                ?? Beneficiaries.IgnoreQueryFilters().Where(b => b.Id == history.BeneficiaryId)
                .Select(b => b.RegisteredByCharityId).FirstOrDefault();
        return entity.GetType().GetProperty("CharityId")?.GetValue(entity) as string ?? _currentUser?.CharityId;
    }

    private static string ResolveEntityName(object entity)
    {
        foreach (var name in new[] { "Name", "FullName", "Title", "ItemName", "ExpenseCategoryName", "Kind", "Username" })
        {
            if (entity.GetType().GetProperty(name)?.GetValue(entity) is string value && !string.IsNullOrWhiteSpace(value))
                return value.Length > 200 ? value[..200] : value;
        }
        return string.Empty;
    }

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

        builder.Entity<AuditLog>(b =>
        {
            b.ToTable("AuditLogs");
            b.HasKey(x => x.Id);
            b.Property(x => x.CharityId).HasMaxLength(50).IsRequired();
            b.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            b.Property(x => x.Username).HasMaxLength(256).IsRequired();
            b.Property(x => x.ActorName).HasMaxLength(200).IsRequired();
            b.Property(x => x.Action).HasMaxLength(40).IsRequired();
            b.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
            b.Property(x => x.EntityId).HasMaxLength(450).IsRequired();
            b.Property(x => x.EntityName).HasMaxLength(200).IsRequired();
            b.HasIndex(x => new { x.CharityId, x.OccurredAt });
            b.HasIndex(x => new { x.CharityId, x.UserId, x.Action });
        });

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

        // SeparationReason configuration (Tenant / System-wide)
        builder.Entity<SeparationReason>(b =>
        {
            b.ToTable("SeparationReasons");
            b.HasKey(x => x.Id);
            b.Property(x => x.Title).IsRequired().HasMaxLength(200);
            b.Property(x => x.Description).HasMaxLength(500);
            b.Property(x => x.CharityId).HasMaxLength(50).IsRequired(false);
            b.Property(x => x.CharityName).HasMaxLength(200).IsRequired(false);
        });

        // Governorate configuration (المحافظة)
        builder.Entity<Governorate>(b =>
        {
            b.ToTable("Governorates");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasMaxLength(50);
            b.Property(x => x.Name).IsRequired().HasMaxLength(150);
            b.Property(x => x.NameEn).HasMaxLength(150);
        });

        // District configuration (المركز أو الحي)
        builder.Entity<District>(b =>
        {
            b.ToTable("Districts");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasMaxLength(50);
            b.Property(x => x.Name).IsRequired().HasMaxLength(150);
            b.Property(x => x.NameEn).HasMaxLength(150);
            b.Property(x => x.GovernorateId).IsRequired().HasMaxLength(50);

            b.HasOne(x => x.Governorate)
             .WithMany(g => g.Districts)
             .HasForeignKey(x => x.GovernorateId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // AddressRecord configuration (جدول العناوين المستقل)
        builder.Entity<AddressRecord>(b =>
        {
            b.ToTable("AddressRecords");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasMaxLength(50);
            b.Property(x => x.AddressText).IsRequired().HasMaxLength(300);
            b.Property(x => x.NormalizedText).IsRequired().HasMaxLength(300);
            b.Property(x => x.GovernorateId).IsRequired().HasMaxLength(50);
            b.Property(x => x.DistrictId).IsRequired().HasMaxLength(50);
            b.Property(x => x.CharityId).HasMaxLength(50).IsRequired(false);
            b.Property(x => x.CharityName).HasMaxLength(200).IsRequired(false);

            b.HasOne(x => x.Governorate)
             .WithMany()
             .HasForeignKey(x => x.GovernorateId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.District)
             .WithMany(d => d.AddressRecords)
             .HasForeignKey(x => x.DistrictId)
             .OnDelete(DeleteBehavior.Restrict);
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

            b.Property(x => x.ExpenseCategoryId).HasMaxLength(50).IsRequired(false);
            b.Property(x => x.ExpenseCategoryName).HasMaxLength(200).IsRequired(false);

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

            b.HasOne(x => x.ExpenseCategory)
             .WithMany(ec => ec.Distributions)
             .HasForeignKey(x => x.ExpenseCategoryId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ExpenseCategory configuration (أنواع وبنود المصروفات)
        builder.Entity<ExpenseCategory>(b =>
        {
            b.ToTable("ExpenseCategories");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasMaxLength(50);
            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.Code).HasMaxLength(50);
            b.Property(x => x.Description).HasMaxLength(500);
            b.Property(x => x.CharityId).HasMaxLength(50).IsRequired(false);
            b.Property(x => x.CharityName).HasMaxLength(200).IsRequired(false);
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

        // Village configuration (القرى والوحدات المحلية)
        builder.Entity<Village>(b =>
        {
            b.ToTable("Villages");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.NameEn).HasMaxLength(200);
            b.Property(x => x.CharityId).HasMaxLength(50).IsRequired(false);
            b.Property(x => x.CharityName).HasMaxLength(200).IsRequired(false);

            b.HasOne(x => x.District)
             .WithMany(d => d.Villages)
             .HasForeignKey(x => x.DistrictId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // VillageArea configuration (المناطق والشوارع التابعة للقرية)
        builder.Entity<VillageArea>(b =>
        {
            b.ToTable("VillageAreas");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.NameEn).HasMaxLength(200);
            b.Property(x => x.Notes).HasMaxLength(500);
            b.Property(x => x.CharityId).HasMaxLength(50).IsRequired(false);
            b.Property(x => x.CharityName).HasMaxLength(200).IsRequired(false);

            b.HasOne(x => x.Village)
             .WithMany(v => v.Areas)
             .HasForeignKey(x => x.VillageId)
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
        builder.Entity<SeparationReason>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Governorate>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<District>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<AddressRecord>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<ExpenseCategory>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Village>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<VillageArea>().HasQueryFilter(x => !x.IsDeleted);
    }
}
