using System.ComponentModel.DataAnnotations;

namespace QuanLyYTe.Models
{
    public class GioiTinh
    {
        [Key]
        public int ID_GioiTinh { get; set; }
        public string? TenGioiTinh { get; set; }
    }
}
