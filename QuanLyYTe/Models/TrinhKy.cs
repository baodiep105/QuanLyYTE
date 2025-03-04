using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuanLyYTe.Models
{
    public class TrinhKy
    {
        [Key]
        public int ID_TK { get; set; }
        public int ID_PhongBan { get; set; }
        [NotMapped]
        public string? TenPhongBan { get; set; }
        public string? NoiDung { get; set; }
        public DateTime? NgayTrinhKy { get; set; }
        public string? FilePath { get; set; }
        public Nullable<int> NguoiLap { get; set; }
        [NotMapped]
        public string? MaNV_NguoiLap { get; set; }
        [NotMapped]
        public string? HoTen_NguoiLap { get; set; }
        public Nullable<int> TinhTrang_NguoiLap { get; set; }
        public DateTime? Ngay_NguoiLap { get; set; }

        public Nullable< int> TruongPho { get; set; }
        [NotMapped]
        public string? HoTen_TruongPho { get; set; }
        public Nullable<int> TinhTrang_TruongPho { get; set; }
        public DateTime? Ngay_TruongPho { get; set; }
        public string? GhiChu { get; set; }
        [NotMapped]
        public int? TinhTrang_PheDuyet { get; set; }
    }
}
