using System.ComponentModel.DataAnnotations;

namespace cpcApi.Model.Cpc
{
    public class OrderPengisianKaset : BaseModel
    {
        [Key]
        [StringLength(100)]
        public string Id { get; set; } = default!;
        // contoh: 01RPL01260155|BCA-341H

        [Required]
        [StringLength(50)]
        public string NomorMesin { get; set; } = default!; // WSID

        public DateTime TanggalOrder { get; set; }

        [Required]
        [StringLength(200)]
        public string Lokasi { get; set; } = default!;

        [Required]
        [StringLength(50)]
        public string MerekMesin { get; set; } = default!;

        [StringLength(10)]
        public string KDBANK { get; set; } = default!;

        [StringLength(10)]
        public string KDCABANG { get; set; } = default!;

        public decimal Jumlah { get; set; }  // total uang


        // 🔥 TAMBAHAN
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "DRAFT";

        // 🔒 Anti race condition
        [Timestamp]
        public byte[] RowVersion { get; set; } = default!;


        // ================= NAVIGATION =================
        public ICollection<OrderPengisianKasetDetail> Details { get; set; }
            = new List<OrderPengisianKasetDetail>();
    }
}
