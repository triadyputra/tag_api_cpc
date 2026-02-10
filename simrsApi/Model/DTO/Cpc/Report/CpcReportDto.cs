namespace cpcApi.Model.DTO.Cpc.Report
{
    public class CpcReportHeaderDto
    {
        public string KodeBank { get; set; }
        public DateOnly TanggalProses { get; set; }
        public string NomorDvr { get; set; }
        public string Meja { get; set; }
        public TimeOnly JamMulai { get; set; }
        public TimeOnly JamSelesai { get; set; }
        public string NamaPetugas { get; set; }
        public string JabatanPetugas { get; set; }
        public string PathTtdPetugas { get; set; }

        public List<CpcReportSetDto> Sets { get; set; } = new();
    }

    public class CpcReportSetDto
    {
        public int SetKe { get; set; }
        public List<CpcReportKotakDto> Kotak { get; set; } = new();
    }

    public class CpcReportKotakDto
    {
        public int UrutanKolom { get; set; }   // 1 - 12
        public string NomorKotakUang { get; set; }
        public string NomorSeal { get; set; }
        public int? JumlahLembar { get; set; }
        public int? JenisUang { get; set; }
    }

    public class CpcReportRowDto
    {
        // ===== HEADER =====
        public string KodeBank { get; set; }
        public DateOnly TanggalProses { get; set; }
        public string NomorDvr { get; set; }
        public string Meja { get; set; }
        public TimeOnly JamMulai { get; set; }
        public TimeOnly JamSelesai { get; set; }
        public string NamaPetugas { get; set; }
        public string JabatanPetugas { get; set; }
        
        public string JenisProses { get; set; }

        // ===== SET =====
        public int SetKe { get; set; }

        // ===== KOTAK 1–10 =====
        public string Kotak1 { get; set; }
        public string Kotak2 { get; set; }
        public string Kotak3 { get; set; }
        public string Kotak4 { get; set; }
        public string Kotak5 { get; set; }
        public string Kotak6 { get; set; }
        public string Kotak7 { get; set; }
        public string Kotak8 { get; set; }
        public string Kotak9 { get; set; }
        public string Kotak10 { get; set; }

        public string Seal1 { get; set; }
        public string Seal2 { get; set; }
        public string Seal3 { get; set; }
        public string Seal4 { get; set; }
        public string Seal5 { get; set; }
        public string Seal6 { get; set; }
        public string Seal7 { get; set; }
        public string Seal8 { get; set; }
        public string Seal9 { get; set; }
        public string Seal10 { get; set; }

        public int? Lembar1 { get; set; }
        public int? Lembar2 { get; set; }
        public int? Lembar3 { get; set; }
        public int? Lembar4 { get; set; }
        public int? Lembar5 { get; set; }
        public int? Lembar6 { get; set; }
        public int? Lembar7 { get; set; }
        public int? Lembar8 { get; set; }
        public int? Lembar9 { get; set; }
        public int? Lembar10 { get; set; }

        public int? Jenis1 { get; set; }
        public int? Jenis2 { get; set; }
        public int? Jenis3 { get; set; }
        public int? Jenis4 { get; set; }
        public int? Jenis5 { get; set; }
        public int? Jenis6 { get; set; }
        public int? Jenis7 { get; set; }
        public int? Jenis8 { get; set; }
        public int? Jenis9 { get; set; }
        public int? Jenis10 { get; set; }
    }
}
