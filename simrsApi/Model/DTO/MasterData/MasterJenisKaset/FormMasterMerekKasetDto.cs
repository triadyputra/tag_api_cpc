namespace cpcApi.Model.DTO.MasterData.MasterJenisKaset
{
    public class FormMasterMerekKasetDto
    {
        public string? KdMerek { get; set; }
        public string NmMerek { get; set; } = default!;
        public string? Keterangan { get; set; }
        public bool Aktif { get; set; } = true;
    }
}
