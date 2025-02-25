using System.ComponentModel.DataAnnotations;

namespace QuanLyYTe.Models
{
    public class KipLamViec
    {
        [Key]
        public int ID_Kip { get; set; }
        public string? TenKip { get; set; }
    }
}
