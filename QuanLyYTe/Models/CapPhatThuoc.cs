using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuanLyYTe.Models
{
    public class CapPhatThuoc
    {
        [Key]
        public int? ID_CapThuoc { get; set; }
        public int? ID_NV { get; set; }
        [NotMapped]
        public string? MaNV { get; set; }
        [NotMapped]
        public string? HoTen { get; set; }
        public int? ID_PhongBan { get; set; }
        [NotMapped]
        public string? TenPhongBan { get; set; }
        public string? SoDienThoai { get; set; }
        public DateTime? NgayCapThuoc { get; set; }
        public string? ThoiGianDen { get; set; }
        public string? ThoiGianDi { get; set; }
        public string? SoPhutLuuLai { get; set; }
        public int? ID_NhomBenh { get; set; }
        [NotMapped]
        public string? TenNhomBenh { get; set; }
        public string? GhiChu { get; set; }
        public List<ChiTiet_CapPhatThuoc>? detail { get; set; }
    }
}
