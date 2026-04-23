using System.ComponentModel.DataAnnotations;

namespace Converse_NMCNPM.Models
{
    public class XacNhanNhapHangViewModel
    {
        public string? MaSP { get; set; }

        public string? GhiChuChung { get; set; }

        [Required]
        public List<NhapHangItemViewModel> Items { get; set; } = new();
    }
}