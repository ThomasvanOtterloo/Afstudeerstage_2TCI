using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EonWatchesAPI.Migrations
{
    /// <inheritdoc />
    public partial class addedMoreClassesAndAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Ads",
                newName: "GroupId");

            migrationBuilder.AddColumn<bool>(
                name: "Archived",
                table: "Ads",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Ads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Ads",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Ads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAnSeller",
                table: "Ads",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Ads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Ads",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceNumber",
                table: "Ads",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Archived",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "IsAnSeller",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "ReferenceNumber",
                table: "Ads");

            migrationBuilder.RenameColumn(
                name: "GroupId",
                table: "Ads",
                newName: "Name");
        }
    }
}
