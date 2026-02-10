namespace cpcApi.Model.DTO.MasterData.MasterKaset
{
    public class UploadKasetResultDto
    {
        public int Row { get; set; }
        public string NoSerial { get; set; } = "";
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }
}
