using System.ComponentModel.DataAnnotations;

namespace QuanLyYTe.Models
{
    public class KetQuaDauVao
    {
        [Key]
        public int ID_KetQuaDV { get; set; }
        public string? TenKetQua { get; set; }
    }
}
