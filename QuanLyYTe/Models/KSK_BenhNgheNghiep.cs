using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuanLyYTe.Models
{
    public class KSK_BenhNgheNghiep
    {
        [Key]
        public int ID_KSK_BNN { get; set; }
        public int ID_NV { get; set; }
        [NotMapped]
        public string? MaNV { get; set; }
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
        public int ID_PhongBan { get; set; }
        [NotMapped]
        public string? TenPhongBan { get; set; }
        [NotMapped]
        public int ID_ViTri { get; set; }
        [NotMapped]
        public string? TenViTri { get; set; }
        public int ID_ViTriLaoDong { get; set; }
        [NotMapped]
        public string? TenViTriLaoDong { get; set; }
        [NotMapped]
        public string? TGtiepxuc { get; set; }
        
        public DateTime? NgayLenDanhSach { get; set; }
        public DateTime? NgayKham { get; set; }
        public string? XQuangTimPhoi { get; set; }
        public string? DoCNHoHap {get;set;}
        public string? XQuangCSTLThangNghien {get;set;}
        public string? DoThinhLuc {get;set;}
        public string? DoNhanAp {get;set;}
        public double? DinhLuongHbCo {get;set;}
        public string? DoDienTim {get;set;}
        public double? ThoiGianMauChay {get;set;}
        public double? ThoiGianMauDong {get;set;}
        public string? TestHCV_HBsAg {get;set;}
        public double? SGOT {get;set;}
        public double? SGPT {get;set;}
        public string? NuocTieu {get;set;}
        public string? HIV {get;set;}
        public double? DoPHda {get;set;}
        public string? DoLieuSinhHoc {get;set;}
        public string? KetLuan {get;set;}
        public int? ID_PheDuyet { get; set; }
        public int? ID_TK { get; set; }
        public string? GhiChu { get; set; }
    }
}
