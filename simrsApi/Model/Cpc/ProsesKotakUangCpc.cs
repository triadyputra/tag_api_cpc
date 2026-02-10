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
        public int? JenisUang { get; set; } // contoh: 100000

        // 🔥 STATUS PEMAKAIAN
        public StatusKotakUangCpc Status { get; set; } = StatusKotakUangCpc.Draft;

        // optional (audit)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    public enum StatusKotakUangCpc
    {
        Draft = 1,        // masih draft, belum final
        Ready = 2,     // dipakai proses lain (lock)
        Used = 3,         // sudah dipakai di ATM (FINAL)
        Cancelled = 9
    }
}
