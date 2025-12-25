using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace basics.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerInfoToEtkinlikKoltuk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 24, 13, 19, 24, 182, DateTimeKind.Utc).AddTicks(1301),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 24, 12, 48, 27, 90, DateTimeKind.Utc).AddTicks(6050));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 24, 13, 19, 24, 182, DateTimeKind.Utc).AddTicks(784),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 24, 12, 48, 27, 90, DateTimeKind.Utc).AddTicks(5534));

            migrationBuilder.AddColumn<string>(
                name: "MusteriAdi",
                table: "EtkinlikKoltuklari",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MusteriSoyadi",
                table: "EtkinlikKoltuklari",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MusteriTelefon",
                table: "EtkinlikKoltuklari",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "OdemeYontemi",
                table: "EtkinlikKoltuklari",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "SatisTarihi",
                table: "EtkinlikKoltuklari",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MusteriAdi",
                table: "EtkinlikKoltuklari");

            migrationBuilder.DropColumn(
                name: "MusteriSoyadi",
                table: "EtkinlikKoltuklari");

            migrationBuilder.DropColumn(
                name: "MusteriTelefon",
                table: "EtkinlikKoltuklari");

            migrationBuilder.DropColumn(
                name: "OdemeYontemi",
                table: "EtkinlikKoltuklari");

            migrationBuilder.DropColumn(
                name: "SatisTarihi",
                table: "EtkinlikKoltuklari");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 24, 12, 48, 27, 90, DateTimeKind.Utc).AddTicks(6050),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 24, 13, 19, 24, 182, DateTimeKind.Utc).AddTicks(1301));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 24, 12, 48, 27, 90, DateTimeKind.Utc).AddTicks(5534),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 24, 13, 19, 24, 182, DateTimeKind.Utc).AddTicks(784));
        }
    }
}
