using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyYTe.Models
{
    public class TaiKhoan
    {
        [Key]
        public int? ID_TK { get; set; }
        public int? ID_NV { get; set; }
        public int? ID_PhongBan { get; set; }

        [NotMapped]
        public string? MaNV { get; set; }
        [NotMapped]
        public string? HoTen { get; set; }
        [NotMapped]
        public string? TenPhongBan { get; set; }
        [NotMapped]
        public string? TenViTri { get; set; }
        public string? TenDangNhap { get; set; }
        public string? MatKhau { get; set; }
        [NotMapped]
        public string? MatKhauCu { get; set; }
        [NotMapped]
        public string? NhapLaiMatKhau { get; set; }
        public int? ID_Quyen { get; set; }
        [NotMapped]
        public string? TenQuyen { get; set; }
        public int? IsLock { get; set; }
        public Nullable<int> BDA_ID { get; set; }
        public string? ChuKy { get; set; }
    }
}
