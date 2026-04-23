using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Converse_NMCNPM.Models
{
    [Table("NhapHang")]
    public class NhapHang
    {
        [Key]
        public string MaNH { get; set; } = "";

        public string MaNV { get; set; } = "";

        public DateTime NgayNhap { get; set; }

        public string? GhiChu { get; set; }

        [ForeignKey("MaNV")]
        public NhanVien? NhanVien { get; set; }
    }
}