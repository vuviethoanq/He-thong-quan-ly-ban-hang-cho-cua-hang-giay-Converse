using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Converse_NMCNPM.Models
{
    [Table("KhachHang")]
    public class KhachHang
    {
        [Key]
        public string MaKH { get; set; } = "";

        public string TenKH { get; set; } = "";

        public string SdtKH { get; set; } = "";

        public string DiaChiKH { get; set; } = "";
    }
}