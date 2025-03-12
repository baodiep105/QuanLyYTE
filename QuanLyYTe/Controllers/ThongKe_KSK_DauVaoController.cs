using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyYTe.Models;
using QuanLyYTe.Repositorys;
using Microsoft.AspNetCore.Mvc.Rendering;
using ExcelDataReader;
using System.Data;
using ClosedXML.Excel;

namespace QuanLyYTe.Controllers
{
    public class ThongKe_KSK_DauVaoController : Controller
    {
        private readonly DataContext _context;

        private readonly IWebHostEnvironment _webHostEnvironment;
        public ThongKe_KSK_DauVaoController(DataContext _context, IWebHostEnvironment webHostEnvironment)
        {
            this._context = _context;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index(DateTime? begind, DateTime? endd, int page = 1)
        {
            DateTime Now = DateTime.Now;
            begind = (begind == null && endd == null) ? new DateTime(Now.Year, Now.Month, 1) : begind;
            endd = (begind == null && endd == null) ? begind?.AddMonths(1).AddDays(-1) : endd;

            var data1 = await (from a in _context.KSK_DauVao
                              join kq in _context.KetQuaDauVao on a.ID_KetQuaDV equals kq.ID_KetQuaDV
                              join gt in _context.GioiTinh on a.ID_GioiTinh equals gt.ID_GioiTinh
                              join ld in _context.LyDoKhongDat on a.ID_LyDo equals ld.ID_LyDo into ulist1
                              from ld in ulist1.DefaultIfEmpty()
                              where a.NgayKham>=begind && a.NgayKham<=endd
                              select a).ToListAsync(); // Lấy dữ liệu trước
            var res = data1.GroupBy(a => a.NgayKham)
                          .Select(g => new ThongKeSKDV
                          {
                              NgayKham = g.Key,
                              CountKham = g.Count(),
                              CountDat = g.Count(x => x.ID_KetQuaDV == 1),
                              CountKDat = g.Count(x => x.ID_KetQuaDV == 2),
                              CountXS = g.Count(x => x.ID_KetQuaDV == 3),
                              CountHinhXam = g.Count(x => x.ID_LyDo == 1),
                              CountThiLuc = g.Count(x => x.ID_LyDo == 2),
                              CountHA = g.Count(x => x.ID_LyDo == 3),
                              CountTM = g.Count(x => x.ID_LyDo == 4),
                              CountTK = g.Count(x => x.ID_LyDo == 5),
                              CountTT = g.Count(x => x.ID_LyDo == 6),
                              CountDT = g.Count(x => x.ID_LyDo == 7),
                              CountKhac = g.Count(x => x.ID_LyDo == 8),
                              CountBMI = g.Count(x => x.ID_LyDo == 9),
                              CountVT = g.Count(x => x.ID_LyDo == 10),
                          }).ToList();
            var resTong = new
            {
                Count = data1.Count(),
                CountDat = data1.Count(x => x.ID_KetQuaDV == 1),
                CountKDat = data1.Count(x => x.ID_KetQuaDV == 2),
                CountXS = data1.Count(x => x.ID_KetQuaDV == 3),
                CountHinhXam = data1.Count(x => x.ID_LyDo == 1),
                CountThiLuc = data1.Count(x => x.ID_LyDo == 2),
                CountHA = data1.Count(x => x.ID_LyDo == 3),
                CountTM = data1.Count(x => x.ID_LyDo == 4),
                CountTK = data1.Count(x => x.ID_LyDo == 5),
                CountTT = data1.Count(x => x.ID_LyDo == 6),
                CountDT = data1.Count(x => x.ID_LyDo == 7),
                CountKhac = data1.Count(x => x.ID_LyDo == 8),
                CountBMI = data1.Count(x => x.ID_LyDo == 9),
                CountVT = data1.Count(x => x.ID_LyDo == 10),
            };
            ViewBag.tong = resTong;
            const int pageSize = 10000;
            if (page < 1)
            {
                page = 1;
            }
            var ct_pl = _context.LyDoKhongDat.ToList();
            ViewData["LyDoKhongDat"] = ct_pl;
            int resCount = res.Count;
            var pager = new Pager(resCount, page, pageSize);
            int recSkip = (page - 1) * pageSize;
            var data = res.Skip(recSkip).Take(pager.PageSize).ToList();
            this.ViewBag.Pager = pager;
            return View(data);


        }
        public async Task<IActionResult> ExportToExcel(DateTime? begind, DateTime? endd, int? IDPhongBan)
        {
   
            try
            {
                string path = "Form files/thong_ke_KDV.xlsx";
                HttpContext.Response.ContentType = "application/xlsx";
                string filePath = Path.Combine(_webHostEnvironment.ContentRootPath, path);

                if (!System.IO.File.Exists(filePath))
                {
                    return null; // Xử lý lỗi nếu file không tồn tại
                }

                DateTime Now = DateTime.Now;
                begind = (begind == null && endd == null) ? new DateTime(Now.Year, Now.Month, 1) : begind;
                endd = (begind == null && endd == null) ? begind?.AddMonths(1).AddDays(-1) : endd;

                var data1 = await (from a in _context.KSK_DauVao
                                   join kq in _context.KetQuaDauVao on a.ID_KetQuaDV equals kq.ID_KetQuaDV
                                   join gt in _context.GioiTinh on a.ID_GioiTinh equals gt.ID_GioiTinh
                                   join ld in _context.LyDoKhongDat on a.ID_LyDo equals ld.ID_LyDo into ulist1
                                   from ld in ulist1.DefaultIfEmpty()
                                   where a.NgayKham >= begind && a.NgayKham <= endd
                                   select a).ToListAsync(); // Lấy dữ liệu trước
                    var res = data1.GroupBy(a => a.NgayKham)
                                  .Select(g => new ThongKeSKDV
                                  {
                                      NgayKham = g.Key,
                                      CountKham = g.Count(),
                                      CountDat = g.Count(x => x.ID_KetQuaDV == 1),
                                      CountKDat = g.Count(x => x.ID_KetQuaDV == 2),
                                      CountXS = g.Count(x => x.ID_KetQuaDV == 3),
                                      CountHinhXam = g.Count(x => x.ID_LyDo == 1),
                                      CountThiLuc = g.Count(x => x.ID_LyDo == 2),
                                      CountHA = g.Count(x => x.ID_LyDo == 3),
                                      CountTM = g.Count(x => x.ID_LyDo == 4),
                                      CountTK = g.Count(x => x.ID_LyDo == 5),
                                      CountTT = g.Count(x => x.ID_LyDo == 6),
                                      CountDT = g.Count(x => x.ID_LyDo == 7),
                                      CountKhac = g.Count(x => x.ID_LyDo == 8),
                                      CountBMI = g.Count(x => x.ID_LyDo == 9),
                                      CountVT = g.Count(x => x.ID_LyDo == 10),
                                  }).ToList();
                var resTong = new
                {
                    Count = data1.Count(),
                    CountDat = data1.Count(x => x.ID_KetQuaDV == 1),
                    CountKDat = data1.Count(x => x.ID_KetQuaDV == 2),
                    CountXS = data1.Count(x => x.ID_KetQuaDV == 3),
                    CountHinhXam = data1.Count(x => x.ID_LyDo == 1),
                    CountThiLuc = data1.Count(x => x.ID_LyDo == 2),
                    CountHA = data1.Count(x => x.ID_LyDo == 3),
                    CountTM = data1.Count(x => x.ID_LyDo == 4),
                    CountTK = data1.Count(x => x.ID_LyDo == 5),
                    CountTT = data1.Count(x => x.ID_LyDo == 6),
                    CountDT = data1.Count(x => x.ID_LyDo == 7),
                    CountKhac = data1.Count(x => x.ID_LyDo == 8),
                    CountBMI = data1.Count(x => x.ID_LyDo == 9),
                    CountVT = data1.Count(x => x.ID_LyDo == 10),
                };
                
                
                    using (var workbook = new XLWorkbook(filePath))
                    {
                        var worksheet = workbook.Worksheet(1);
                    for (var i = 0; i < res.Count; i++)
                    {
                        worksheet.Cell(i + 6, 4).Value = i + 1;
                        worksheet.Cell(i + 6, 5).Value = res[i].NgayKham;
                        worksheet.Cell(i + 6, 6).Value = res[i].CountKham;
                        worksheet.Cell(i + 6, 7).Value = res[i].CountDat;
                        worksheet.Cell(i + 6, 8).Value = res[i].CountKDat;
                        worksheet.Cell(i + 6, 9).Value = res[i].CountXS;
                        worksheet.Cell(i + 6, 10).Value = res[i].CountHinhXam;
                        worksheet.Cell(i + 6, 11).Value = res[i].CountThiLuc;
                        worksheet.Cell(i + 6, 12).Value = res[i].CountHA;
                        worksheet.Cell(i + 6, 13).Value = res[i].CountTM;
                        worksheet.Cell(i + 6, 14).Value = res[i].CountTK;
                        worksheet.Cell(i + 6, 15).Value = res[i].CountTT;
                        worksheet.Cell(i + 6, 16).Value = res[i].CountDT;
                        worksheet.Cell(i + 6, 17).Value = res[i].CountKhac;
                        worksheet.Cell(i + 6, 18).Value = res[i].CountBMI;
                        worksheet.Cell(i + 6, 19).Value = res[i].CountVT;
                    }
                    var a= worksheet.Range($"D{res.Count + 6}:S{res.Count + 6}").Style.Fill.BackgroundColor = XLColor.FromArgb(70,128,255);
                    worksheet.Range($"D{res.Count + 6}:S{res.Count + 6}").Style.Font.FontColor = XLColor.White;
                    var range = worksheet.Range($"D{res.Count + 6}:E{res.Count + 6}");
                    range.Merge();
                    range.Value = "Tổng số Khu liên hợp";
                    worksheet.Cell(res.Count + 6, 6).Value = resTong.Count;
                    worksheet.Cell(res.Count + 6, 7).Value = resTong.CountDat;
                    worksheet.Cell(res.Count + 6, 8).Value = resTong.CountKDat;
                    worksheet.Cell(res.Count + 6, 9).Value = resTong.CountXS;
                    worksheet.Cell(res.Count + 6, 10).Value = resTong.CountHinhXam;
                    worksheet.Cell(res.Count + 6, 11).Value = resTong.CountThiLuc;
                    worksheet.Cell(res.Count + 6, 12).Value = resTong.CountHA;
                    worksheet.Cell(res.Count + 6, 13).Value = resTong.CountTM;
                    worksheet.Cell(res.Count + 6, 14).Value = resTong.CountTK;
                    worksheet.Cell(res.Count + 6, 15).Value = resTong.CountTT;
                    worksheet.Cell(res.Count + 6, 16).Value = resTong.CountDT;
                    worksheet.Cell(res.Count + 6, 17).Value = resTong.CountKhac;
                    worksheet.Cell(res.Count + 6, 18).Value = resTong.CountBMI;
                    worksheet.Cell(res.Count + 6, 19).Value = resTong.CountVT;
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
                return RedirectToAction("Index", "ThongKe_KSK_DauVao");
            }
        }
    }
}
