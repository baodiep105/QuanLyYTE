using System.ComponentModel.DataAnnotations;

namespace QuanLyYTe.Models
{
    public class LyDoKhongDat
    {
        [Key]
        public int ID_LyDo { get; set; }
        public string? TenLyDo { get; set; }
        public int LoaiLyDo { get; set; }
    }
}
