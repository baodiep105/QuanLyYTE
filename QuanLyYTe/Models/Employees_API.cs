using System.ComponentModel.DataAnnotations;

namespace QuanLyYTe.Models
{
    public class Employees_API
    {
        public string result { get; set; }
        public string content { get; set; }
        public List<Employee> data { get; set; }
        public class Employee
        {
            public string manv { get; set; }
            public string hoten { get; set; }
            public string ngaysinh { get; set; }
            public string cmnd { get; set; }
            public string diachi { get; set; }
            public string sodienthoai { get; set; }
            public string email { get; set; }
            public string ngayvaolam { get; set; }
            public int tinhtranglamviec { get; set; }
            public string ngaynghiviec { get; set; }
            public string phongban { get; set; }
            public string phanxuong { get; set; }
            public string tolamviec { get; set; }
            public string tenkip { get; set; }
            public string vitri { get; set; }
        }
    }
}
