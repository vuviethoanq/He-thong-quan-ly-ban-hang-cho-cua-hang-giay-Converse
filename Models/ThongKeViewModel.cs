namespace Converse_NMCNPM.Models
{
    public class ThongKeViewModel
    {
        public List<string> NhanVienLabels { get; set; } = new();
        public List<decimal> NhanVienHours { get; set; } = new();

        public List<string> DoanhThuLabels { get; set; } = new();
        public List<decimal> DoanhThuValues { get; set; } = new();

        public List<string> SanPhamLabels { get; set; } = new();
        public List<int> SanPhamSoLuong { get; set; } = new();
    }
}