using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyYTe.Models
{
    public class NhanVien
    {
        [Key]
        public int ID_NV { get; set; }
        public string? MaNV { get; set; }
        public string? HoTen { get; set; }
        public string? CMND { get; set; }
        public DateTime NgaySinh { get; set; }
        public string? DiaChi { get; set; }
        public DateTime  NgayVaoLam { get; set; }
        public int? ID_PhongBan { get; set; }
        [NotMapped]
        public string? TenPhongBan { get; set; }
        public int? ID_PhanXuong { get; set; }
        [NotMapped]
        public string? TenPhanXuong { get; set; }
        public int? ID_To { get; set; }
        [NotMapped]
        public string? TenTo { get; set; }
        public int? ID_Kip { get; set; }
        [NotMapped]
        public string? TenKip { get; set; }
        public int? ID_ViTri { get; set; }
        [NotMapped]
        public string? TenViTri { get; set; }
        public int? ID_TinhTrangLamViec { get; set; }
    }
}
