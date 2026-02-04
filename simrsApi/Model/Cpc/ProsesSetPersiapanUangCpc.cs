namespace cpcApi.Model.Cpc
{
    public class ProsesSetPersiapanUangCpc
    {
        public Guid Id { get; set; }

        public Guid ProsesPersiapanUangCpcId { get; set; }
        public ProsesPersiapanUangCpc Proses { get; set; } = default!;

        public int SetKe { get; set; } // Set ke-1, 2, 3

        public ICollection<ProsesKotakUangCpc> DaftarKotakUang { get; set; }
            = new List<ProsesKotakUangCpc>();
    }
}
