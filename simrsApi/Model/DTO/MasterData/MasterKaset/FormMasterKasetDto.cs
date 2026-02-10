namespace cpcApi.Model.DTO.MasterData.MasterKaset
{
    public class FormMasterKasetDto
    {
        //public string? IdKaset { get; set; }

        public string? KdKaset { get; set; }

        public string KdBank { get; set; } = default!;
        public string? NmBank { get; set; } = default!;

        public string KdMerek { get; set; } = default!;
        public string Tipe { get; set; } = default!;
        public string Jenis { get; set; } = default!;
        public string NoSerial { get; set; } = default!;
        public string StatusFisik { get; set; } = default!;
        public string KdCabang { get; set; } = default!;
    }
}
