namespace cpcApi.Model.DTO.Cpc.Vault
{
    public class MutasiRequest
    {
        public string KdCabang { get; set; } = default!;
        public string KdBank { get; set; } = default!;   // 🔥 TAMBAHAN
        public int Nominal { get; set; }
        public long QtyLembar { get; set; }
        public string TipeMutasi { get; set; } = default!;
        public string? ReferenceNo { get; set; }
    }
}
