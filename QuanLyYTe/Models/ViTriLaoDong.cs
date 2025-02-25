using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyYTe.Models
{
    public class ViTriLaoDong
    {
        [Key]
        public int ID_ViTriLaoDong { get; set; }
        public string? TenViTriLaoDong { get; set; }
        [NotMapped]
        public int ChiTieuNoiDung { get; set; }
        public int ID_PhongBan { get; set; }
        [NotMapped]
        public string TenPhongBan { get; set; }

    }
}
