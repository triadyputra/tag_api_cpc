using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cpcApi.Migrations
{
    /// <inheritdoc />
    public partial class create_table_proses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProsesPersiapanUangCpc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KodeBank = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TanggalProses = table.Column<DateOnly>(type: "date", nullable: false),
                    JamMulai = table.Column<TimeOnly>(type: "time", nullable: false),
                    JenisProses = table.Column<int>(type: "int", nullable: false),
                    NomorDvr = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Meja = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    JamSelesai = table.Column<TimeOnly>(type: "time", nullable: false),
                    NamaPetugas = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    JabatanPetugas = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PathTtdPetugas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TanggalDibuat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TanggalFinal = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProsesPersiapanUangCpc", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProsesSetPersiapanUangCpc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProsesPersiapanUangCpcId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SetKe = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProsesSetPersiapanUangCpc", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProsesSetPersiapanUangCpc_ProsesPersiapanUangCpc_ProsesPersiapanUangCpcId",
                        column: x => x.ProsesPersiapanUangCpcId,
                        principalTable: "ProsesPersiapanUangCpc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProsesKotakUangCpc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProsesSetPersiapanUangCpcId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UrutanKolom = table.Column<int>(type: "int", nullable: false),
                    NomorKotakUang = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NomorSeal = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    JumlahLembar = table.Column<int>(type: "int", nullable: true),
                    JenisUang = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProsesKotakUangCpc", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProsesKotakUangCpc_ProsesSetPersiapanUangCpc_ProsesSetPersiapanUangCpcId",
                        column: x => x.ProsesSetPersiapanUangCpcId,
                        principalTable: "ProsesSetPersiapanUangCpc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UX_KOTAK_CPC_SET_URUTAN",
                table: "ProsesKotakUangCpc",
                columns: new[] { "ProsesSetPersiapanUangCpcId", "UrutanKolom" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PROSES_CPC_BANK_TANGGAL",
                table: "ProsesPersiapanUangCpc",
                columns: new[] { "KodeBank", "TanggalProses" });

            migrationBuilder.CreateIndex(
                name: "IX_PROSES_CPC_STATUS",
                table: "ProsesPersiapanUangCpc",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "UX_SET_CPC_PROSES_SETKE",
                table: "ProsesSetPersiapanUangCpc",
                columns: new[] { "ProsesPersiapanUangCpcId", "SetKe" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProsesKotakUangCpc");

            migrationBuilder.DropTable(
                name: "ProsesSetPersiapanUangCpc");

            migrationBuilder.DropTable(
                name: "ProsesPersiapanUangCpc");
        }
    }
}
