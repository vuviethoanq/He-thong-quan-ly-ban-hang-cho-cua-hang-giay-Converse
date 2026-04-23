namespace Converse_NMCNPM.Models
{
    public class SanPhamCardViewModel
    {
        public string MaSP { get; set; } = "";
        public string TenSP { get; set; } = "";
        public string TenDM { get; set; } = "";
        public decimal GiaGoc { get; set; }
        public decimal GiaHienThi { get; set; }
        public decimal DiscountLonNhat { get; set; }
        public string? HinhAnh { get; set; }
    }
}