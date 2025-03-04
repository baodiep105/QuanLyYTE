using System.ComponentModel.DataAnnotations;

namespace QuanLyYTe.Models
{
    public class ViTriLamViec
    {
        [Key]
        public int ID_ViTri { get; set; }
        
        public string? TenViTri { get; set; }

        public Nullable<int> LoaiViTri { get; set; }
    }
}
