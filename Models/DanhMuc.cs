using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Converse_NMCNPM.Models
{

    [Table("DanhMuc")]
    public class DanhMuc
    {
        [Key]
        public string MaDM { get; set; } = "";

        public string TenDM { get; set; } = "";
    }

}