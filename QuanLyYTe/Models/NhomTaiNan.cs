using System.ComponentModel.DataAnnotations;

namespace QuanLyYTe.Models
{
    public class NhomTaiNan
    {
        [Key]
        public int ID_NhomTaiNan { get; set; }
        public string? TenNhomTaiNan { get; set; }
    }
}
