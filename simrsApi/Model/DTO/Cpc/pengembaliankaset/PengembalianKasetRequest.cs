namespace cpcApi.Model.DTO.Cpc.pengembaliankaset
{
    public class PengembalianKasetRequest
    {
        public string? Id { get; set; } // null = create
        public string OrderPengisianId { get; set; } = default!;
        public string NomorMesin { get; set; } = default!;
        public string Lokasi { get; set; } = default!;
        public string MerekMesin { get; set; } = default!;
        public string KDBANK { get; set; } = default!;
        public string KDCABANG { get; set; } = default!;
        public DateTime TanggalTerima { get; set; }
        public string? DiterimaOleh { get; set; }
        public string? Catatan { get; set; }

        public List<PengembalianKasetDetailRequest> Details { get; set; }
            = new();
    }
}
