using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cpcApi.Migrations
{
    /// <inheritdoc />
    public partial class create_pengembalian_kaset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PengembalianKaset",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OrderPengisianId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NomorMesin = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Lokasi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MerekMesin = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    KDBANK = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    KDCABANG = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Jumlah = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TanggalTerima = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DiterimaOleh = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Catatan = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    created = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updatedat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PengembalianKaset", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PengembalianKasetDetail",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PengembalianId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Kaset = table.Column<int>(type: "int", nullable: false),
                    KodeKaset = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NoSeal = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Denom = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Lembar = table.Column<int>(type: "int", nullable: false),
                    KasetStockIdKaset = table.Column<string>(type: "nvarchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PengembalianKasetDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PengembalianKasetDetail_KasetStock_KasetStockIdKaset",
                        column: x => x.KasetStockIdKaset,
                        principalTable: "KasetStock",
                        principalColumn: "IdKaset");
                    table.ForeignKey(
                        name: "FK_PengembalianKasetDetail_PengembalianKaset_PengembalianId",
                        column: x => x.PengembalianId,
                        principalTable: "PengembalianKaset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PengembalianKasetDetail_KasetStockIdKaset",
                table: "PengembalianKasetDetail",
                column: "KasetStockIdKaset");

            migrationBuilder.CreateIndex(
                name: "IX_PengembalianKasetDetail_PengembalianId",
                table: "PengembalianKasetDetail",
                column: "PengembalianId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PengembalianKasetDetail");

            migrationBuilder.DropTable(
                name: "PengembalianKaset");
        }
    }
}
