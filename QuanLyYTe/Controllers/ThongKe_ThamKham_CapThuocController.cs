using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyYTe.Models;
using QuanLyYTe.Repositorys;
using Microsoft.AspNetCore.Mvc.Rendering;
using ExcelDataReader;
using System.Data;
using Microsoft.Data.SqlClient;
using ClosedXML.Excel;

namespace QuanLyYTe.Controllers
{
    public class ThongKe_ThamKham_CapThuocController : Controller
    {
        private readonly DataContext _context;

        private readonly IWebHostEnvironment _webHostEnvironment;
        public ThongKe_ThamKham_CapThuocController(DataContext _context, IWebHostEnvironment webHostEnvironment)
        {
            this._context = _context;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index(DateTime? begind, DateTime? endd, int page = 1)
        {
            DateTime Now = DateTime.Now;
           begind = (begind == null && endd == null)? new DateTime(Now.Year, Now.Month, 1) : begind;
           endd = (begind == null && endd == null)? begind?.AddMonths(1).AddDays(-1) : endd;
            

            var res1 = await (from a in _context.PhongBan
                              join cpt in _context.CapPhatThuoc on a.ID_PhongBan equals cpt.ID_PhongBan into list1
                              from cpt in list1.DefaultIfEmpty()
                              join nb in _context.NhomBenh on cpt.ID_NhomBenh equals nb.ID_NhomBenh into list2
                              from nb in list2.DefaultIfEmpty()
                              select new CapPhatThuoc
                              {
                                  ID_CapThuoc = cpt.ID_CapThuoc,
                                  ID_PhongBan = (int?)a.ID_PhongBan ?? default,
                                  TenPhongBan = a.TenPhongBan,
                                  NgayCapThuoc = (DateTime?)cpt.NgayCapThuoc ?? default,
                                 ID_NhomBenh = cpt.ID_NhomBenh ?? default,
                              }).ToListAsync();
                        
            var data = res1.GroupBy(a => a.TenPhongBan)
                          .Select(g => new 
                          {
                              pb = g.Key,
                              coutTong =  g.Count(X => X.NgayCapThuoc>=begind&&X.NgayCapThuoc<=endd),
                              countHH =g.Count(X =>  X.ID_NhomBenh ==1 && X.NgayCapThuoc>=begind&&X.NgayCapThuoc<=endd),
                              countTH = g.Count(X =>  X.ID_NhomBenh ==2 && X.NgayCapThuoc>=begind&&X.NgayCapThuoc<=endd),
                              countTuanHoan = g.Count(X => X.ID_NhomBenh ==3 && X.NgayCapThuoc>=begind&&X.NgayCapThuoc<=endd),
                              countTMH =  g.Count(X =>  X.ID_NhomBenh ==4 && X.NgayCapThuoc>=begind&&X.NgayCapThuoc<=endd),
                              countMat = g.Count(X =>  X.ID_NhomBenh ==5 && X.NgayCapThuoc>=begind&&X.NgayCapThuoc<=endd),
                              countDL = g.Count(X =>  X.ID_NhomBenh == 6 && X.NgayCapThuoc>=begind&&X.NgayCapThuoc<=endd),
                              countKhop =  g.Count(X =>  X.ID_NhomBenh ==7 && X.NgayCapThuoc>=begind&&X.NgayCapThuoc<=endd),
                              countDU =  g.Count(X =>  X.ID_NhomBenh ==8 && X.NgayCapThuoc>=begind&&X.NgayCapThuoc<=endd),
                              countPM =   g.Count(X =>  X.ID_NhomBenh ==9 && X.NgayCapThuoc>=begind&&X.NgayCapThuoc<=endd),
                              countBN =   g.Count(X => X.ID_NhomBenh ==10 && X.NgayCapThuoc>=begind&&X.NgayCapThuoc<=endd),
                              countSot =   g.Count(X => X.ID_NhomBenh ==11 && X.NgayCapThuoc>=begind&&X.NgayCapThuoc<=endd),
                              countKhac =  g.Count(X => X.ID_NhomBenh == 12&& X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd)
                          }).ToList();
            ViewBag.data1 = new
            {
                coutTong =   res1.Count(X => X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                countHH = res1.Count(X => X.ID_NhomBenh == 1 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                countTH =  res1.Count(X => X.ID_NhomBenh == 2 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                countTuanHoan =   res1.Count(X => X.ID_NhomBenh == 3 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                countTMH =  res1.Count(X => X.ID_NhomBenh == 4 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                countMat =   res1.Count(X => X.ID_NhomBenh == 5 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                countDL =   res1.Count(X => X.ID_NhomBenh == 6 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                countKhop =  res1.Count(X => X.ID_NhomBenh == 7 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                countDU =  res1.Count(X => X.ID_NhomBenh == 8 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                countPM =   res1.Count(X => X.ID_NhomBenh == 9 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                countBN =   res1.Count(X => X.ID_NhomBenh == 10 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                countSot =  res1.Count(X => X.ID_NhomBenh == 11 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                countKhac =   res1.Count(X => X.ID_NhomBenh == 12 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd)
            };
            
            ViewBag.data=data;
            ViewData["NhomBenh"] = _context.NhomBenh.ToList();
            return View();

        }

        public async Task<IActionResult> ExportToExcel(DateTime? begind, DateTime? endd, int? IDPhongBan)
        {

            try
            {

                string path = "Form files/thong_ke_CPT.xlsx";
                HttpContext.Response.ContentType = "application/xlsx";
                string filePath = Path.Combine(_webHostEnvironment.ContentRootPath, path);

                if (!System.IO.File.Exists(filePath))
                {
                    return null; // Xử lý lỗi nếu file không tồn tại
                }

                DateTime Now = DateTime.Now;
                begind = (begind == null && endd == null) ? new DateTime(Now.Year, Now.Month, 1) : begind;
                endd = (begind == null && endd == null) ? begind?.AddMonths(1).AddDays(-1) : endd;
                var res1 = await (from a in _context.PhongBan
                                  join cpt in _context.CapPhatThuoc on a.ID_PhongBan equals cpt.ID_PhongBan into list1
                                  from cpt in list1.DefaultIfEmpty()
                                  join nb in _context.NhomBenh on cpt.ID_NhomBenh equals nb.ID_NhomBenh into list2
                                  from nb in list2.DefaultIfEmpty()
                                  select new CapPhatThuoc
                                  {
                                      ID_CapThuoc = cpt.ID_CapThuoc,
                                      ID_PhongBan = (int?)a.ID_PhongBan ?? default,
                                      TenPhongBan = a.TenPhongBan,
                                      NgayCapThuoc = (DateTime?)cpt.NgayCapThuoc ?? default,
                                      ID_NhomBenh = cpt.ID_NhomBenh ?? default,
                                  }).ToListAsync();

                var data = res1.GroupBy(a => a.TenPhongBan)
                              .Select(g => new
                              {
                                  pb = g.Key,
                                  coutTong =  g.Count(X => X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                                  countHH =  g.Count(X => X.ID_NhomBenh == 1 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                                  countTH =  g.Count(X => X.ID_NhomBenh == 2 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                                  countTuanHoan =  g.Count(X => X.ID_NhomBenh == 3 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                                  countTMH =  g.Count(X => X.ID_NhomBenh == 4 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                                  countMat =  g.Count(X => X.ID_NhomBenh == 5 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                                  countDL =   g.Count(X => X.ID_NhomBenh == 6 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                                  countKhop =  g.Count(X => X.ID_NhomBenh == 7 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                                  countDU =   g.Count(X => X.ID_NhomBenh == 8 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                                  countPM =  g.Count(X => X.ID_NhomBenh == 9 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                                  countBN =   g.Count(X => X.ID_NhomBenh == 10 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                                  countSot =   g.Count(X => X.ID_NhomBenh == 11 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                                  countKhac =   g.Count(X => X.ID_NhomBenh == 12 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd)
                              }).ToList();
                var data1 = new
                {
                    coutTong =   res1.Count(X => X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                    countHH =   res1.Count(X => X.ID_NhomBenh == 1 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                    countTH =   res1.Count(X => X.ID_NhomBenh == 2 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                    countTuanHoan =  res1.Count(X => X.ID_NhomBenh == 3 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                    countTMH =   res1.Count(X => X.ID_NhomBenh == 4 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                    countMat =   res1.Count(X => X.ID_NhomBenh == 5 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                    countDL =  res1.Count(X => X.ID_NhomBenh == 6 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                    countKhop =   res1.Count(X => X.ID_NhomBenh == 7 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                    countDU =   res1.Count(X => X.ID_NhomBenh == 8 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                    countPM =   res1.Count(X => X.ID_NhomBenh == 9 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                    countBN =   res1.Count(X => X.ID_NhomBenh == 10 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                    countSot =   res1.Count(X => X.ID_NhomBenh == 11 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd),
                    countKhac =   res1.Count(X => X.ID_NhomBenh == 12 && X.NgayCapThuoc >= begind && X.NgayCapThuoc <= endd)
                };
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1);
                    for (int i = 0; i < data.Count(); i++)
                    {
                        worksheet.Cell(i + 8, 5).Value = i + 1;
                        worksheet.Cell(i + 8, 6).Value = data[i].pb;
                        worksheet.Cell(i + 8, 7).Value = data[i].coutTong;
                        worksheet.Cell(i + 8, 8).Value = data[i].countHH;
                        worksheet.Cell(i + 8, 9).Value = data[i].countTH;
                        worksheet.Cell(i + 8, 10).Value = data[i].countTuanHoan;
                        worksheet.Cell(i + 8, 11).Value = data[i].countTMH;
                        worksheet.Cell(i + 8, 12).Value = data[i].countMat;
                        worksheet.Cell(i + 8, 13).Value = data[i].countDL;
                        worksheet.Cell(i + 8, 14).Value = data[i].countKhop;
                        worksheet.Cell(i + 8, 15).Value = data[i].countDU;
                        worksheet.Cell(i + 8, 16).Value = data[i].countPM;
                        worksheet.Cell(i + 8, 17).Value = data[i].countBN;
                        worksheet.Cell(i + 8, 18).Value = data[i].countSot;
                        worksheet.Cell(i + 8, 19).Value = data[i].countKhac;

                    }
                    var range = worksheet.Range($"E{data.Count() + 8}:F{data.Count() + 8}");
                    range.Merge();
                    range.Value = "Tổng số Khu liên hợp";
                    worksheet.Range($"E{data.Count + 8}:S{data.Count + 8}").Style.Fill.BackgroundColor = XLColor.FromArgb(70, 128, 255);
                    worksheet.Range($"E{data.Count + 8}:S{data.Count + 8}").Style.Font.FontColor = XLColor.White;

                    worksheet.Cell(data.Count() + 8, 7).Value = data1.coutTong;
                    worksheet.Cell(data.Count() + 8, 8).Value = data1.countHH;
                    worksheet.Cell(data.Count() + 8, 9).Value = data1.countTH;
                    worksheet.Cell(data.Count() + 8, 10).Value = data1.countTuanHoan;
                    worksheet.Cell(data.Count() + 8, 11).Value = data1.countTMH;
                    worksheet.Cell(data.Count() + 8, 12).Value = data1.countMat;
                    worksheet.Cell(data.Count() + 8, 13).Value = data1.countDL;
                    worksheet.Cell(data.Count() + 8, 14).Value = data1.countKhop;
                    worksheet.Cell(data.Count() + 8, 15).Value = data1.countDU;
                    worksheet.Cell(data.Count() + 8, 16).Value = data1.countPM;
                    worksheet.Cell(data.Count() + 8, 17).Value = data1.countBN;
                    worksheet.Cell(data.Count() + 8, 18).Value = data1.countSot;
                    worksheet.Cell(data.Count() + 8, 19).Value = data1.countKhac;


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
                return RedirectToAction("Index", "ThongKe_ThamKham_CapThuoc");
            }
        }
    }
}
