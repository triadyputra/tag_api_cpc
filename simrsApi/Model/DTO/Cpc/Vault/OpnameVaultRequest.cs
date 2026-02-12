namespace cpcApi.Model.DTO.Cpc.Vault
{
    public class OpnameVaultRequest
    {
        public string KdCabang { get; set; } = default!;
        public int Nominal { get; set; }
        public long SaldoFisik { get; set; }
    }
}
