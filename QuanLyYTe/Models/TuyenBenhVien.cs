using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyYTe.Models
{
    public class TuyenBenhVien
    {
        [Key]
        public int ID_TuyenBenhVien { get; set; }
        public int ID_SCC { get; set; }
        public string? TenBenhVien { get; set; }
        public int ThuTu { get; set; }
        public string? Ytephutrach { get; set; }
        
        public DateTime? ThoiGianChuyenVien { get; set; }
        public Nullable<decimal> TamUng { get; set; }
        public Nullable<decimal> ThanhToan { get; set; }
        public string? ChungTu { get; set; }
        public string? ThoiGianDieuTri { get; set; }
    }
}
