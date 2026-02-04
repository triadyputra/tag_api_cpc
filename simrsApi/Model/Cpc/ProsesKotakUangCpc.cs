namespace cpcApi.Model.Cpc
{
    public class ProsesKotakUangCpc
    {
        public Guid Id { get; set; }

        public Guid ProsesSetPersiapanUangCpcId { get; set; }
        public ProsesSetPersiapanUangCpc Set { get; set; } = default!;

        public int UrutanKolom { get; set; } // 1 s/d 10

        public string? NomorKotakUang { get; set; }
        public string? NomorSeal { get; set; }
        public int? JumlahLembar { get; set; }
        public string? JenisUang { get; set; } // contoh: 100000
    }
}
