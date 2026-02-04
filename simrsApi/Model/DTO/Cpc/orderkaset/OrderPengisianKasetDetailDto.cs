namespace cpcApi.Model.DTO.Cpc.orderkaset
{
    public class OrderPengisianKasetDetailDto
    {
        public int Kaset { get; set; }
        public string KodeKaset { get; set; } = default!;
        public string? NoSeal { get; set; }
        public decimal Denom { get; set; }
        public int Lembar { get; set; }
        public decimal Total => Denom * Lembar;
    }
}
