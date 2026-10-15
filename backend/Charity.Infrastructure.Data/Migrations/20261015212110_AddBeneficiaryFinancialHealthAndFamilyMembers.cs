using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Charity.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBeneficiaryFinancialHealthAndFamilyMembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Beneficiaries_NationalId",
                table: "Beneficiaries");

            migrationBuilder.AddColumn<string>(
                name: "ChronicDiseaseDetails",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GovernmentPensionDetails",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "HasChronicDisease",
                table: "Beneficiaries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasGovernmentPension",
                table: "Beneficiaries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasLandOrProperty",
                table: "Beneficiaries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "IncomeSource",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LandOrPropertyDetails",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MedicationFrequency",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyIncome",
                table: "Beneficiaries",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NeedsPeriodicMedication",
                table: "Beneficiaries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PeriodicMedicationDetails",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "BeneficiaryFamilyMembers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BeneficiaryId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NationalId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Relationship = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MemberType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HasIncome = table.Column<bool>(type: "bit", nullable: false),
                    IncomeAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    IncomeSource = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupportType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HealthStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeneficiaryFamilyMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BeneficiaryFamilyMembers_Beneficiaries_BeneficiaryId",
                        column: x => x.BeneficiaryId,
                        principalTable: "Beneficiaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiaries_NationalId",
                table: "Beneficiaries",
                column: "NationalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryFamilyMembers_BeneficiaryId",
                table: "BeneficiaryFamilyMembers",
                column: "BeneficiaryId");

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryFamilyMembers_NationalId",
                table: "BeneficiaryFamilyMembers",
                column: "NationalId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BeneficiaryFamilyMembers");

            migrationBuilder.DropIndex(
                name: "IX_Beneficiaries_NationalId",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "ChronicDiseaseDetails",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "GovernmentPensionDetails",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "HasChronicDisease",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "HasGovernmentPension",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "HasLandOrProperty",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "IncomeSource",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "LandOrPropertyDetails",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "MedicationFrequency",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "MonthlyIncome",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "NeedsPeriodicMedication",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "PeriodicMedicationDetails",
                table: "Beneficiaries");

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiaries_NationalId",
                table: "Beneficiaries",
                column: "NationalId");
        }
    }
}
