using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopfinity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_AspNetUsers_UserId",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("117139e5-c54b-4d14-8fcc-4b87403b9d83"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("5d1ea250-937f-47e3-bbcf-610e469509cc"));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "RefreshTokens",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "RefreshTokens",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "RefreshTokens",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "15d6e58f-beec-4fde-a8cd-9624513afa98", "AQAAAAIAAYagAAAAEBm5xplkh7R24pFKYcHx99AHGmXU19ZLoNPEO6C1CCpH1j8/WdhctlPVsxGmFWTZ2w==", "89454d1a-1b06-49c9-8660-b34f7196c110" });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "ImageUrl", "IsDeleted", "Name", "UpdatedAt" },
                values: new object[] { new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tech gear", null, false, "Electronics", null });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "Description", "IsDeleted", "Name", "Price", "Slug", "StockQuantity", "UpdatedAt" },
                values: new object[] { new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"), new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "High end laptop", false, "Sample Laptop", 999.99m, "sample-laptop", 10, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"));

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "RefreshTokens");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "476a47c4-ae8e-438e-b7ab-6d86cf717f7d", "AQAAAAIAAYagAAAAELE92GdhuNNXMY2JsJImyusbItZYociKeLtaoB8scFUqCglwltvSVWSGvAnLQadDYQ==", "a7093e94-0d13-455d-b5b2-9b1e8c634085" });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "ImageUrl", "IsDeleted", "Name", "UpdatedAt" },
                values: new object[] { new Guid("5d1ea250-937f-47e3-bbcf-610e469509cc"), new DateTime(2026, 4, 5, 11, 59, 8, 954, DateTimeKind.Utc).AddTicks(880), "Tech gear", null, false, "Electronics", null });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "Description", "IsDeleted", "Name", "Price", "Slug", "StockQuantity", "UpdatedAt" },
                values: new object[] { new Guid("117139e5-c54b-4d14-8fcc-4b87403b9d83"), new Guid("5d1ea250-937f-47e3-bbcf-610e469509cc"), new DateTime(2026, 4, 5, 11, 59, 8, 954, DateTimeKind.Utc).AddTicks(4354), "High end laptop", false, "Sample Laptop", 999.99m, "sample-laptop", 10, null });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_AspNetUsers_UserId",
                table: "RefreshTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
