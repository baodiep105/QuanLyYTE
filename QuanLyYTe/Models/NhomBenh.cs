using System.ComponentModel.DataAnnotations;

namespace QuanLyYTe.Models
{
    public class NhomBenh
    {
        [Key]
        public int ID_NhomBenh { get; set; }
        public string TenNhomBenh { get; set; }
    }
}
