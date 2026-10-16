using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Charity.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFilteredIndexesForSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BeneficiaryFamilyMembers_NationalId",
                table: "BeneficiaryFamilyMembers");

            migrationBuilder.DropIndex(
                name: "IX_Beneficiaries_NationalId",
                table: "Beneficiaries");

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryFamilyMembers_NationalId",
                table: "BeneficiaryFamilyMembers",
                column: "NationalId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiaries_NationalId",
                table: "Beneficiaries",
                column: "NationalId",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BeneficiaryFamilyMembers_NationalId",
                table: "BeneficiaryFamilyMembers");

            migrationBuilder.DropIndex(
                name: "IX_Beneficiaries_NationalId",
                table: "Beneficiaries");

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryFamilyMembers_NationalId",
                table: "BeneficiaryFamilyMembers",
                column: "NationalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiaries_NationalId",
                table: "Beneficiaries",
                column: "NationalId",
                unique: true);
        }
    }
}
