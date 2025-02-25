using System.ComponentModel.DataAnnotations;

namespace QuanLyYTe.Models
{
    public class CT_KSK_BenhNgheNghiep
    {
        [Key]
        public int ID_CT_KSKBNN { get; set; }
        public int ID_KSK_BNN { get; set; }
        public string? TenChiTieu { get; set; }
        public string? TenNoiDung { get; set; }
        public string? KetQua { get; set; }
    }
}
