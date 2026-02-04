using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cpcApi.Migrations
{
    /// <inheritdoc />
    public partial class create_table_kaset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MasterMerekKaset",
                columns: table => new
                {
                    KdMerek = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NmMerek = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Keterangan = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    created = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updatedat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterMerekKaset", x => x.KdMerek);
                });

            migrationBuilder.CreateTable(
                name: "MasterKaset",
                columns: table => new
                {
                    IdKaset = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    KdKasetBank = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    KdBank = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NmBank = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    KdMerek = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Tipe = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Jenis = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NoSerial = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StatusFisik = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    KdCabang = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    created = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updatedat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterKaset", x => x.IdKaset);
                    table.ForeignKey(
                        name: "FK_MasterKaset_MasterMerekKaset_KdMerek",
                        column: x => x.KdMerek,
                        principalTable: "MasterMerekKaset",
                        principalColumn: "KdMerek",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KasetMovement",
                columns: table => new
                {
                    IdMovement = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdKaset = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FromLocation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ToLocation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NoWO = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Wsid = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    created = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    createdat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updatedat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KasetMovement", x => x.IdMovement);
                    table.ForeignKey(
                        name: "FK_KasetMovement_MasterKaset_IdKaset",
                        column: x => x.IdKaset,
                        principalTable: "MasterKaset",
                        principalColumn: "IdKaset",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KasetStock",
                columns: table => new
                {
                    IdKaset = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LocationType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LocationId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KasetStock", x => x.IdKaset);
                    table.ForeignKey(
                        name: "FK_KasetStock_MasterKaset_IdKaset",
                        column: x => x.IdKaset,
                        principalTable: "MasterKaset",
                        principalColumn: "IdKaset",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KASET_MOVEMENT_KASET_DATE",
                table: "KasetMovement",
                columns: new[] { "IdKaset", "created" });

            migrationBuilder.CreateIndex(
                name: "IX_KASET_MOVEMENT_NOWO",
                table: "KasetMovement",
                column: "NoWO");

            migrationBuilder.CreateIndex(
                name: "IX_KASET_MOVEMENT_WSID",
                table: "KasetMovement",
                column: "Wsid");

            migrationBuilder.CreateIndex(
                name: "IX_KASET_STOCK_LOCATION",
                table: "KasetStock",
                columns: new[] { "LocationType", "LocationId" });

            migrationBuilder.CreateIndex(
                name: "IX_KASET_STOCK_READY",
                table: "KasetStock",
                columns: new[] { "Status", "LocationType" });

            migrationBuilder.CreateIndex(
                name: "IX_KASET_STOCK_STATUS",
                table: "KasetStock",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MST_KASET_CABANG_STATUS",
                table: "MasterKaset",
                columns: new[] { "KdCabang", "StatusFisik" });

            migrationBuilder.CreateIndex(
                name: "IX_MST_KASET_KDBANK",
                table: "MasterKaset",
                column: "KdBank");

            migrationBuilder.CreateIndex(
                name: "IX_MST_KASET_KDMEREK",
                table: "MasterKaset",
                column: "KdMerek");

            migrationBuilder.CreateIndex(
                name: "IX_MST_KASET_STATUSFISIK",
                table: "MasterKaset",
                column: "StatusFisik");

            migrationBuilder.CreateIndex(
                name: "UQ_MST_KASET_NOSERIAL",
                table: "MasterKaset",
                column: "NoSerial",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_MST_MEREK_KASET_NAMA",
                table: "MasterMerekKaset",
                column: "NmMerek",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KasetMovement");

            migrationBuilder.DropTable(
                name: "KasetStock");

            migrationBuilder.DropTable(
                name: "MasterKaset");

            migrationBuilder.DropTable(
                name: "MasterMerekKaset");
        }
    }
}
