using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuanLyYTe.Models
{
    public class ChiTiet_CapPhatThuoc
    {
         
        [Key]
        public int ID_CT_CapThuoc { get; set; } // Khóa chính

        public int? ID_CapThuoc { get; set; } // Khóa ngoại, có thể nullable

        public int? ID_LoaiThuoc { get; set; }

        public string SoLuong { get; set; } // Dữ liệu kiểu string vì cột SoLuong là nvarchar(50)

        [NotMapped] // Bỏ ánh xạ cột không tồn tại trong cơ sở dữ liệu
        public string? TenLoaiThuoc { get; set; }
        

    }
}
