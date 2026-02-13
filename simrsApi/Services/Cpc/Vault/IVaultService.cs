using cpcApi.Model;

namespace cpcApi.Services.Cpc.Vault
{
    public interface IVaultService
    {
        Task<ServiceResult> MutasiAsync(
            string kdCabang,
            string KdBank,
            int nominal,
            long qtyLembar,
            string tipeMutasi,
            string? referenceNo = null
        );

        Task<ServiceResult> TransferAsync(
            string cabangAsal,
            string cabangTujuan,
            string KdBank,
            int nominal,
            long qty,
            string? referenceNo = null
        );

        Task<ServiceResult> OpnameAsync(
            string kdCabang,
            string KdBank,
            int nominal,
            long saldoFisik
        );

        Task<ServiceResult> TransaksiAsync(
             string kdCabang,
            string KdBank,
            int nominal,
            long qtyLembar,
            string tipeMutasi,
            string? referenceNo = null
        );
    }

}
