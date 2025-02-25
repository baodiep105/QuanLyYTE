using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuanLyYTe.Models
{
    public class KSK_DinhKy
    {
        [Key]
        public int ID_KSK_DK { get; set; }
        public int ID_NV { get; set; }
        public int ID_ViTri { get; set; }
        [NotMapped]
        public string? TenViTri { get; set; }
        [NotMapped]
        public string? MaNV { get; set; }
        [NotMapped]
        public string? HoVaTen { get; set; }
        [NotMapped]
        public DateTime? NgaySinh { get; set; }

        public int? ID_GioiTinh { get; set; }
        [NotMapped]
        public string? TenGioiTinh { get; set; }

        [NotMapped]
        public int ID_PhongBan { get; set; }
        [NotMapped]
        public string? TenPhongBan { get; set; }
        public string? KhamTongQuat { get; set; }
        public string? KhamPhuKhoa { get; set; }
        public int? ID_NhomMau { get; set; }
        [NotMapped]
        public string TenNhomMau { get; set; }
        public string? NhomMauRh { get; set; }
        public string? CongThucMau { get; set; }
        public string? NuocTieu { get; set; }
        public int ID_PhanLoaiKSK { get; set; }
        [NotMapped]
        public string? TenLoaiKSK { get; set; }

        public string? KetLuanKSK { get; set; }
        public DateTime? NgayKSK { get; set; }
    }
}
