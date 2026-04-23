using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Converse_NMCNPM.Models
{
    [Table("DonHang")]
    public class DonHang
    {
        [Key]
        public string MaDH { get; set; } = "";

        public string MaKH { get; set; } = "";

        public string MaNV { get; set; } = "";

        public DateTime NgayTao { get; set; }

        public decimal TongTien { get; set; }

        [ForeignKey("MaKH")]
        public KhachHang? KhachHang { get; set; }

        [ForeignKey("MaNV")]
        public NhanVien? NhanVien { get; set; }
    }
}