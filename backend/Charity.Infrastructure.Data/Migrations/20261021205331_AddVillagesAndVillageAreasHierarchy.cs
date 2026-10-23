using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Charity.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVillagesAndVillageAreasHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VillageAreaId",
                table: "Beneficiaries",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VillageAreaName",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VillageId",
                table: "Beneficiaries",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VillageName",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Villages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DistrictId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CharityId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CharityName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Villages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Villages_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VillageAreas",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VillageId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CharityId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CharityName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VillageAreas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VillageAreas_Villages_VillageId",
                        column: x => x.VillageId,
                        principalTable: "Villages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiaries_VillageAreaId",
                table: "Beneficiaries",
                column: "VillageAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiaries_VillageId",
                table: "Beneficiaries",
                column: "VillageId");

            migrationBuilder.CreateIndex(
                name: "IX_VillageAreas_VillageId",
                table: "VillageAreas",
                column: "VillageId");

            migrationBuilder.CreateIndex(
                name: "IX_Villages_DistrictId",
                table: "Villages",
                column: "DistrictId");

            migrationBuilder.AddForeignKey(
                name: "FK_Beneficiaries_VillageAreas_VillageAreaId",
                table: "Beneficiaries",
                column: "VillageAreaId",
                principalTable: "VillageAreas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Beneficiaries_Villages_VillageId",
                table: "Beneficiaries",
                column: "VillageId",
                principalTable: "Villages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Beneficiaries_VillageAreas_VillageAreaId",
                table: "Beneficiaries");

            migrationBuilder.DropForeignKey(
                name: "FK_Beneficiaries_Villages_VillageId",
                table: "Beneficiaries");

            migrationBuilder.DropTable(
                name: "VillageAreas");

            migrationBuilder.DropTable(
                name: "Villages");

            migrationBuilder.DropIndex(
                name: "IX_Beneficiaries_VillageAreaId",
                table: "Beneficiaries");

            migrationBuilder.DropIndex(
                name: "IX_Beneficiaries_VillageId",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "VillageAreaId",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "VillageAreaName",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "VillageId",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "VillageName",
                table: "Beneficiaries");
        }
    }
}
