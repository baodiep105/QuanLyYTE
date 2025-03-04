using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyYTe.Models
{
    public class ChiTiet_ChiTieuNoiDung_ViTri
    {
        [Key]
        public int ID_CT_ViTriLaoDong { get; set; }
        public int ID_ViTriLaoDong { get; set; }
        public int ID_DocHai { get; set; }
        [NotMapped]
        public string TenDocHai { get; set; }
    }
}
