using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Charity.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditTrail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Villages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "Villages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Villages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "Villages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Villages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Villages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "Villages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "VillageAreas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "VillageAreas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "VillageAreas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "VillageAreas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "VillageAreas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "VillageAreas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "VillageAreas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "SuspensionReasons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "SuspensionReasons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "SuspensionReasons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "SuspensionReasons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "SuspensionReasons",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "SuspensionReasons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "SuspensionReasons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "StaffMembers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "StaffMembers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "StaffMembers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "StaffMembers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "StaffMembers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "StaffMembers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "StaffMembers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "SeparationReasons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "SeparationReasons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "SeparationReasons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "SeparationReasons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "SeparationReasons",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "SeparationReasons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "SeparationReasons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "InventoryItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "InventoryItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "InventoryItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "InventoryItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "InventoryItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "InventoryItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "InventoryItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Governorates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "Governorates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Governorates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "Governorates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Governorates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Governorates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "Governorates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ExpenseCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "ExpenseCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "ExpenseCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "ExpenseCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ExpenseCategories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "ExpenseCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "ExpenseCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Donations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "Donations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Donations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "Donations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Donations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Donations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "Donations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Districts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "Districts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Districts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "Districts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Districts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Districts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "Districts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Distributions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "Distributions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Distributions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "Distributions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Distributions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Distributions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "Distributions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "CharitySettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "CharitySettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "CharitySettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "CharitySettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "CharitySettings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "CharitySettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "CharitySettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Charities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "Charities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Charities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "Charities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Charities",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Charities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "Charities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "BeneficiaryHeadHistories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "BeneficiaryHeadHistories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "BeneficiaryHeadHistories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "BeneficiaryHeadHistories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "BeneficiaryHeadHistories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "BeneficiaryHeadHistories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "BeneficiaryHeadHistories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "BeneficiaryFamilyMembers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "BeneficiaryFamilyMembers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "BeneficiaryFamilyMembers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "BeneficiaryFamilyMembers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "BeneficiaryFamilyMembers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "BeneficiaryFamilyMembers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "BeneficiaryFamilyMembers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Beneficiaries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "AddressRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "AddressRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "AddressRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByName",
                table: "AddressRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AddressRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "AddressRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "AddressRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CharityId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ActorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CharityId_OccurredAt",
                table: "AuditLogs",
                columns: new[] { "CharityId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CharityId_UserId_Action",
                table: "AuditLogs",
                columns: new[] { "CharityId", "UserId", "Action" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Villages");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "Villages");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Villages");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "Villages");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Villages");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Villages");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "Villages");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "VillageAreas");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "VillageAreas");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "VillageAreas");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "VillageAreas");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "VillageAreas");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "VillageAreas");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "VillageAreas");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SuspensionReasons");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "SuspensionReasons");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "SuspensionReasons");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "SuspensionReasons");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "SuspensionReasons");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "SuspensionReasons");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "SuspensionReasons");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "StaffMembers");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "StaffMembers");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "StaffMembers");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "StaffMembers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "StaffMembers");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "StaffMembers");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "StaffMembers");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SeparationReasons");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "SeparationReasons");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "SeparationReasons");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "SeparationReasons");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "SeparationReasons");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "SeparationReasons");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "SeparationReasons");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Governorates");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "Governorates");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Governorates");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "Governorates");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Governorates");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Governorates");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "Governorates");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ExpenseCategories");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "ExpenseCategories");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "ExpenseCategories");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "ExpenseCategories");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ExpenseCategories");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ExpenseCategories");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "ExpenseCategories");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Districts");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "Districts");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Districts");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "Districts");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Districts");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Districts");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "Districts");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Distributions");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "Distributions");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Distributions");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "Distributions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Distributions");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Distributions");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "Distributions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CharitySettings");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "CharitySettings");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "CharitySettings");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "CharitySettings");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CharitySettings");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "CharitySettings");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "CharitySettings");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "BeneficiaryHeadHistories");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "BeneficiaryHeadHistories");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "BeneficiaryHeadHistories");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "BeneficiaryHeadHistories");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "BeneficiaryHeadHistories");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "BeneficiaryHeadHistories");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "BeneficiaryHeadHistories");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "BeneficiaryFamilyMembers");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "BeneficiaryFamilyMembers");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "BeneficiaryFamilyMembers");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "BeneficiaryFamilyMembers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "BeneficiaryFamilyMembers");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "BeneficiaryFamilyMembers");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "BeneficiaryFamilyMembers");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AddressRecords");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "AddressRecords");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "AddressRecords");

            migrationBuilder.DropColumn(
                name: "DeletedByName",
                table: "AddressRecords");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AddressRecords");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "AddressRecords");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "AddressRecords");
        }
    }
}
