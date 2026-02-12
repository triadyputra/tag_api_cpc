namespace cpcApi.Model.DTO.Cpc.Vault
{
    public class TransferVaultRequest
    {
        public string KdCabangAsal { get; set; } = default!;
        public string KdCabangTujuan { get; set; } = default!;
        public string KdBank { get; set; } = default!;
        public int Nominal { get; set; }
        public long QtyLembar { get; set; }
        public string? ReferenceNo { get; set; }
    }
}
