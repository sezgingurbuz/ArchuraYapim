using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace basics.Migrations
{
    /// <inheritdoc />
    public partial class AddKategoriSatisSistemi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2026, 1, 3, 19, 21, 46, 191, DateTimeKind.Utc).AddTicks(4385),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2026, 1, 1, 12, 59, 32, 623, DateTimeKind.Utc).AddTicks(7765));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2026, 1, 3, 19, 21, 46, 191, DateTimeKind.Utc).AddTicks(3393),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2026, 1, 1, 12, 59, 32, 623, DateTimeKind.Utc).AddTicks(6990));

            migrationBuilder.AddColumn<string>(
                name: "SatisTipi",
                table: "Etkinlikler",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EtkinlikKategorileri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EtkinlikId = table.Column<int>(type: "int", nullable: false),
                    KategoriAdi = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Fiyat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Kontenjan = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EtkinlikKategorileri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EtkinlikKategorileri_Etkinlikler_EtkinlikId",
                        column: x => x.EtkinlikId,
                        principalTable: "Etkinlikler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "KategoriBiletler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EtkinlikKategoriId = table.Column<int>(type: "int", nullable: false),
                    RezervasyonKodu = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    MusteriAdi = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MusteriSoyadi = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MusteriTelefon = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MusteriEmail = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SatisTarihi = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    OdemeYontemi = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OdenenFiyat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AtananKoltukId = table.Column<int>(type: "int", nullable: true),
                    KoltukAtandiMi = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    KoltukAtamaTarihi = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    BiletKodu = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    GirisYapildiMi = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KategoriBiletler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KategoriBiletler_EtkinlikKategorileri_EtkinlikKategoriId",
                        column: x => x.EtkinlikKategoriId,
                        principalTable: "EtkinlikKategorileri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KategoriBiletler_EtkinlikKoltuklari_AtananKoltukId",
                        column: x => x.AtananKoltukId,
                        principalTable: "EtkinlikKoltuklari",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_EtkinlikKategorileri_EtkinlikId",
                table: "EtkinlikKategorileri",
                column: "EtkinlikId");

            migrationBuilder.CreateIndex(
                name: "IX_KategoriBiletler_AtananKoltukId",
                table: "KategoriBiletler",
                column: "AtananKoltukId");

            migrationBuilder.CreateIndex(
                name: "IX_KategoriBiletler_EtkinlikKategoriId",
                table: "KategoriBiletler",
                column: "EtkinlikKategoriId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KategoriBiletler");

            migrationBuilder.DropTable(
                name: "EtkinlikKategorileri");

            migrationBuilder.DropColumn(
                name: "SatisTipi",
                table: "Etkinlikler");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2026, 1, 1, 12, 59, 32, 623, DateTimeKind.Utc).AddTicks(7765),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2026, 1, 3, 19, 21, 46, 191, DateTimeKind.Utc).AddTicks(4385));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2026, 1, 1, 12, 59, 32, 623, DateTimeKind.Utc).AddTicks(6990),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2026, 1, 3, 19, 21, 46, 191, DateTimeKind.Utc).AddTicks(3393));
        }
    }
}
