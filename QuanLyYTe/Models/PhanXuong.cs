using System.ComponentModel.DataAnnotations;

namespace QuanLyYTe.Models
{
    public class PhanXuong
    {
        [Key]
        public int ID_PhanXuong { get; set; }
        public string? TenPhanXuong { get; set; }
    }
}
