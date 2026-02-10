using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cpcApi.Migrations
{
    /// <inheritdoc />
    public partial class initialdatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Access = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Keterangan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Photo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Photo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Cabang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

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
                name: "OrderPengisianKaset",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NomorMesin = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TanggalOrder = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Lokasi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MerekMesin = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    KDBANK = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    KDCABANG = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Jumlah = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    created = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updatedat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderPengisianKaset", x => x.Id);
                });

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
                    TanggalFinal = table.Column<DateTime>(type: "datetime2", nullable: true),
                    KdCabang = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProsesPersiapanUangCpc", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefreshToken",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshToken", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegisterSeal",
                columns: table => new
                {
                    NomorSeal = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReservedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegisterSeal", x => x.NomorSeal);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MasterKaset",
                columns: table => new
                {
                    KdKaset = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("PK_MasterKaset", x => x.KdKaset);
                    table.ForeignKey(
                        name: "FK_MasterKaset_MasterMerekKaset_KdMerek",
                        column: x => x.KdMerek,
                        principalTable: "MasterMerekKaset",
                        principalColumn: "KdMerek",
                        onDelete: ReferentialAction.Restrict);
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
                name: "KasetMovement",
                columns: table => new
                {
                    IdMovement = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KdKaset = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
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
                        name: "FK_KasetMovement_MasterKaset_KdKaset",
                        column: x => x.KdKaset,
                        principalTable: "MasterKaset",
                        principalColumn: "KdKaset",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KasetStock",
                columns: table => new
                {
                    KdKaset = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReservedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LocationType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LocationId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KasetStock", x => x.KdKaset);
                    table.ForeignKey(
                        name: "FK_KasetStock_MasterKaset_KdKaset",
                        column: x => x.KdKaset,
                        principalTable: "MasterKaset",
                        principalColumn: "KdKaset",
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
                    JenisUang = table.Column<int>(type: "int", maxLength: 20, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "OrderPengisianKasetDetail",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Kaset = table.Column<int>(type: "int", nullable: false),
                    KodeKaset = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NoSeal = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Denom = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Lembar = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderPengisianKasetDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderPengisianKasetDetail_KasetStock_KodeKaset",
                        column: x => x.KodeKaset,
                        principalTable: "KasetStock",
                        principalColumn: "KdKaset",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderPengisianKasetDetail_OrderPengisianKaset_OrderId",
                        column: x => x.OrderId,
                        principalTable: "OrderPengisianKaset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    KasetStockKdKaset = table.Column<string>(type: "nvarchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PengembalianKasetDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PengembalianKasetDetail_KasetStock_KasetStockKdKaset",
                        column: x => x.KasetStockKdKaset,
                        principalTable: "KasetStock",
                        principalColumn: "KdKaset");
                    table.ForeignKey(
                        name: "FK_PengembalianKasetDetail_PengembalianKaset_PengembalianId",
                        column: x => x.PengembalianId,
                        principalTable: "PengembalianKaset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_KASET_MOVEMENT_KASET_DATE",
                table: "KasetMovement",
                columns: new[] { "KdKaset", "created" });

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
                name: "UQ_MST_KASET_KDKASET",
                table: "MasterKaset",
                column: "KdKaset",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_OrderPengisianKasetDetail_KodeKaset",
                table: "OrderPengisianKasetDetail",
                column: "KodeKaset");

            migrationBuilder.CreateIndex(
                name: "IX_OrderPengisianKasetDetail_OrderId",
                table: "OrderPengisianKasetDetail",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PengembalianKasetDetail_KasetStockKdKaset",
                table: "PengembalianKasetDetail",
                column: "KasetStockKdKaset");

            migrationBuilder.CreateIndex(
                name: "IX_PengembalianKasetDetail_PengembalianId",
                table: "PengembalianKasetDetail",
                column: "PengembalianId");

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
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "KasetMovement");

            migrationBuilder.DropTable(
                name: "OrderPengisianKasetDetail");

            migrationBuilder.DropTable(
                name: "PengembalianKasetDetail");

            migrationBuilder.DropTable(
                name: "ProsesKotakUangCpc");

            migrationBuilder.DropTable(
                name: "RefreshToken");

            migrationBuilder.DropTable(
                name: "RegisterSeal");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "OrderPengisianKaset");

            migrationBuilder.DropTable(
                name: "KasetStock");

            migrationBuilder.DropTable(
                name: "PengembalianKaset");

            migrationBuilder.DropTable(
                name: "ProsesSetPersiapanUangCpc");

            migrationBuilder.DropTable(
                name: "MasterKaset");

            migrationBuilder.DropTable(
                name: "ProsesPersiapanUangCpc");

            migrationBuilder.DropTable(
                name: "MasterMerekKaset");
        }
    }
}
