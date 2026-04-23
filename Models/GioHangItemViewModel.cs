namespace Converse_NMCNPM.Models
{
    public class GioHangItemViewModel
    {
        public string MaCTSP { get; set; } = "";
        public string TenSP { get; set; } = "";
        public string MauSP { get; set; } = "";
        public int SizeSP { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public string? HinhAnh { get; set; }

        public decimal ThanhTien => SoLuong * DonGia;
    }
}