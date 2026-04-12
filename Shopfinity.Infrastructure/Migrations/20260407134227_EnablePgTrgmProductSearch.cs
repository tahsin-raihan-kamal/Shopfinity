using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopfinity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnablePgTrgmProductSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // PostgreSQL only: trigram similarity for product search suggestions
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
            migrationBuilder.Sql(
                @"CREATE INDEX IF NOT EXISTS ""IX_Products_Name_trgm"" ON ""Products"" USING gin (""Name"" gin_trgm_ops);");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 7, 13, 42, 25, 555, DateTimeKind.Utc).AddTicks(621));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 7, 13, 42, 25, 555, DateTimeKind.Utc).AddTicks(954));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 7, 13, 42, 25, 555, DateTimeKind.Utc).AddTicks(959));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 7, 13, 42, 25, 555, DateTimeKind.Utc).AddTicks(962));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Products_Name_trgm"";");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 7, 12, 54, 56, 54, DateTimeKind.Utc).AddTicks(2417));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 7, 12, 54, 56, 54, DateTimeKind.Utc).AddTicks(2847));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 7, 12, 54, 56, 54, DateTimeKind.Utc).AddTicks(2854));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 7, 12, 54, 56, 54, DateTimeKind.Utc).AddTicks(2861));
        }
    }
}
