namespace cpcApi.Model.DTO.Cpc.orderkaset
{
    public class OrderPengisianKasetLookupDto
    {
        public string Id { get; set; } = default!;
        public string NomorMesin { get; set; } = default!;
        public string Lokasi { get; set; } = default!;
        public string MerekMesin { get; set; } = default!;
        public string KDBANK { get; set; } = default!;
        public string KDCABANG { get; set; } = default!;
        public decimal Jumlah { get; set; }

        public List<OrderPengisianKasetDetailLookupDto> Details { get; set; }
            = new();
    }
}
