using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Charity.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExpenseCategoriesAndLinkDisbursements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExpenseCategoryId",
                table: "Distributions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExpenseCategoryName",
                table: "Distributions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ExpenseCategories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CharityId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CharityName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseCategories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Distributions_ExpenseCategoryId",
                table: "Distributions",
                column: "ExpenseCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Distributions_ExpenseCategories_ExpenseCategoryId",
                table: "Distributions",
                column: "ExpenseCategoryId",
                principalTable: "ExpenseCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Distributions_ExpenseCategories_ExpenseCategoryId",
                table: "Distributions");

            migrationBuilder.DropTable(
                name: "ExpenseCategories");

            migrationBuilder.DropIndex(
                name: "IX_Distributions_ExpenseCategoryId",
                table: "Distributions");

            migrationBuilder.DropColumn(
                name: "ExpenseCategoryId",
                table: "Distributions");

            migrationBuilder.DropColumn(
                name: "ExpenseCategoryName",
                table: "Distributions");
        }
    }
}
