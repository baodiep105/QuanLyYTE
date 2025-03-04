using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyYTe.Models
{
    public class ToLamViec
    {
        [Key]
        public int ID_To { get; set; }
        public string? TenTo { get; set; }
    }
}
