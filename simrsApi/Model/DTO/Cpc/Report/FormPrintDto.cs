namespace cpcApi.Model.DTO.Cpc.Report
{
    public class FormPrintDto
    {
        public string? nowo { get; set; }
        public string? cabang { get; set; }
        public string? bank { get; set; }

        public string? tanggalawal { get; set; }

        public string? tanggalakhir { get; set; }


        /// <summary>
        /// pdf | excel | xlsx
        /// </summary>
        public string? format { get; set; }
    }
}
