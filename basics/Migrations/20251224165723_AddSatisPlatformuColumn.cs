using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace basics.Migrations
{
    /// <inheritdoc />
    public partial class AddSatisPlatformuColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 24, 16, 57, 23, 221, DateTimeKind.Utc).AddTicks(5005),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 24, 15, 12, 59, 55, DateTimeKind.Utc).AddTicks(3888));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 24, 16, 57, 23, 221, DateTimeKind.Utc).AddTicks(4492),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 24, 15, 12, 59, 55, DateTimeKind.Utc).AddTicks(3374));

            migrationBuilder.AddColumn<string>(
                name: "SatisPlatformu",
                table: "EtkinlikKoltuklari",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SatisPlatformu",
                table: "EtkinlikKoltuklari");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 24, 15, 12, 59, 55, DateTimeKind.Utc).AddTicks(3888),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 24, 16, 57, 23, 221, DateTimeKind.Utc).AddTicks(5005));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 24, 15, 12, 59, 55, DateTimeKind.Utc).AddTicks(3374),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 24, 16, 57, 23, 221, DateTimeKind.Utc).AddTicks(4492));
        }
    }
}
