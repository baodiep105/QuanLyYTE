using System.ComponentModel.DataAnnotations;

namespace QuanLyYTe.Models
{
    public class NhomMau
    {
        [Key]
        public int ID_NhomMau { get; set; }
        public string TenNhomMau { get; set; }
    }
}
