using System.ComponentModel.DataAnnotations;

namespace Converse_NMCNPM.Models
{
    public class GioHangViewModel
    {
        public List<GioHangItemViewModel> Items { get; set; } = new();

        [Required(ErrorMessage = "Vui lòng nhập tên khách hàng")]
        public string TenKH { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string SDT { get; set; } = "";

        public string? DiaChi { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        public string PhuongThucThanhToan { get; set; } = "TienMat";

        public decimal TongTien => Items.Sum(x => x.ThanhTien);
    }
}