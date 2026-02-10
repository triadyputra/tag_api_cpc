namespace cpcApi.Model.DTO.Cpc.logistik
{
    public class RegisterSealRangeDto
    {
        public int NomorAwal { get; set; }   // contoh: 1
        public int NomorAkhir { get; set; }  // contoh: 1000
        public bool Active { get; set; } = true;
    }

    public class RegisterSealUpdateDto
    {
        public bool Active { get; set; }
    }

    public class ViewRegisterSealDto
    {
        public string NomorSeal { get; set; } = default!;
        public string Status { get; set; } = default!;
        public bool Active { get; set; }
    }
}
