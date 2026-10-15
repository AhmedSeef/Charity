using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Charity.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSuspensionReasonsAndBeneficiarySuspension : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSeparated",
                table: "BeneficiaryFamilyMembers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SeparatedAt",
                table: "BeneficiaryFamilyMembers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeparatedToBeneficiaryId",
                table: "BeneficiaryFamilyMembers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeparationReason",
                table: "BeneficiaryFamilyMembers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSuspended",
                table: "Beneficiaries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SuspendedAt",
                table: "Beneficiaries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuspendedByCharityId",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuspendedByCharityName",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuspensionNotes",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuspensionReasonId",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuspensionReasonTitle",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SuspensionReasons",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CharityId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CharityName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuspensionReasons", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SuspensionReasons");

            migrationBuilder.DropColumn(
                name: "IsSeparated",
                table: "BeneficiaryFamilyMembers");

            migrationBuilder.DropColumn(
                name: "SeparatedAt",
                table: "BeneficiaryFamilyMembers");

            migrationBuilder.DropColumn(
                name: "SeparatedToBeneficiaryId",
                table: "BeneficiaryFamilyMembers");

            migrationBuilder.DropColumn(
                name: "SeparationReason",
                table: "BeneficiaryFamilyMembers");

            migrationBuilder.DropColumn(
                name: "IsSuspended",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "SuspendedAt",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "SuspendedByCharityId",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "SuspendedByCharityName",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "SuspensionNotes",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "SuspensionReasonId",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "SuspensionReasonTitle",
                table: "Beneficiaries");
        }
    }
}
