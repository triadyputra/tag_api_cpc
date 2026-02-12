namespace cpcApi.Model.DTO.Cpc
{
    public class TrackingKasetDto
    {
        public string KdKaset { get; set; }
        public string NmBank { get; set; }
        public string KdCabang { get; set; }
        public string Status { get; set; }
        public string LocationType { get; set; }
        public string? LocationId { get; set; }
        public string StatusFisik { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
