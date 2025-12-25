using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace basics.Migrations
{
    /// <inheritdoc />
    public partial class FixDecimalPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 10, 14, 48, 7, 263, DateTimeKind.Utc).AddTicks(8796),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 10, 13, 12, 24, 19, DateTimeKind.Utc).AddTicks(6954));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 10, 14, 48, 7, 263, DateTimeKind.Utc).AddTicks(8312),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 10, 13, 12, 24, 19, DateTimeKind.Utc).AddTicks(6460));

            migrationBuilder.AlterColumn<decimal>(
                name: "BiletFiyati",
                table: "Etkinlikler",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Fiyat",
                table: "EtkinlikKoltuklari",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 10, 13, 12, 24, 19, DateTimeKind.Utc).AddTicks(6954),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 10, 14, 48, 7, 263, DateTimeKind.Utc).AddTicks(8796));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 10, 13, 12, 24, 19, DateTimeKind.Utc).AddTicks(6460),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 10, 14, 48, 7, 263, DateTimeKind.Utc).AddTicks(8312));

            migrationBuilder.AlterColumn<decimal>(
                name: "BiletFiyati",
                table: "Etkinlikler",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Fiyat",
                table: "EtkinlikKoltuklari",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}
