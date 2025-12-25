using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace basics.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 24, 15, 12, 59, 55, DateTimeKind.Utc).AddTicks(3888),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 24, 13, 32, 6, 785, DateTimeKind.Utc).AddTicks(6359));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 24, 15, 12, 59, 55, DateTimeKind.Utc).AddTicks(3374),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 24, 13, 32, 6, 785, DateTimeKind.Utc).AddTicks(5818));

            migrationBuilder.CreateTable(
                name: "AdminUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    userName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    passwordHash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminUsers", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminUsers");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 24, 13, 32, 6, 785, DateTimeKind.Utc).AddTicks(6359),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 24, 15, 12, 59, 55, DateTimeKind.Utc).AddTicks(3888));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 24, 13, 32, 6, 785, DateTimeKind.Utc).AddTicks(5818),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 24, 15, 12, 59, 55, DateTimeKind.Utc).AddTicks(3374));
        }
    }
}
