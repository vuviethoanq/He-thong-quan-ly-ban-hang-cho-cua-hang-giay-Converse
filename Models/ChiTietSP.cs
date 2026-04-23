using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Converse_NMCNPM.Models
{
    [Table("ChiTietSP")]
    public class ChiTietSP
    {
        [Key]
        public string MaCTSP { get; set; } = "";

        public string MaSP { get; set; } = "";

        public int SizeSP { get; set; }

        public string MauSP { get; set; } = "";

        public int SoLuongTon { get; set; }

        public decimal Discount { get; set; }

        public string? HinhAnh { get; set; }

        [ForeignKey("MaSP")]
        public SanPham? SanPham { get; set; }
    }
}