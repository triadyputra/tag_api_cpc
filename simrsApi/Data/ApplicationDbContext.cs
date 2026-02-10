using cpcApi.Model;
using cpcApi.Model.Cpc;
using cpcApi.Model.Logistik;
using cpcApi.Model.MasterData;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;




namespace cpcApi.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }



        public DbSet<MasterKaset> MasterKaset { get; set; }
        public DbSet<KasetStock> KasetStock { get; set; }
        public DbSet<KasetMovement> KasetMovement { get; set; }
        
        public DbSet<MasterMerekKaset> MasterMerekKaset { get; set; }
        public DbSet<RefreshToken> RefreshToken { get; set; }
        
        public DbSet<OrderPengisianKaset> OrderPengisianKaset { get; set; }
        public DbSet<OrderPengisianKasetDetail> OrderPengisianKasetDetail { get; set; }
        
        public DbSet<PengembalianKaset> PengembalianKaset { get; set; }
        public DbSet<PengembalianKasetDetail> PengembalianKasetDetail { get; set; }


        /* Proses */
        public DbSet<ProsesPersiapanUangCpc> ProsesPersiapanUangCpc { get; set; }
        public DbSet<ProsesSetPersiapanUangCpc> ProsesSetPersiapanUangCpc { get; set; }
        public DbSet<ProsesKotakUangCpc> ProsesKotakUangCpc { get; set; }

        /* Logistik */
        public DbSet<RegisterSeal> RegisterSeal { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<MasterKaset>(entity =>
            {
                /* ================= INDEX ================= */

                // ===== UNIQUE =====
                entity.HasIndex(e => e.KdKaset)
                    .IsUnique()
                    .HasDatabaseName("UQ_MST_KASET_KDKASET");

                entity.HasIndex(e => e.NoSerial)
                     .IsUnique()
                     .HasDatabaseName("UQ_MST_KASET_NOSERIAL");

                // ===== SEARCH & LIST =====
                entity.HasIndex(e => e.KdBank)
                    .HasDatabaseName("IX_MST_KASET_KDBANK");

                entity.HasIndex(e => e.KdMerek)
                    .HasDatabaseName("IX_MST_KASET_KDMEREK");

                entity.HasIndex(e => e.StatusFisik)
                    .HasDatabaseName("IX_MST_KASET_STATUSFISIK");

                entity.HasIndex(e => new { e.KdCabang, e.StatusFisik })
                    .HasDatabaseName("IX_MST_KASET_CABANG_STATUS");

            });

            builder.Entity<KasetStock>(entity =>
            {
                /* ================= RELATION ================= */

                entity.HasOne(e => e.Kaset)
                    .WithOne(e => e.Stock)
                    .HasForeignKey<KasetStock>(e => e.KdKaset)
                    .OnDelete(DeleteBehavior.Cascade);
                // ✅ BOLEH CASCADE

                /* ================= INDEX ================= */

                // List utama (EMPTY / LOADED / ON_TRIP)
                entity.HasIndex(e => e.Status)
                    .HasDatabaseName("IX_KASET_STOCK_STATUS");

                // Filter lokasi (VAULT / WO / ATM)
                entity.HasIndex(e => new { e.LocationType, e.LocationId })
                    .HasDatabaseName("IX_KASET_STOCK_LOCATION");

                // Kombinasi untuk list cepat
                entity.HasIndex(e => new { e.Status, e.LocationType })
                    .HasDatabaseName("IX_KASET_STOCK_READY");
            });

            builder.Entity<KasetMovement>(entity =>
            {
                /* ================= RELATION ================= */

                entity.HasOne(e => e.Kaset)
                    .WithMany(e => e.Movements)
                    .HasForeignKey(e => e.KdKaset)
                    .OnDelete(DeleteBehavior.Restrict);
                // ❌ TIDAK BOLEH CASCADE (audit wajib aman)

                /* ================= INDEX ================= */

                // Audit histori per kaset
                entity.HasIndex(e => new { e.KdKaset, e.created })
                    .HasDatabaseName("IX_KASET_MOVEMENT_KASET_DATE");

                // Audit per Work Order
                entity.HasIndex(e => e.NoWO)
                    .HasDatabaseName("IX_KASET_MOVEMENT_NOWO");

                // Audit per ATM
                entity.HasIndex(e => e.Wsid)
                    .HasDatabaseName("IX_KASET_MOVEMENT_WSID");
            });

            builder.Entity<MasterMerekKaset>(entity =>
            {
                // Nama merek harus unik
                entity.HasIndex(e => e.NmMerek)
                    .IsUnique()
                    .HasDatabaseName("UQ_MST_MEREK_KASET_NAMA");
            });

            builder.Entity<MasterKaset>()
                .HasOne(k => k.Merek)
                .WithMany(m => m.Kasets)
                .HasForeignKey(k => k.KdMerek)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================
            // OrderPengisianKaset
            // =========================
            builder.Entity<OrderPengisianKaset>(entity =>
            {
                entity.Property(e => e.Jumlah)
                      .HasPrecision(18, 2);
            });

            // =========================
            // OrderPengisianKasetDetail
            // =========================
            builder.Entity<OrderPengisianKasetDetail>(entity =>
            {
                entity.Property(e => e.Denom)
                      .HasPrecision(18, 2);

                entity.HasOne(d => d.Order)
                      .WithMany(h => h.Details)
                      .HasForeignKey(d => d.OrderId);

                entity.HasOne(d => d.KasetStock)
                      .WithMany(k => k.OrderDetails)
                      .HasForeignKey(d => d.KodeKaset)
                      .HasPrincipalKey(k => k.KdKaset);
            });


            // =========================
            // Pengembalian kaset
            // =========================
            builder.Entity<PengembalianKaset>(entity =>
            {
                entity.HasMany(x => x.Details)
                      .WithOne(x => x.Pengembalian)
                      .HasForeignKey(x => x.PengembalianId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.Jumlah)
                      .HasPrecision(18, 2); // 💰 TOTAL UANG
            });

            builder.Entity<PengembalianKasetDetail>(entity =>
            {
                entity.Property(e => e.Denom)
                      .HasPrecision(18, 2); // 💵 DENOM KASET
            });


            // =====================================================
            // PROSES PERSIAPAN UANG CPC
            // =====================================================
            builder.Entity<ProsesPersiapanUangCpc>(entity =>
            {
                entity.ToTable("ProsesPersiapanUangCpc");

                entity.HasKey(e => e.Id);

                /* ================= INDEX ================= */

                // Filter laporan per bank & tanggal
                entity.HasIndex(e => new { e.KodeBank, e.TanggalProses })
                      .HasDatabaseName("IX_PROSES_CPC_BANK_TANGGAL");

                // Filter status (Draft / Final)
                entity.HasIndex(e => e.Status)
                      .HasDatabaseName("IX_PROSES_CPC_STATUS");

                /* ================= FIELD ================= */

                entity.Property(e => e.KodeBank)
                      .HasMaxLength(20)
                      .IsRequired();

                entity.Property(e => e.NomorDvr)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(e => e.Meja)
                      .HasMaxLength(10)
                      .IsRequired();

                entity.Property(e => e.NamaPetugas)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.JabatanPetugas)
                      .HasMaxLength(50)
                      .IsRequired();

                /* ================= RELATION ================= */

                entity.HasMany(e => e.DaftarSet)
                      .WithOne(e => e.Proses)
                      .HasForeignKey(e => e.ProsesPersiapanUangCpcId)
                      .OnDelete(DeleteBehavior.Cascade);
                // ✅ DELETE PROSES → SET & KOTAK IKUT TERHAPUS
            });


            // =====================================================
            // SET PERSIAPAN UANG CPC
            // =====================================================
            builder.Entity<ProsesSetPersiapanUangCpc>(entity =>
            {
                entity.ToTable("ProsesSetPersiapanUangCpc");

                entity.HasKey(e => e.Id);

                /* ================= INDEX ================= */

                // 1 Proses tidak boleh punya SetKe sama
                entity.HasIndex(e => new { e.ProsesPersiapanUangCpcId, e.SetKe })
                      .IsUnique()
                      .HasDatabaseName("UX_SET_CPC_PROSES_SETKE");

                /* ================= FIELD ================= */

                entity.Property(e => e.SetKe)
                      .IsRequired();

                /* ================= RELATION ================= */

                entity.HasMany(e => e.DaftarKotakUang)
                      .WithOne(e => e.Set)
                      .HasForeignKey(e => e.ProsesSetPersiapanUangCpcId)
                      .OnDelete(DeleteBehavior.Cascade);
                // ✅ DELETE SET → KOTAK IKUT TERHAPUS
            });


            // =====================================================
            // KOTAK UANG CPC
            // =====================================================
            builder.Entity<ProsesKotakUangCpc>(entity =>
            {
                entity.ToTable("ProsesKotakUangCpc");

                entity.HasKey(e => e.Id);

                /* ================= INDEX ================= */

                // 1 Set tidak boleh punya UrutanKolom sama
                entity.HasIndex(e => new { e.ProsesSetPersiapanUangCpcId, e.UrutanKolom })
                      .IsUnique()
                      .HasDatabaseName("UX_KOTAK_CPC_SET_URUTAN");

                /* ================= FIELD ================= */

                entity.Property(e => e.UrutanKolom)
                      .IsRequired();

                entity.Property(e => e.NomorKotakUang)
                      .HasMaxLength(50);

                entity.Property(e => e.NomorSeal)
                      .HasMaxLength(50);

                entity.Property(e => e.JenisUang)
                      .HasMaxLength(20);
            });
        }

    }
}
