using System.ComponentModel.DataAnnotations;

namespace Converse_NMCNPM.Models
{
    public class TaoDonHangViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên khách hàng")]
        public string TenKH { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string SDT { get; set; } = "";

        public string? DiaChi { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn chi tiết sản phẩm")]
        public string MaCTSP { get; set; } = "";

        [Range(1, 1000, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int SoLuong { get; set; } = 1;
    }
}