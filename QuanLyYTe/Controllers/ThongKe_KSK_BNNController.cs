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

        public ThongKe_KSK_BNNController(DataContext _context)
        {
            this._context = _context;
        }
        public async Task<IActionResult> Index( DateTime? begind, DateTime? endd, int page = 1)
        {
            DateTime Now = DateTime.Now;
            DateTime startDay = new DateTime(Now.Year, Now.Month, 1);
            DateTime endDay = startDay.AddMonths(1).AddDays(-1);

            var res = await (from a in _context.KSK_BenhNgheNghiep
                             join nv in _context.NhanVien on a.ID_NV equals nv.ID_NV
                             join bp in _context.PhongBan on a.ID_PhongBan equals bp.ID_PhongBan
                             join k in _context.KipLamViec on nv.ID_Kip equals k.ID_Kip into ulist3
                             from k in ulist3.DefaultIfEmpty()
                             join vt in _context.ViTriLamViec on nv.ID_ViTri equals vt.ID_ViTri into ulist4
                             from vt in ulist4.DefaultIfEmpty()
                             join vtld in _context.ViTriLaoDong on a.ID_ViTriLaoDong equals vtld.ID_ViTriLaoDong into ulist5
                             from vtld in ulist5.DefaultIfEmpty()
                             select new KSK_BenhNgheNghiep
                             {
                                 ID_KSK_BNN = a.ID_KSK_BNN,
                                 ID_NV = (int)a.ID_NV,
                                 MaNV = nv.MaNV,
                                 HoTen = nv.HoTen,
                                 NgaySinh = (DateTime?)nv.NgaySinh ?? default,
                                 ID_PhongBan = (int)a.ID_PhongBan,
                                 TenPhongBan = bp.TenPhongBan,
                                 TenKip = k.TenKip,
                                 TenViTri = vt.TenViTri,
                                 NgayKham = (DateTime?)a.NgayKham ?? default,
                                 NgayLenDanhSach = (DateTime?)a.NgayLenDanhSach ?? default,
                                 ID_ViTriLaoDong = (int)a.ID_ViTriLaoDong,
                                 TenViTriLaoDong = vtld.TenViTriLaoDong,
                                 GhiChu = a.GhiChu,
                                 ID_PheDuyet = (int?)a.ID_PheDuyet ?? default
                             }).ToListAsync();

            if (begind == null && endd == null)
            {
                res = res.Where(x => x.NgayLenDanhSach >= startDay && x.NgayLenDanhSach <= endDay).ToList();
            }
            else
            {
                res = res.Where(x => x.NgayLenDanhSach >= begind && x.NgayLenDanhSach <= endd).ToList();
            }
            const int pageSize = 10000;
            if (page < 1)
            {
                page = 1;
            }
            int resCount = res.Count;
            var pager = new Pager(resCount, page, pageSize);
            int recSkip = (page - 1) * pageSize;
            var data = res.Skip(recSkip).Take(pager.PageSize).ToList();
            this.ViewBag.Pager = pager;
            var bp_nm = _context.PhongBan.ToList();
            ViewData["PhongBan"] = bp_nm;
            return View(data);

        }
        public async Task<IActionResult> ExportToExcel(DateTime? begind, DateTime? endd, int? IDPhongBan)
        {

            try
            {

                string fileNamemau = AppDomain.CurrentDomain.DynamicDirectory + @"App_Data\Thong ke.xlsx";
                string fileNamemaunew = AppDomain.CurrentDomain.DynamicDirectory + @"App_Data\Thong ke_Temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNamemau);
                IXLWorksheet Worksheet = Workbook.Worksheet("TD");
                var Data = _context.PhongBan.ToList();
                int row = 5, stt = 0, icol = 1, icol_ = 1, row_ = 5;

                DateTime Begin = (DateTime)begind;
                string TuNgay = Begin.ToString("MM");
                DateTime End = (DateTime)endd;
                string DenNgay = End.ToString("MM");
                int SoThang =  Convert.ToInt32(DenNgay) - Convert.ToInt32(TuNgay);
                if (Data.Count > 0)
                {

                 

                    foreach (var item in Data)
                    {
                        row++; stt++; icol = 1; icol_ = 1;

                        Worksheet.Cell(row, icol).Value = stt;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                        icol++;
                        Worksheet.Cell(row, icol).Value = item.TenPhongBan;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                        for (int i = 0; i <= SoThang; i++)
                        {
                            DateTime Now = Begin.AddMonths(i);
                            DateTime startDay = new DateTime(Now.Year, Now.Month, 1);
                            DateTime endDay = startDay.AddMonths(1).AddDays(-1);

                            icol_++;
                            Worksheet.Cell(row_, (icol_ + 1)).Value = startDay.ToString("MM/yyyy");
                            Worksheet.Cell(row_, (icol_ + 1)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell(row_, (icol_ + 1)).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row_, (icol_ + 1)).Style.Alignment.WrapText = true;


                            var Count = _context.KSK_BenhNgheNghiep.Where(x => x.ID_PhongBan == item.ID_PhongBan && x.NgayKham >= startDay && x.NgayKham <= endDay).Count();
                            Worksheet.Cell(row, (icol_ + 1)).Value = Count;
                            Worksheet.Cell(row, (icol_ + 1)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, (icol_ + 1)).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        }
                    }

                    Worksheet.Range("A6:N" + (row)).Style.Font.SetFontName("Times New Roman");
                    Worksheet.Range("A6:N" + (row)).Style.Font.SetFontSize(13);
                    Worksheet.Range("A6:N" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    Worksheet.Range("A6:N" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;


                    Workbook.SaveAs(fileNamemaunew);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNamemaunew);
                    string fileName = "Thống kê KSK BNN - " + DateTime.Now.Date.ToString("dd/MM/yyyy") + ".xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {


                    Workbook.SaveAs(fileNamemaunew);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNamemaunew);
                    string fileName = "Thống kê KSK BNN - " + DateTime.Now.Date.ToString("dd/MM/yyyy") + ".xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
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
