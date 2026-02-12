using cpcApi.Data;
using cpcApi.Model.Cpc;
using cpcApi.Model;
using cpcApi.Services.Cpc.Vault;
using Microsoft.EntityFrameworkCore;

public class VaultService : IVaultService
{
    private readonly ApplicationDbContext _context;

    public VaultService(ApplicationDbContext context)
    {
        _context = context;
    }

    // =============================================
    // LOCK ROW WITH UPDLOCK + HOLDLOCK
    // =============================================
    private async Task<StokVaultCabang?> GetLockedStokAsync(
    string kdCabang,
    string kdBank,
    int nominal)
    {
        return await _context.StokVaultCabang
            .FromSqlInterpolated($@"
            SELECT * FROM StokVaultCabang WITH (UPDLOCK, HOLDLOCK)
            WHERE KdCabang = {kdCabang}
              AND KdBank = {kdBank}
              AND Nominal = {nominal}")
            .AsTracking()
            .SingleOrDefaultAsync();
    }

    // =============================================
    // MUTASI
    // =============================================
    public async Task<ServiceResult> MutasiAsync(
        string kdCabang,
        string kdBank,
        int nominal,
        long qtyLembar,
        string tipeMutasi,
        string? referenceNo = null)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        ServiceResult result = ServiceResult.Ok();

        await strategy.ExecuteAsync(async () =>
        {
            await using var trx = await _context.Database.BeginTransactionAsync();

            try
            {
                var now = DateTime.UtcNow;
                tipeMutasi = tipeMutasi.ToUpper();

                var stok = await GetLockedStokAsync(kdCabang, kdBank, nominal);

                // ===== VALIDASI =====
                if (tipeMutasi != "SALDO_AWAL" && stok == null)
                {
                    result = ServiceResult.Fail("Stok belum diinisialisasi.");
                    await trx.RollbackAsync();
                    return;
                }

                // ===== CREATE STOK FIRST TIME =====
                if (stok == null)
                {
                    stok = new StokVaultCabang
                    {
                        KdCabang = kdCabang,
                        KdBank = kdBank,
                        Nominal = nominal,
                        SaldoLembar = 0,
                        UpdatedAt = now
                    };

                    _context.StokVaultCabang.Add(stok);
                    await _context.SaveChangesAsync();
                }

                long saldoBaru = tipeMutasi == "SALDO_AWAL"
                    ? qtyLembar
                    : stok.SaldoLembar + qtyLembar;

                if (saldoBaru < 0)
                {
                    result = ServiceResult.Fail("Saldo tidak mencukupi.");
                    await trx.RollbackAsync();
                    return;
                }

                stok.SaldoLembar = saldoBaru;
                stok.UpdatedAt = now;

                _context.MutasiVault.Add(new MutasiVault
                {
                    KdCabang = kdCabang,
                    KdBank = kdBank,
                    Nominal = nominal,
                    QtyLembar = qtyLembar,
                    TipeMutasi = tipeMutasi,
                    ReferenceNo = referenceNo,
                    SaldoSetelah = saldoBaru,
                    CreatedAt = now
                });

                await _context.SaveChangesAsync();
                await trx.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                await trx.RollbackAsync();
                result = ServiceResult.Fail("Terjadi konflik transaksi. Silakan ulangi.");
            }
            catch (DbUpdateException)
            {
                await trx.RollbackAsync();
                result = ServiceResult.Fail("Saldo awal sudah pernah dibuat.");
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                result = ServiceResult.Fail(ex.Message);
            }
        });

        return result;
    }

    // =============================================
    // TRANSFER (ANTI DEADLOCK)
    // =============================================
    public async Task<ServiceResult> TransferAsync(
        string cabangAsal,
        string cabangTujuan,
        string kdBank,
        int nominal,
        long qty,
        string? referenceNo)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        ServiceResult result = ServiceResult.Ok();

