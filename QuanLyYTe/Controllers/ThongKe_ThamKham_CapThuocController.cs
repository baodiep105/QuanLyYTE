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

        public ThongKe_ThamKham_CapThuocController(DataContext _context)
        {
            this._context = _context;
        }
        public async Task<IActionResult> Index(DateTime? begind, DateTime? endd, int page = 1)
        {
            DateTime Now = DateTime.Now;
            DateTime startDay = new DateTime(Now.Year, Now.Month, 1);
            DateTime endDay = startDay.AddMonths(1).AddDays(-1);

            var res = await (from a in _context.CapPhatThuoc
                             join nv in _context.NhanVien on a.ID_NV equals nv.ID_NV into ulist1
                             from nv in ulist1.DefaultIfEmpty()
                             join pb in _context.PhongBan on a.ID_PhongBan equals pb.ID_PhongBan into ulist2
                             from pb in ulist2.DefaultIfEmpty()
                             join b in _context.NhomBenh on a.ID_NhomBenh equals b.ID_NhomBenh into ulist3
                             from b in ulist3.DefaultIfEmpty()
                             select new CapPhatThuoc
                             {
                                 ID_CapThuoc = a.ID_CapThuoc,
                                 ID_NV = (int?)a.ID_NV ?? default,
                                 MaNV = nv.MaNV,
                                 HoTen = nv.HoTen,
                                 SoDienThoai = a.SoDienThoai,
                                 ID_PhongBan = (int?)a.ID_PhongBan ?? default,
                                 TenPhongBan = pb.TenPhongBan,
                                 NgayCapThuoc = (DateTime?)a.NgayCapThuoc ?? default,
                                 ThoiGianDen = a.ThoiGianDen,
                                 ThoiGianDi = a.ThoiGianDi,
                                 SoPhutLuuLai = a.SoPhutLuuLai,
                                 ID_NhomBenh = (int?)a.ID_NhomBenh ?? default,
                                 TenNhomBenh = b.TenNhomBenh,
                                 GhiChu = a.GhiChu
                             }).ToListAsync();

            if (begind == null && endd == null)
            {
                res = res.Where(x => x.NgayCapThuoc >= startDay && x.NgayCapThuoc <= endDay).ToList();
            }
            else
            {
                res = res.Where(x => x.NgayCapThuoc >= begind && x.NgayCapThuoc <= endd).ToList();
            }
            const int pageSize = 10000;
            if (page < 1)
            {
                page = 1;
            }
            var bp_nm = _context.PhongBan.ToList();
            ViewData["PhongBan"] = bp_nm;
            int resCount = res.Count;
            var pager = new Pager(resCount, page, pageSize);
            int recSkip = (page - 1) * pageSize;
            var data = res.Skip(recSkip).Take(pager.PageSize).ToList();
            this.ViewBag.Pager = pager;
            var ct_pl = _context.NhomBenh.ToList();
            ViewData["NhomBenh"] = ct_pl;
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
                int SoThang = Convert.ToInt32(DenNgay) - Convert.ToInt32(TuNgay);
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


                            var Count = _context.CapPhatThuoc.Where(x => x.ID_PhongBan == item.ID_PhongBan && x.NgayCapThuoc >= startDay && x.NgayCapThuoc <= endDay).Count();
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
                return RedirectToAction("Index", "ThongKe_ThamKham_CapThuoc");
            }
        }
    }
}
