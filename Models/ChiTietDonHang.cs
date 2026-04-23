using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Converse_NMCNPM.Models
{
    [Table("ChiTietDonHang")]
    public class ChiTietDonHang
    {
        [Key]
        public string MaCTDH { get; set; } = "";

        public string MaDH { get; set; } = "";

        public string MaCTSP { get; set; } = "";

        public int SoLuong { get; set; }

        public decimal Gia { get; set; }

        [ForeignKey("MaDH")]
        public DonHang? DonHang { get; set; }

        [ForeignKey("MaCTSP")]
        public ChiTietSP? ChiTietSP { get; set; }
    }
}
