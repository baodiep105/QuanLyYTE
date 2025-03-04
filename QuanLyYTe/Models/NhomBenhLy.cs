using System.ComponentModel.DataAnnotations;

namespace QuanLyYTe.Models
{
    public class NhomBenhLy
    {
        [Key]
        public int ID_BenhLy { get; set; }
        public string TenBenhLy { get; set; }
    }
}
