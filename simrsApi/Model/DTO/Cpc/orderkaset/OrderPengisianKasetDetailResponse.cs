namespace cpcApi.Model.DTO.Cpc.orderkaset
{
    public class OrderPengisianKasetDetailResponse
    {
        public string Id { get; set; } = default!;
        public string NomorMesin { get; set; } = default!;
        public DateTime TanggalOrder { get; set; }
        public string Lokasi { get; set; } = default!;
        public string MerekMesin { get; set; } = default!;
        public string KDBANK { get; set; } = default!;
        public string KDCABANG { get; set; } = default!;
        public decimal Jumlah { get; set; }

        public List<OrderPengisianKasetDetailDto> Details { get; set; }
            = new();
    }
}
