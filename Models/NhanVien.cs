using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Converse_NMCNPM.Models
{
    [Table("NhanVien")]
    public class NhanVien
    {
        [Key]
        public string MaNV { get; set; } = "";

        public string Username { get; set; } = "";

        public string Password { get; set; } = "";

        public string Role { get; set; } = "";
        public bool DangLamViec { get; set; }

        public DateTime? GioBatDauLam { get; set; }

        public decimal TongGioLam { get; set; }
    }
}
