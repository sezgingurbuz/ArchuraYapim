using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace basics.Migrations
{
    /// <inheritdoc />
    public partial class AddSeatingPlanId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 10, 15, 13, 10, 690, DateTimeKind.Utc).AddTicks(4898),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 10, 14, 48, 7, 263, DateTimeKind.Utc).AddTicks(8796));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 10, 15, 13, 10, 690, DateTimeKind.Utc).AddTicks(4388),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 10, 14, 48, 7, 263, DateTimeKind.Utc).AddTicks(8312));

            migrationBuilder.AddColumn<int>(
                name: "SeatingPlanId",
                table: "Salonlar",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("UPDATE Salonlar SET SeatingPlanId = NULL WHERE SeatingPlanId = 0");

            /* Index already exists
            migrationBuilder.CreateIndex(
                name: "IX_Salonlar_SeatingPlanId",
                table: "Salonlar",
                column: "SeatingPlanId");
            */

            migrationBuilder.AddForeignKey(
                name: "FK_Salonlar_SeatingPlans_SeatingPlanId",
                table: "Salonlar",
                column: "SeatingPlanId",
                principalTable: "SeatingPlans",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Salonlar_SeatingPlans_SeatingPlanId",
                table: "Salonlar");

            migrationBuilder.DropIndex(
                name: "IX_Salonlar_SeatingPlanId",
                table: "Salonlar");

            migrationBuilder.DropColumn(
                name: "SeatingPlanId",
                table: "Salonlar");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 10, 14, 48, 7, 263, DateTimeKind.Utc).AddTicks(8796),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 10, 15, 13, 10, 690, DateTimeKind.Utc).AddTicks(4898));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 10, 14, 48, 7, 263, DateTimeKind.Utc).AddTicks(8312),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 10, 15, 13, 10, 690, DateTimeKind.Utc).AddTicks(4388));
        }
    }
}
