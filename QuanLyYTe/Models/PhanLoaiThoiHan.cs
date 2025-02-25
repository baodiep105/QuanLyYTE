using System.ComponentModel.DataAnnotations;

namespace QuanLyYTe.Models
{
    public class PhanLoaiThoiHan
    {
        [Key]
        public int ID_PhanLoai { get; set; }
        public string? TenLoai { get; set; }
        public int ThoiHan { get; set; }
    }
}
