using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cpcApi.Migrations
{
    /// <inheritdoc />
    public partial class alter_table_order_cpc_add_status : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderPengisianKasetDetail_KasetStock_KodeKaset",
                table: "OrderPengisianKasetDetail");

            migrationBuilder.DropIndex(
                name: "UX_STOKVAULT_CABANG_NOMINAL",
                table: "StokVaultCabang");

            migrationBuilder.DropIndex(
                name: "IX_OrderPengisianKasetDetail_OrderId",
                table: "OrderPengisianKasetDetail");

            migrationBuilder.DropIndex(
                name: "IX_MUTASIVAULT_HISTORY",
                table: "MutasiVault");

            migrationBuilder.DropIndex(
                name: "UX_MUTASIVAULT_SALDOAWAL",
                table: "MutasiVault");

            migrationBuilder.AddColumn<string>(
                name: "KdBank",
                table: "StokVaultCabang",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "OrderPengisianKaset",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "OrderPengisianKaset",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "DRAFT");

            migrationBuilder.AddColumn<string>(
                name: "KdBank",
                table: "MutasiVault",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "UX_STOKVAULT_CABANG_BANK_NOMINAL",
                table: "StokVaultCabang",
                columns: new[] { "KdCabang", "KdBank", "Nominal" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderPengisianKasetDetail_OrderId_Kaset",
                table: "OrderPengisianKasetDetail",
                columns: new[] { "OrderId", "Kaset" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderPengisianKasetDetail_OrderId_KodeKaset",
                table: "OrderPengisianKasetDetail",
                columns: new[] { "OrderId", "KodeKaset" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderDetail_Denom_Positive",
                table: "OrderPengisianKasetDetail",
                sql: "[Denom] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderDetail_Lembar_Positive",
                table: "OrderPengisianKasetDetail",
                sql: "[Lembar] >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_OrderPengisianKaset_KDCABANG_KDBANK",
                table: "OrderPengisianKaset",
                columns: new[] { "KDCABANG", "KDBANK" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderPengisianKaset_Status",
                table: "OrderPengisianKaset",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MUTASIVAULT_HISTORY",
                table: "MutasiVault",
                columns: new[] { "KdCabang", "KdBank", "Nominal", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MUTASIVAULT_REF_TIPE",
                table: "MutasiVault",
                columns: new[] { "ReferenceNo", "TipeMutasi" });

            migrationBuilder.CreateIndex(
                name: "UX_MUTASIVAULT_SALDOAWAL",
                table: "MutasiVault",
                columns: new[] { "KdCabang", "KdBank", "Nominal" },
                unique: true,
                filter: "[TipeMutasi] = 'SALDO_AWAL'");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderPengisianKasetDetail_KasetStock_KodeKaset",
                table: "OrderPengisianKasetDetail",
                column: "KodeKaset",
                principalTable: "KasetStock",
                principalColumn: "KdKaset",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderPengisianKasetDetail_KasetStock_KodeKaset",
                table: "OrderPengisianKasetDetail");

            migrationBuilder.DropIndex(
                name: "UX_STOKVAULT_CABANG_BANK_NOMINAL",
                table: "StokVaultCabang");

            migrationBuilder.DropIndex(
                name: "IX_OrderPengisianKasetDetail_OrderId_Kaset",
                table: "OrderPengisianKasetDetail");

            migrationBuilder.DropIndex(
                name: "IX_OrderPengisianKasetDetail_OrderId_KodeKaset",
                table: "OrderPengisianKasetDetail");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderDetail_Denom_Positive",
                table: "OrderPengisianKasetDetail");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderDetail_Lembar_Positive",
                table: "OrderPengisianKasetDetail");

            migrationBuilder.DropIndex(
                name: "IX_OrderPengisianKaset_KDCABANG_KDBANK",
                table: "OrderPengisianKaset");

            migrationBuilder.DropIndex(
                name: "IX_OrderPengisianKaset_Status",
                table: "OrderPengisianKaset");

            migrationBuilder.DropIndex(
                name: "IX_MUTASIVAULT_HISTORY",
                table: "MutasiVault");

            migrationBuilder.DropIndex(
                name: "IX_MUTASIVAULT_REF_TIPE",
                table: "MutasiVault");

            migrationBuilder.DropIndex(
                name: "UX_MUTASIVAULT_SALDOAWAL",
                table: "MutasiVault");

            migrationBuilder.DropColumn(
                name: "KdBank",
                table: "StokVaultCabang");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "OrderPengisianKaset");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "OrderPengisianKaset");

            migrationBuilder.DropColumn(
                name: "KdBank",
                table: "MutasiVault");

            migrationBuilder.CreateIndex(
                name: "UX_STOKVAULT_CABANG_NOMINAL",
                table: "StokVaultCabang",
                columns: new[] { "KdCabang", "Nominal" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderPengisianKasetDetail_OrderId",
                table: "OrderPengisianKasetDetail",
                column: "OrderId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_OrderPengisianKasetDetail_KasetStock_KodeKaset",
                table: "OrderPengisianKasetDetail",
                column: "KodeKaset",
                principalTable: "KasetStock",
                principalColumn: "KdKaset",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
