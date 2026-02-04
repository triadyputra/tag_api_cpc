namespace cpcApi.Model.DTO.Cpc.Report
{
    public class OrderPengisianAtmDto
    {
        public DateTime Tanggal { get; set; }
        public string NmBank { get; set; }
        public string Custody1 { get; set; }
        public string Custody2 { get; set; }
        public string Polisi { get; set; }
        public string Sector { get; set; }  
        public string Shift {  get; set; }
        public string Nopol {  get; set; }
        public string Wsid { get; set; }    
        public string Lokasi { get; set; }
        public string Mesin { get; set; }
        public string IsiDenom { get; set; }
        public string IsiDenom2 { get; set; }
        public decimal Total { get; set; }

        /// Berangkat
        public string Kaset1 { get; set; }
        public string Seal1 { get; set; }

        public string Kaset2 { get; set; }
        public string Seal2 { get; set; }

        public string Kaset3 { get; set; }
        public string Seal3 { get; set; }

        public string Kaset4 { get; set; }
        public string Seal4 { get; set; }

        public string Kaset5 { get; set; }
        public string Seal5 { get; set; }

        public string Bag { get; set; }

        /// Pulang
        public string PKaset1 { get; set; }
        public string PSeal1 { get; set; }

        public string PKaset2 { get; set; }
        public string PSeal2 { get; set; }

        public string PKaset3 { get; set; }
        public string PSeal3 { get; set; }

        public string PKaset4 { get; set; }
        public string PSeal4 { get; set; }

        public string PKaset5 { get; set; }
        public string PSeal5 { get; set; }

        public string PBag { get; set; }
        
        public string Keterangan { get; set; }
    }
}
