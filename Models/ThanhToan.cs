using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Converse_NMCNPM.Models
{
    [Table("ThanhToan")]
    public class ThanhToan
    {
        [Key]
        public string MaTT { get; set; } = "";

        public string MaDH { get; set; } = "";

        public string PhuongThuc { get; set; } = "";

        public DateTime ThoiGian { get; set; }

        public string TrangThai { get; set; } = "";

        [ForeignKey("MaDH")]
        public DonHang? DonHang { get; set; }
    }
}
