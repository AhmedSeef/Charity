using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Charity.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDistributionRecipientDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReceivedByMemberId",
                table: "Distributions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceivedByName",
                table: "Distributions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceivedByNationalId",
                table: "Distributions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceivedByRelationship",
                table: "Distributions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceivedByMemberId",
                table: "Distributions");

            migrationBuilder.DropColumn(
                name: "ReceivedByName",
                table: "Distributions");

            migrationBuilder.DropColumn(
                name: "ReceivedByNationalId",
                table: "Distributions");

            migrationBuilder.DropColumn(
                name: "ReceivedByRelationship",
                table: "Distributions");
        }
    }
}
