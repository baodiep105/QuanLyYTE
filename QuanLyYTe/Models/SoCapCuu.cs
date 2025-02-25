using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuanLyYTe.Models
{
    public class SoCapCuu
    {
        [Key]
        public int? ID_SCC { get; set; }
        public DateTime? NgayThangNam { get; set; }
        public int? ID_NV { get; set; }
        [NotMapped]
        public string? MaNV { get; set; }
        [NotMapped]
        public DateTime? NgaySinh { get; set; }
        [NotMapped]
        public string? HoTen { get; set; }
        [NotMapped]
        public int? ID_PhongBan { get; set; }
        [NotMapped]
        public string? TenPhongBan { get; set; }
        public int? ID_GioiTinh { get; set; }
        [NotMapped]
        public string? TenGioiTinh { get; set; }
        public string? ThoiGianTiepNhan { get; set; }
        public string? ThoiGianCapCuu { get; set; }
        public Nullable<int> TaiNan { get; set; }
        [NotMapped]
        public string? TenTaiNan { get; set; }
        public Nullable<int> BenhLy { get; set; }
        [NotMapped]
        public string? TenBenhLy { get; set; }
        public string? DienBien { get; set; }
        public string? PhanLoaiNT { get; set; }
        public string? YeuToGayTaiNan { get; set; }
        public string? XuLyCapCuu { get; set; }
        public int? ThoiGianNghiViec { get; set; }
        public string? KetQuaGiamDinh { get; set; }
        public string? SoDienThoai { get; set; }
        public string? BienBan24h { get; set; }
        [NotMapped]
        public string? TenBenhVien { get; set; }
        [NotMapped]
        public string? YTePhuTrach { get; set; }
        [NotMapped]
        public string? ThoiGianDiChuyenVien { get; set; }
        [NotMapped]
        public string? TamUng { get; set; }
        [NotMapped]
        public string? ThanhToan { get; set; }
        [NotMapped]
        public string? ChungTu { get; set; }
        [NotMapped]
        public string? ThoiGianDieuTri { get; set; }
        [NotMapped]
        public string? BVTuyenHai { get; set; }
        public string? TongChiPhi { get; set; }
        public string? KhongCanKT_SK { get; set; }
        public string? KetQuaKT_SK { get; set; }
        public string? GhiChu { get; set; }

    }
}
