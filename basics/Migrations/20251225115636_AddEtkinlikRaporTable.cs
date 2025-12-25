using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace basics.Migrations
{
    /// <inheritdoc />
    public partial class AddEtkinlikRaporTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 25, 11, 56, 34, 562, DateTimeKind.Utc).AddTicks(4720),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 24, 16, 57, 23, 221, DateTimeKind.Utc).AddTicks(5005));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 25, 11, 56, 34, 562, DateTimeKind.Utc).AddTicks(4218),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 24, 16, 57, 23, 221, DateTimeKind.Utc).AddTicks(4492));

            migrationBuilder.CreateTable(
                name: "EtkinlikRaporlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EtkinlikAdi = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tur = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SalonAdi = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Sehir = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TarihSaat = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ToplamKapasite = table.Column<int>(type: "int", nullable: false),
                    SatilanBilet = table.Column<int>(type: "int", nullable: false),
                    BosKoltuk = table.Column<int>(type: "int", nullable: false),
                    ToplamHasilat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BubiletSatisAdedi = table.Column<int>(type: "int", nullable: false),
                    BiletinialSatisAdedi = table.Column<int>(type: "int", nullable: false),
                    NakitSatisAdedi = table.Column<int>(type: "int", nullable: false),
                    NakitHasilat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KartSatisAdedi = table.Column<int>(type: "int", nullable: false),
                    KartHasilat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EFTSatisAdedi = table.Column<int>(type: "int", nullable: false),
                    EFTHasilat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RaporTarihi = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RaporlayanKullanici = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EtkinlikRaporlari", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EtkinlikRaporlari");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 24, 16, 57, 23, 221, DateTimeKind.Utc).AddTicks(5005),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 25, 11, 56, 34, 562, DateTimeKind.Utc).AddTicks(4720));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SeatingPlans",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 24, 16, 57, 23, 221, DateTimeKind.Utc).AddTicks(4492),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 12, 25, 11, 56, 34, 562, DateTimeKind.Utc).AddTicks(4218));
        }
    }
}
