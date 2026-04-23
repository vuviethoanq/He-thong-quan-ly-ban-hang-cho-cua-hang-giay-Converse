    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace Converse_NMCNPM.Models
    {
    [Table("SanPham")]
    public class SanPham
    {
        [Key]
        public string MaSP { get; set; } = "";

        public string TenSP { get; set; } = "";

        public decimal GiaSP { get; set; }

        public string MaDM { get; set; } = "";

        [ForeignKey("MaDM")]
        public DanhMuc? DanhMuc { get; set; }
    }
}
