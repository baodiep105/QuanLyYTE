using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyYTe.Models;
using QuanLyYTe.Repositorys;
using Microsoft.AspNetCore.Mvc.Rendering;
using ExcelDataReader;
using System.Data;
using ClosedXML.Excel;
using System.Security.Claims;

namespace QuanLyYTe.Controllers
{
    public class ThongKe_KSK_BNNController : Controller
    {
        private readonly DataContext _context;

        private readonly IWebHostEnvironment _webHostEnvironment;
        public ThongKe_KSK_BNNController(DataContext _context, IWebHostEnvironment webHostEnvironment)
        {
            this._context = _context;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index( DateTime? begind, DateTime? endd)
        {
            DateTime Now = DateTime.Now;
            begind = (begind == null && endd == null) ? new DateTime(Now.Year, Now.Month, 1) : begind;
            endd = (begind == null && endd == null) ? begind?.AddMonths(1).AddDays(-1) : endd;

            var res =  (from a in _context.KSK_BenhNgheNghiep
                             join nv in _context.NhanVien on a.ID_NV equals nv.ID_NV
                             join bp in _context.PhongBan on a.ID_PhongBan equals bp.ID_PhongBan
                             where a.NgayKham >= begind && a.NgayKham <= endd
                        select new KSK_BenhNgheNghiep
                             {
                                 ID_PhongBan = (int)a.ID_PhongBan,
                                 TenPhongBan = bp.TenPhongBan,
                                NgayKham = a.NgayKham
                        }).ToList();
            
            List<object> data = new List<object>();
            _context.PhongBan.ToList().ForEach(x =>
            {
                int count = res.Where(y => y.ID_PhongBan == x.ID_PhongBan).Count();
                data.Add( new
                    {
                        pb = x.TenPhongBan,
                        count = count
                    });
            });
            ViewBag.tong = data;
            return View();

        }
        public async Task<IActionResult> ExportToExcel(DateTime? begind, DateTime? endd, int? IDPhongBan)
        {

            try
            {

                string path = "Form files/Thong_ke_BNN.xlsx";
                HttpContext.Response.ContentType = "application/xlsx";
                string filePath = Path.Combine(_webHostEnvironment.ContentRootPath, path);

                if (!System.IO.File.Exists(filePath))
                {
                    return null; // Xử lý lỗi nếu file không tồn tại
                }

                DateTime Now = DateTime.Now;
                begind = (begind == null && endd == null) ? new DateTime(Now.Year, Now.Month, 1) : begind;
                endd = (begind == null && endd == null) ? begind?.AddMonths(1).AddDays(-1) : endd;
                var res = await (from a in _context.PhongBan
                                 join b in _context.KSK_BenhNgheNghiep on a.ID_PhongBan equals b.ID_PhongBan into list
                                 from b in list.DefaultIfEmpty()
                                 select new
                                 {
                                     pb=a.TenPhongBan,
                                     ngay = b.NgayKham
                                 }).ToListAsync();
                var data = res.GroupBy(x => x.pb).Select(y => new
                {
                    pb = y.Key,
                    tong =  y.Count(x => x.ngay >= begind && x.ngay <= endd)
                }).ToList();
                var data1 = new
                {
                    tong = res.Count(x => x.ngay >= begind && x.ngay <= endd) 
                };
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1);
                    for (int i = 0; i < data.Count(); i++)
                    {
                        worksheet.Cell(i + 5, 9).Value = i + 1;
                        worksheet.Cell(i + 5, 10).Value = data[i].pb;
                        worksheet.Cell(i + 5, 11).Value = data[i].tong;
                        
                    }
                    var range = worksheet.Range($"I{data.Count() + 5}:J{data.Count() + 5}");
                    range.Merge();
                    range.Value = "Tổng số Khu liên hợp";
                    worksheet.Range($"I{data.Count + 5}:K{data.Count + 5}").Style.Fill.BackgroundColor = XLColor.FromArgb(70, 128, 255);
                    worksheet.Range($"I{data.Count + 5}:K{data.Count + 5}").Style.Font.FontColor = XLColor.White;

                    worksheet.Cell(data.Count() + 5, 11).Value = data1.tong;
                    

                    // Lưu lại file Excel
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Seek(0, SeekOrigin.Begin);
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", path);
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Có lỗi khi truy xuất dữ liệu');</script>";
                return RedirectToAction("Index", "ThongKe_KSK_BNN");
            }
        }
    }
}
