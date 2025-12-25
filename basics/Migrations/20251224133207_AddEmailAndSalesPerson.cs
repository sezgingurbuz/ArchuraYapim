using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace basics.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailAndSalesPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 24, 13, 32, 6, 785, DateTimeKind.Utc).AddTicks(6359),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 24, 13, 19, 24, 182, DateTimeKind.Utc).AddTicks(1301));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 24, 13, 32, 6, 785, DateTimeKind.Utc).AddTicks(5818),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 24, 13, 19, 24, 182, DateTimeKind.Utc).AddTicks(784));

            migrationBuilder.AddColumn<string>(
                name: "MusteriEmail",
                table: "EtkinlikKoltuklari",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SatisYapanKullanici",
                table: "EtkinlikKoltuklari",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MusteriEmail",
                table: "EtkinlikKoltuklari");

            migrationBuilder.DropColumn(
                name: "SatisYapanKullanici",
                table: "EtkinlikKoltuklari");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 24, 13, 19, 24, 182, DateTimeKind.Utc).AddTicks(1301),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 24, 13, 32, 6, 785, DateTimeKind.Utc).AddTicks(6359));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 24, 13, 19, 24, 182, DateTimeKind.Utc).AddTicks(784),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 24, 13, 32, 6, 785, DateTimeKind.Utc).AddTicks(5818));
        }
    }
}
