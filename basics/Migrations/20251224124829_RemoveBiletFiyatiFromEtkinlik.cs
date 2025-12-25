using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace basics.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBiletFiyatiFromEtkinlik : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KoltukDuzeni",
                table: "Salonlar");

            migrationBuilder.DropColumn(
                name: "BiletFiyati",
                table: "Etkinlikler");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 24, 12, 48, 27, 90, DateTimeKind.Utc).AddTicks(6050),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 10, 15, 13, 10, 690, DateTimeKind.Utc).AddTicks(4898));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 24, 12, 48, 27, 90, DateTimeKind.Utc).AddTicks(5534),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 10, 15, 13, 10, 690, DateTimeKind.Utc).AddTicks(4388));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 10, 15, 13, 10, 690, DateTimeKind.Utc).AddTicks(4898),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 24, 12, 48, 27, 90, DateTimeKind.Utc).AddTicks(6050));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 10, 15, 13, 10, 690, DateTimeKind.Utc).AddTicks(4388),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 24, 12, 48, 27, 90, DateTimeKind.Utc).AddTicks(5534));

            migrationBuilder.AddColumn<string>(
                name: "KoltukDuzeni",
                table: "Salonlar",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "BiletFiyati",
                table: "Etkinlikler",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
