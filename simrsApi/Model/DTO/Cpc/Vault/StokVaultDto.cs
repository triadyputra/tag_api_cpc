namespace cpcApi.Model.DTO.Cpc.Vault
{
    public class StokVaultDto
    {
        public string KdCabang { get; set; } = default!;
        public string KdBank { get; set; } = default!;

        public int Nominal { get; set; }
        public long Saldo { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
