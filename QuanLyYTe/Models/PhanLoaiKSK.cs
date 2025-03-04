using System.ComponentModel.DataAnnotations;

namespace QuanLyYTe.Models
{
    public class PhanLoaiKSK
    {
        [Key]
        public int ID_PhanLoaiKSK { get; set; }
        public string? TenLoaiKSK { get; set; }
    }
}
