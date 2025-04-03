using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EonWatchesAPI.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Ads",
                columns: new[] { "Id", "Archived", "Brand", "CreatedAt", "Currency", "GroupId", "IsAnSeller", "Model", "Price", "ReferenceNumber" },
                values: new object[,]
                {
                    { 1, false, "Omega", new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "USD", "G001", true, "Speedmaster", 5999.99m, "311.30.42.30.01.005" },
                    { 2, false, "Rolex", new DateTime(2025, 2, 20, 12, 30, 0, 0, DateTimeKind.Utc), "USD", "G002", false, "Submariner", 8999.50m, "124060" },
                    { 3, true, "Seiko", new DateTime(2024, 12, 1, 8, 0, 0, 0, DateTimeKind.Utc), "EUR", "G003", true, "Prospex", 450.00m, "SRPD21" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Ads",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Ads",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Ads",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
