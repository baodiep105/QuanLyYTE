using System.ComponentModel.DataAnnotations;

namespace QuanLyYTe.Models
{
    public class PhongBan
    {
        [Key]
        public int ID_PhongBan { get; set; }
        public string? TenPhongBan { get; set; }
    }
}
