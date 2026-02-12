namespace cpcApi.Model.DTO.Cpc
{
    public class KasetMovementDto
    {
        public long IdMovement { get; set; }
        public string Action { get; set; }
        public string? FromLocation { get; set; }
        public string? ToLocation { get; set; }
        public string NoWO { get; set; }
        public string? Wsid { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
