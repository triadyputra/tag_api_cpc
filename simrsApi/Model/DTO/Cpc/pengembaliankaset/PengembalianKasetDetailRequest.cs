namespace cpcApi.Model.DTO.Cpc.pengembaliankaset
{
    public class PengembalianKasetDetailRequest
    {
        public int Kaset { get; set; }
        public string KodeKaset { get; set; } = default!;
        public string? NoSeal { get; set; }
        public decimal Denom { get; set; }
        public int Lembar { get; set; }
    }
}
