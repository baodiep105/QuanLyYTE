using System.ComponentModel.DataAnnotations;

namespace QuanLyYTe.Models
{
    public class DanhSachDocHai
    {
        [Key]
        public int ID_DocHai { get; set; }
        public string? TenDocHai { get; set; }
    }
}
