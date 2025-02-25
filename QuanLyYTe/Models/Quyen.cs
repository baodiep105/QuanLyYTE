using System.ComponentModel.DataAnnotations;

namespace QuanLyYTe.Models
{
    public class Quyen
    {
        [Key]
        public int ID_Q { get; set; }
        public string? TenQuyen { get; set; }
    }
}
