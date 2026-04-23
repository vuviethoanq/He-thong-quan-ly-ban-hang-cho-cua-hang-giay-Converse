using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Converse_NMCNPM.Models
{
    [Table("ChiTietNhapHang")]
    public class ChiTietNhapHang
    {
        [Key]
        public string MaCTNH { get; set; } = "";

        public string MaNH { get; set; } = "";

        public string MaCTSP { get; set; } = "";

        public int SoLuongNhap { get; set; }

        public decimal GiaNhap { get; set; }

        [ForeignKey("MaNH")]
        public NhapHang? NhapHang { get; set; }

        [ForeignKey("MaCTSP")]
        public ChiTietSP? ChiTietSP { get; set; }
    }
}