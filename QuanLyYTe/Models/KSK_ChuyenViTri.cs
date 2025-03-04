using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuanLyYTe.Models
{
    public class KSK_ChuyenViTri
    {
        [Key]
        public int ID_KSK_CVT { get; set; }
        public int ID_NV { get; set; }
        [NotMapped]
        public string? MaNV { get; set; }
        [NotMapped]
        public string? HoTen { get; set; }
        [NotMapped]
        public DateTime? NgaySinh { get; set; }

        [NotMapped]
        public int ID_Kip { get; set; }
        [NotMapped]
        public string? TenKip { get; set; }
        [NotMapped]
        public int ID_PhongBan { get; set; }
        [NotMapped]
        public string? TenPhongBan { get; set; }
        public int ID_ViTri { get; set; }
        [NotMapped]
        public string? TenViTri { get; set; }

        public DateTime? NgayKham { get; set; }
        public string? Dat { get; set; }
        public string? KhongDat { get; set; }
        public Nullable<int> LyDoKhongDat { get; set; }
        [NotMapped]
        public string? TenLyDoKhongDat { get; set; }
        public string? GhiChu { get; set; }

    }
}
