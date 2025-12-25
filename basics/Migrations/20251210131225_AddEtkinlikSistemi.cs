using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace basics.Migrations
{
    /// <inheritdoc />
    public partial class AddEtkinlikSistemi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 10, 13, 12, 24, 19, DateTimeKind.Utc).AddTicks(6954),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 9, 19, 20, 11, 668, DateTimeKind.Utc).AddTicks(5813));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 10, 13, 12, 24, 19, DateTimeKind.Utc).AddTicks(6460),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 9, 19, 20, 11, 668, DateTimeKind.Utc).AddTicks(5309));

            migrationBuilder.CreateTable(
                name: "Etkinlikler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EtkinlikAdi = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SalonId = table.Column<int>(type: "int", nullable: false),
                    TarihSaat = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Tur = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BiletFiyati = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    SatisAktifMi = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Etkinlikler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Etkinlikler_Salonlar_SalonId",
                        column: x => x.SalonId,
                        principalTable: "Salonlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EtkinlikKoltuklari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EtkinlikId = table.Column<int>(type: "int", nullable: false),
                    KoltukNo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Blok = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Sira = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Numara = table.Column<int>(type: "int", nullable: false),
                    DoluMu = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Fiyat = table.Column<decimal>(type: "decimal(65,30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EtkinlikKoltuklari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EtkinlikKoltuklari_Etkinlikler_EtkinlikId",
                        column: x => x.EtkinlikId,
                        principalTable: "Etkinlikler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_EtkinlikKoltuklari_EtkinlikId",
                table: "EtkinlikKoltuklari",
                column: "EtkinlikId");

            migrationBuilder.CreateIndex(
                name: "IX_Etkinlikler_SalonId",
                table: "Etkinlikler",
                column: "SalonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EtkinlikKoltuklari");

            migrationBuilder.DropTable(
                name: "Etkinlikler");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 9, 19, 20, 11, 668, DateTimeKind.Utc).AddTicks(5813),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 10, 13, 12, 24, 19, DateTimeKind.Utc).AddTicks(6954));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 9, 19, 20, 11, 668, DateTimeKind.Utc).AddTicks(5309),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 10, 13, 12, 24, 19, DateTimeKind.Utc).AddTicks(6460));
        }
    }
}
