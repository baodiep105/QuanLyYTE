using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyYTe.Models
{
    public class KSK_DauVao
    {
        [Key]
        public int ID_KSK_DV { get; set; }
        public string? HoVaTen { get; set; }
        public DateTime? NgaySinh { get; set; }
        public int ID_GioiTinh { get; set; }
        [NotMapped]
        public string? TenGioiTinh { get; set; }
        public string? CCCD { get; set; }
        public string? TDHV { get; set; }
        public string? TDCM { get; set; }
        public string? NgheNghiep { get; set; }
        public string? HoKhau { get; set; }
        public int ID_KetQuaDV { get; set; }
        [NotMapped]
        public string? TenKetQua { get; set; }
        public Nullable< int> ID_LyDo { get; set; }
        [NotMapped]
        public string? TenLyDo { get; set; }
        public DateTime? NgayKham { get; set; }
        public string? GhiChu { get; set; }
        [NotMapped]
        public int Page { get; set; }
    }
}
