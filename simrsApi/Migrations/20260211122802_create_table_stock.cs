using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cpcApi.Migrations
{
    /// <inheritdoc />
    public partial class create_table_stock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MutasiVault",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KdCabang = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Nominal = table.Column<int>(type: "int", nullable: false),
                    QtyLembar = table.Column<long>(type: "bigint", nullable: false),
                    TipeMutasi = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ReferenceNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SaldoSetelah = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MutasiVault", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StokVaultCabang",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KdCabang = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Nominal = table.Column<int>(type: "int", nullable: false),
                    SaldoLembar = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StokVaultCabang", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MUTASIVAULT_CABANG",
                table: "MutasiVault",
                column: "KdCabang");

            migrationBuilder.CreateIndex(
                name: "IX_MUTASIVAULT_CABANG_CREATED",
                table: "MutasiVault",
                columns: new[] { "KdCabang", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MUTASIVAULT_CREATED",
                table: "MutasiVault",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_STOKVAULT_CABANG",
                table: "StokVaultCabang",
                column: "KdCabang");

            migrationBuilder.CreateIndex(
                name: "UX_STOKVAULT_CABANG_NOMINAL",
                table: "StokVaultCabang",
                columns: new[] { "KdCabang", "Nominal" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MutasiVault");

            migrationBuilder.DropTable(
                name: "StokVaultCabang");
        }
    }
}
