using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace QuanLyYTe.Models
{
    public class SoTheoDoi_KSK
    {
        [Key]
        public int ID_STD { get; set; }
        public int? ID_NV { get; set; }
        public Nullable<int> ID_ViTriLaoDong { get; set; }
        [NotMapped]
        public string? TenViTriLaoDong { get; set; }
        [NotMapped]
        public string? TenLoai { get; set; }
        public int? ID_PhongBan { get; set; }
        [NotMapped]
        public string? TenPhongBan { get; set; }
        public Nullable< int> ID_NhomMau { get; set; }
        [NotMapped]
        public string? TenNhomMau { get; set; }
        public int? ID_GioiTinh { get; set; }
        public int? ID_PhanLoai { get; set; }
        [NotMapped]
        public string? TenGioiTinh { get; set; }
        public DateTime? ThoiHanSKS_Truoc { get; set; }
        public DateTime? ThoiHanSKS_TiepTheo { get; set; }
        [NotMapped]
        public string? MaNV { get; set; }
        [NotMapped]
        public string? CCCD { get; set; }
        [NotMapped]
        public string? HoTen { get; set; }
        [NotMapped]
        public DateTime? NgaySinh { get; set; }
         [NotMapped]
        public DateTime? NgayNhanViec { get; set; }
        [NotMapped]
        public int ID_Kip { get; set; }
        [NotMapped]
        public string? TenKip { get; set; }
        [NotMapped]
        public int ID_ViTri { get; set; }
        [NotMapped]
        public string? TenViTri { get; set; }
        [NotMapped]
        public string? TenPLSK { get; set; }
        [NotMapped]
        public int? idPLSK { get; set; }
        

    }
}