        await strategy.ExecuteAsync(async () =>
        {
            await using var trx = await _context.Database.BeginTransactionAsync();

            try
            {
                var now = DateTime.UtcNow;

                // Lock order fix untuk anti deadlock
                var first = string.CompareOrdinal(cabangAsal, cabangTujuan) <= 0
                    ? cabangAsal
                    : cabangTujuan;

                var second = first == cabangAsal
                    ? cabangTujuan
                    : cabangAsal;

                var stokFirst = await GetLockedStokAsync(first, kdBank, nominal);
                var stokSecond = await GetLockedStokAsync(second, kdBank, nominal);

                var stokAsal = first == cabangAsal ? stokFirst : stokSecond;
                var stokTujuan = first == cabangTujuan ? stokFirst : stokSecond;

                if (stokAsal == null || stokTujuan == null)
                {
                    result = ServiceResult.Fail("Stok belum diinisialisasi.");
                    await trx.RollbackAsync();
                    return;
                }

                if (stokAsal.SaldoLembar < qty)
                {
                    result = ServiceResult.Fail("Saldo cabang asal tidak mencukupi.");
                    await trx.RollbackAsync();
                    return;
                }

                stokAsal.SaldoLembar -= qty;
                stokTujuan.SaldoLembar += qty;

                stokAsal.UpdatedAt = now;
                stokTujuan.UpdatedAt = now;

                _context.MutasiVault.AddRange(
                    new MutasiVault
                    {
                        KdCabang = cabangAsal,
                        KdBank = kdBank,
                        Nominal = nominal,
                        QtyLembar = -qty,
                        TipeMutasi = "TRANSFER_OUT",
                        ReferenceNo = referenceNo,
                        SaldoSetelah = stokAsal.SaldoLembar,
                        CreatedAt = now
                    },
                    new MutasiVault
                    {
                        KdCabang = cabangTujuan,
                        KdBank = kdBank,
                        Nominal = nominal,
                        QtyLembar = qty,
                        TipeMutasi = "TRANSFER_IN",
                        ReferenceNo = referenceNo,
                        SaldoSetelah = stokTujuan.SaldoLembar,
                        CreatedAt = now
                    }
                );

                await _context.SaveChangesAsync();
                await trx.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                await trx.RollbackAsync();
                result = ServiceResult.Fail("Terjadi konflik transaksi. Silakan ulangi.");
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                result = ServiceResult.Fail(ex.Message);
            }
        });

        return result;
    }

    // =============================================
    // OPNAME
    // =============================================
    public async Task<ServiceResult> OpnameAsync(
        string kdCabang,
        string kdBank,
        int nominal,
        long saldoFisik)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        ServiceResult result = ServiceResult.Ok();

        await strategy.ExecuteAsync(async () =>
        {
            await using var trx = await _context.Database.BeginTransactionAsync();

            try
            {
                var now = DateTime.UtcNow;

                var stok = await GetLockedStokAsync(kdCabang, kdBank, nominal);

                if (stok == null)
                {
                    result = ServiceResult.Fail("Stok belum diinisialisasi.");
                    await trx.RollbackAsync();
                    return;
                }

                var selisih = saldoFisik - stok.SaldoLembar;

                if (selisih == 0)
                {
                    await trx.CommitAsync();
                    result = ServiceResult.Ok();
                    return;
                }

                stok.SaldoLembar = saldoFisik;
                stok.UpdatedAt = now;

                _context.MutasiVault.Add(new MutasiVault
                {
                    KdCabang = kdCabang,
                    KdBank = kdBank,
                    Nominal = nominal,
                    QtyLembar = selisih,
                    TipeMutasi = "OPNAME_ADJUSTMENT",
                    ReferenceNo = "OPNAME",
                    SaldoSetelah = saldoFisik,
                    CreatedAt = now
                });

                await _context.SaveChangesAsync();
                await trx.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                await trx.RollbackAsync();
                result = ServiceResult.Fail("Terjadi konflik transaksi. Silakan ulangi.");
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                result = ServiceResult.Fail(ex.Message);
            }
        });

        return result;
    }

}
