using System.ComponentModel.DataAnnotations;

namespace QuanLyYTe.Models
{
    public class LoaiThuoc
    {
        [Key]
        public int ID_LoaiThuoc { get; set; }
        public string TenThuoc { get; set; }
    }
}
