namespace cpcApi.Model.DTO.Cpc.orderkaset
{
    public class OrderPengisianKasetDetailRequest
    {
        public int Kaset { get; set; }
        public string KodeKaset { get; set; } = default!; // IdKaset
        public string? NoSeal { get; set; }
        public decimal Denom { get; set; }
        public int Lembar { get; set; }
    }
}
