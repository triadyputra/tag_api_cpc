using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cpcApi.Migrations
{
    /// <inheritdoc />
    public partial class alter_table_stock_add_filter_saldo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MUTASIVAULT_CABANG",
                table: "MutasiVault");

            migrationBuilder.DropIndex(
                name: "IX_MUTASIVAULT_CABANG_CREATED",
                table: "MutasiVault");

            migrationBuilder.DropIndex(
                name: "IX_MUTASIVAULT_CREATED",
                table: "MutasiVault");

            migrationBuilder.CreateIndex(
                name: "IX_MUTASIVAULT_HISTORY",
                table: "MutasiVault",
                columns: new[] { "KdCabang", "Nominal", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "UX_MUTASIVAULT_SALDOAWAL",
                table: "MutasiVault",
                columns: new[] { "KdCabang", "Nominal" },
                unique: true,
                filter: "[TipeMutasi] = 'SALDO_AWAL'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MUTASIVAULT_HISTORY",
                table: "MutasiVault");

            migrationBuilder.DropIndex(
                name: "UX_MUTASIVAULT_SALDOAWAL",
                table: "MutasiVault");

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
        }
    }
}
