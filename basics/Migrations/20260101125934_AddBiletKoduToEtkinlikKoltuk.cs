using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace basics.Migrations
{
    /// <inheritdoc />
    public partial class AddBiletKoduToEtkinlikKoltuk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2026, 1, 1, 12, 59, 32, 623, DateTimeKind.Utc).AddTicks(7765),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2026, 1, 1, 11, 22, 57, 507, DateTimeKind.Utc).AddTicks(1338));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2026, 1, 1, 12, 59, 32, 623, DateTimeKind.Utc).AddTicks(6990),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2026, 1, 1, 11, 22, 57, 506, DateTimeKind.Utc).AddTicks(8442));

            migrationBuilder.AddColumn<Guid>(
                name: "BiletKodu",
                table: "EtkinlikKoltuklari",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
                name: "GirisYapildiMi",
                table: "EtkinlikKoltuklari",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BiletKodu",
                table: "EtkinlikKoltuklari");

            migrationBuilder.DropColumn(
                name: "GirisYapildiMi",
                table: "EtkinlikKoltuklari");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2026, 1, 1, 11, 22, 57, 507, DateTimeKind.Utc).AddTicks(1338),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2026, 1, 1, 12, 59, 32, 623, DateTimeKind.Utc).AddTicks(7765));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2026, 1, 1, 11, 22, 57, 506, DateTimeKind.Utc).AddTicks(8442),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2026, 1, 1, 12, 59, 32, 623, DateTimeKind.Utc).AddTicks(6990));
        }
    }
}
