namespace cpcApi.Model.DTO.Cpc.pengembaliankaset
{
    public class PengembalianKasetListDto
    {
        public string Id { get; set; } = default!;
        public string NomorMesin { get; set; } = default!;
        public string Lokasi { get; set; } = default!;
        public string KDBANK { get; set; } = default!;
        public string KDCABANG { get; set; } = default!;
        public decimal Jumlah { get; set; }
        public DateTime TanggalTerima { get; set; }
    }
}
