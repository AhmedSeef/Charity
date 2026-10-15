using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Charity.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFamilyFileHeadshipTransferAndSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SuspensionReasons",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SuspensionReasons",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "StaffMembers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "StaffMembers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "InventoryItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "InventoryItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Donations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Donations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Distributions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Distributions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CharitySettings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CharitySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Charities",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Charities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "BeneficiaryFamilyMembers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "BeneficiaryFamilyMembers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFormerHead",
                table: "BeneficiaryFamilyMembers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Beneficiaries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FamilyCode",
                table: "Beneficiaries",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FamilyName",
                table: "Beneficiaries",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Beneficiaries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "BeneficiaryHeadHistories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BeneficiaryId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PreviousHeadName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PreviousHeadNationalId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NewHeadName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NewHeadNationalId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    TransferredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransferredByCharityId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TransferredByCharityName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeneficiaryHeadHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BeneficiaryHeadHistories_Beneficiaries_BeneficiaryId",
                        column: x => x.BeneficiaryId,
                        principalTable: "Beneficiaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiaries_FamilyCode",
                table: "Beneficiaries",
                column: "FamilyCode");

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryHeadHistories_BeneficiaryId",
                table: "BeneficiaryHeadHistories",
                column: "BeneficiaryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BeneficiaryHeadHistories");

            migrationBuilder.DropIndex(
                name: "IX_Beneficiaries_FamilyCode",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SuspensionReasons");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SuspensionReasons");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "StaffMembers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "StaffMembers");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Distributions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Distributions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CharitySettings");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CharitySettings");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "BeneficiaryFamilyMembers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "BeneficiaryFamilyMembers");

            migrationBuilder.DropColumn(
                name: "IsFormerHead",
                table: "BeneficiaryFamilyMembers");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "FamilyCode",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "FamilyName",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Beneficiaries");
        }
    }
}
