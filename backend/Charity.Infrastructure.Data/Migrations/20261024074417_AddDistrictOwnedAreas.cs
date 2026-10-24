using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Charity.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDistrictOwnedAreas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "VillageId",
                table: "VillageAreas",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "DistrictId",
                table: "VillageAreas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VillageAreas_DistrictId",
                table: "VillageAreas",
                column: "DistrictId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_VillageAreas_ExactlyOneParent",
                table: "VillageAreas",
                sql: "([VillageId] IS NOT NULL AND [DistrictId] IS NULL) OR ([VillageId] IS NULL AND [DistrictId] IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_VillageAreas_Districts_DistrictId",
                table: "VillageAreas",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VillageAreas_Districts_DistrictId",
                table: "VillageAreas");

            migrationBuilder.DropIndex(
                name: "IX_VillageAreas_DistrictId",
                table: "VillageAreas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_VillageAreas_ExactlyOneParent",
                table: "VillageAreas");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "VillageAreas");

            migrationBuilder.AlterColumn<string>(
                name: "VillageId",
                table: "VillageAreas",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
