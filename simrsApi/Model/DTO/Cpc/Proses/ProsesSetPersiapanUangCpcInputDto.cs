namespace cpcApi.Model.DTO.Cpc.Proses
{
    public class ProsesSetPersiapanUangCpcInputDto
    {
        public int SetKe { get; set; }
        public List<ProsesKotakUangCpcInputDto> DaftarKotakUang { get; set; } = [];
    }
}
