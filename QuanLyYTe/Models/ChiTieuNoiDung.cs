using System.ComponentModel.DataAnnotations;

namespace QuanLyYTe.Models
{
    public class ChiTieuNoiDung
    {
        [Key]
        public int ID_CTND { get; set; }
        public int ID_DocHai { get; set; }
        public string? TenChiTieu { get; set; }
        public string? TenNoiDung { get; set; }
      
    }
}
