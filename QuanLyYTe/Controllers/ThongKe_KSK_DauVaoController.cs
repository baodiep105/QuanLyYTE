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

        public ThongKe_KSK_DauVaoController(DataContext _context)
        {
            this._context = _context;
        }
        public async Task<IActionResult> Index(DateTime? begind, DateTime? endd, int page = 1)
        {
            DateTime Now = DateTime.Now;
            DateTime startDay = new DateTime(Now.Year, Now.Month, 1);
            DateTime endDay = startDay.AddMonths(1).AddDays(-1);

            var res = await (from a in _context.KSK_DauVao
                             join kq in _context.KetQuaDauVao on a.ID_KetQuaDV equals kq.ID_KetQuaDV
                             join gt in _context.GioiTinh on a.ID_GioiTinh equals gt.ID_GioiTinh
                             join ld in _context.LyDoKhongDat on a.ID_LyDo equals ld.ID_LyDo into ulist1
                             from ld in ulist1.DefaultIfEmpty()
                             select new KSK_DauVao
                             {
                                 ID_KSK_DV = a.ID_KSK_DV,
                                 HoVaTen = a.HoVaTen,
                                 NgaySinh = a.NgaySinh,
                                 CCCD = a.CCCD,
                                 ID_GioiTinh = (int)a.ID_GioiTinh,
                                 TenGioiTinh = gt.TenGioiTinh,
                                 TDHV = a.TDHV,
                                 TDCM = a.TDCM,
                                 NgheNghiep = a.NgheNghiep,
                                 HoKhau = a.HoKhau,
                                 ID_KetQuaDV = (int)a.ID_KetQuaDV,
                                 TenKetQua = kq.TenKetQua,
                                 ID_LyDo = (int?)a.ID_LyDo ?? default,
                                 TenLyDo = ld.TenLyDo ?? default,
                                 NgayKham = a.NgayKham,
                                 GhiChu = a.GhiChu
                             }).ToListAsync();
            if(begind == null && endd == null)
            {
                res = res.Where(x => x.NgayKham >= startDay && x.NgayKham <= endDay).ToList();
            }    
            else
            {
                res = res.Where(x => x.NgayKham >= begind && x.NgayKham <= endd).ToList();
            }    
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

                string fileNamemau = AppDomain.CurrentDomain.DynamicDirectory + @"App_Data\Thong ke KSK tuyen dung.xlsx";
                string fileNamemaunew = AppDomain.CurrentDomain.DynamicDirectory + @"App_Data\Thong ke KSK tuyen dung_Temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNamemau);
                IXLWorksheet Worksheet = Workbook.Worksheet("TD");
                var Data = _context.KSK_DauVao.Where(x => x.NgayKham >= begind && x.NgayKham <= endd).ToList();
                int row = 5, stt = 0, icol = 1;
                if (Data.Count > 0)
                {
                    string NgayKham = "";
                    foreach (var item in Data)
                    {
                        string Day = item.NgayKham.ToString();
                        if (NgayKham != Day)
                        {
                            NgayKham = item.NgayKham.ToString();

                            row++; stt++; icol = 1;

                            Worksheet.Cell(row, icol).Value = stt;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                            icol++; 
                            Worksheet.Cell(row, icol).Value = item.NgayKham;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.DateFormat.Format = "dd-MM-yyyy";


                            icol++;
                            var SoLuongKham = _context.KSK_DauVao.Where(x => x.NgayKham == item.NgayKham).Count();
                            Worksheet.Cell(row, icol).Value = SoLuongKham;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;


                            icol++;
                            var SoLuongDat = _context.KSK_DauVao.Where(x => x.ID_KetQuaDV == 1 && x.NgayKham == item.NgayKham).Count();
                            Worksheet.Cell(row, icol).Value = SoLuongDat;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                            icol++;
                            var SoLuongKhongDat = _context.KSK_DauVao.Where(x => x.ID_KetQuaDV == 2 && x.NgayKham == item.NgayKham).Count();
                            Worksheet.Cell(row, icol).Value = SoLuongKhongDat;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                            icol++;
                            var HinhXam = _context.KSK_DauVao.Where(x => x.ID_LyDo == 1 && x.NgayKham == item.NgayKham).Count();
                            Worksheet.Cell(row, icol).Value = HinhXam;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                            icol++;
                            var ThiLuc = _context.KSK_DauVao.Where(x => x.ID_LyDo == 2 && x.NgayKham == item.NgayKham).Count();
                            Worksheet.Cell(row, icol).Value = ThiLuc;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                            icol++;
                            var BenhLy = _context.KSK_DauVao.Where(x => x.ID_LyDo == 3 && x.NgayKham == item.NgayKham).Count();
                            Worksheet.Cell(row, icol).Value = BenhLy;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                            icol++;
                            var TimMach = _context.KSK_DauVao.Where(x => x.ID_LyDo == 4 && x.NgayKham == item.NgayKham).Count();
                            Worksheet.Cell(row, icol).Value = TimMach;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                            icol++;
                            var ThanKinh = _context.KSK_DauVao.Where(x => x.ID_LyDo == 5 && x.NgayKham == item.NgayKham).Count();
                            Worksheet.Cell(row, icol).Value = ThanKinh;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                            icol++;
                            var TheTrang = _context.KSK_DauVao.Where(x => x.ID_LyDo == 6 && x.NgayKham == item.NgayKham).Count();
                            Worksheet.Cell(row, icol).Value = TheTrang;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                            icol++;
                            var DiTat = _context.KSK_DauVao.Where(x => x.ID_LyDo == 7 && x.NgayKham == item.NgayKham).Count();
                            Worksheet.Cell(row, icol).Value = DiTat;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

   
                            icol++;
                            var Khac = _context.KSK_DauVao.Where(x => x.ID_LyDo == 8 && x.NgayKham == item.NgayKham).Count();
                            Worksheet.Cell(row, icol).Value = Khac;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

     
                            icol++;
                            var XemXet = _context.KSK_DauVao.Where(x => x.ID_KetQuaDV == 3 && x.NgayKham == item.NgayKham).Count();
                            Worksheet.Cell(row, icol).Value = XemXet;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;



                            icol++;
                            var BMI = _context.KSK_DauVao.Where(x => x.ID_LyDo == 9 && x.NgayKham == item.NgayKham).Count();
                            Worksheet.Cell(row, icol).Value = BMI;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                            icol++;
                            var KhongPhuHop = _context.KSK_DauVao.Where(x => x.ID_LyDo == 10 && x.NgayKham == item.NgayKham).Count();
                            Worksheet.Cell(row, icol).Value = KhongPhuHop;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        }    


                    }

                    Worksheet.Range("A6:Q" + (row)).Style.Font.SetFontName("Times New Roman");
                    Worksheet.Range("A6:Q" + (row)).Style.Font.SetFontSize(13);
                    Worksheet.Range("A6:Q" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    Worksheet.Range("A6:Q" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;


                    Workbook.SaveAs(fileNamemaunew);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNamemaunew);
                    string fileName = "Thống kế KSK Tuyển dụng - " + DateTime.Now.Date.ToString("dd/MM/yyyy") + ".xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {


                    Workbook.SaveAs(fileNamemaunew);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNamemaunew);
                    string fileName = "Thống kế KSK Tuyển dụng - " + DateTime.Now.Date.ToString("dd/MM/yyyy") + ".xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
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
