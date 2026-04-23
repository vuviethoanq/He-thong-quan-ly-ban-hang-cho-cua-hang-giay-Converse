using System.ComponentModel.DataAnnotations;

namespace Converse_NMCNPM.Models
{
    public class NhapHangItemViewModel
    {
        [Required]
        public string MaCTSP { get; set; } = "";

        [Range(1, int.MaxValue)]
        public int SoLuongNhap { get; set; }

        [Range(0, double.MaxValue)]
        public decimal GiaNhap { get; set; }

        public string? GhiChu { get; set; }
    }
}