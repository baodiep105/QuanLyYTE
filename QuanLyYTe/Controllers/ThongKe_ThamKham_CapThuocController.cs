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
                             /* )
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
                                 ID_PhongBan = (int?)a.ID_PhongBan ?? default,
                                 TenPhongBan = pb.TenPhongBan,
                                 NgayCapThuoc = (DateTime?)a.NgayCapThuoc ?? default
                                 ID_NhomBenh = (int?)a.ID_NhomBenh ?? default,
                             }).ToListAsync();*/

            if (begind == null && endd == null)
            {
                res1 = res1.Where(x => x.NgayCapThuoc >= startDay && x.NgayCapThuoc <= endDay).ToList();
            }
            else
            {
                res1 = res1.Where(x => x.NgayCapThuoc >= begind && x.NgayCapThuoc <= endd).ToList();
            }
            var data = res1.GroupBy(a => a.TenPhongBan)
                          .Select(g => new 
                          {
                              pb = g.Key,
                              coutTong = g.Count(X => X.ID_CapThuoc!=null),
                              countHH = g.Count(X => X.ID_CapThuoc != null && X.ID_NhomBenh == 1),
                              countTH = g.Count(X => X.ID_CapThuoc != null && X.ID_NhomBenh == 2),
                              countTuanHoan = g.Count(X => X.ID_CapThuoc != null && X.ID_NhomBenh == 3),
                              countTMH = g.Count(X => X.ID_CapThuoc != null && X.ID_NhomBenh == 4),
                              countMat = g.Count(X => X.ID_CapThuoc != null && X.ID_NhomBenh == 5),
                              countDL = g.Count(X => X.ID_CapThuoc != null && X.ID_NhomBenh == 6),
                              countKhop = g.Count(X => X.ID_CapThuoc != null && X.ID_NhomBenh == 7),
                              countDU = g.Count(X => X.ID_CapThuoc != null && X.ID_NhomBenh == 8),
                              countPM = g.Count(X => X.ID_CapThuoc != null && X.ID_NhomBenh == 9),
                              countBN = g.Count(X => X.ID_CapThuoc != null && X.ID_NhomBenh == 10),
                              countSot = g.Count(X => X.ID_CapThuoc != null && X.ID_NhomBenh == 11),
                              countKhac = g.Count(X => X.ID_CapThuoc != null && X.ID_NhomBenh == 12)
                          }).ToList();
            ViewBag.data1 = new
            {
                coutTong = res1.Count(X => X.ID_CapThuoc != null),
                countHH = res1.Count(X => X.ID_CapThuoc != null && X.ID_NhomBenh == 1),
                countTH = res1.Count(X => X.ID_CapThuoc != null && X.ID_NhomBenh == 2),
                countTuanHoan = res1.Count(X => X.ID_CapThuoc != null && X.ID_NhomBenh == 3),
                countTMH = res1.Count(X => X.ID_CapThuoc != null && X.ID_NhomBenh == 4),
                countMat = res1.Count(X => X.ID_CapThuoc != null && X.ID_NhomBenh == 5),
                countDL = res1.Count(X => X.ID_CapThuoc != null && X.ID_NhomBenh == 6),
                countKhop = res1.Count(X => X.ID_CapThuoc != null && X.ID_NhomBenh == 7),
                countDU = res1.Count(X => X.ID_CapThuoc != null && X.ID_NhomBenh == 8),
                countPM = res1.Count(X => X.ID_CapThuoc != null && X.ID_NhomBenh == 9),
                countBN = res1.Count(X => X.ID_CapThuoc != null && X.ID_NhomBenh == 10),
                countSot = res1.Count(X => X.ID_CapThuoc != null && X.ID_NhomBenh == 11),
                countKhac = res1.Count(X => X.ID_CapThuoc != null && X.ID_NhomBenh == 12)
            };
            
            ViewBag.data=data;
            ViewData["NhomBenh"] = _context.NhomBenh.ToList();
            return View();

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
