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
    public class ThoiHan_KSK_DKController : Controller
    {
        private readonly DataContext _context;
        public ThoiHan_KSK_DKController(DataContext _context)
        {
            this._context = _context;
        }
        public async Task<IActionResult> Index(DateTime? begind, DateTime? endd, int? IDPhongBan, int page = 1)
        {

            ViewBag.PBList = new SelectList(_context.PhongBan.ToList(), "ID_PhongBan", "TenPhongBan", IDPhongBan);
            var MNV = User.FindFirstValue(ClaimTypes.Name);
            var check = _context.TaiKhoan.Where(x => x.TenDangNhap == MNV).FirstOrDefault();
            var res = await (from a in _context.SoTheoDoi_KSK.Where(x => x.ThoiHanSKS_TiepTheo >= begind && x.ThoiHanSKS_TiepTheo <= endd)
                             join nv in _context.NhanVien on a.ID_NV equals nv.ID_NV
                             join gt in _context.GioiTinh on a.ID_GioiTinh equals gt.ID_GioiTinh
                             join bp in _context.PhongBan on a.ID_PhongBan equals bp.ID_PhongBan
                             join vt in _context.ViTriLamViec on nv.ID_ViTri equals vt.ID_ViTri
                             join k in _context.KipLamViec on nv.ID_Kip equals k.ID_Kip
                             join nm in _context.NhomMau on a.ID_NhomMau equals nm.ID_NhomMau into ulist1
                             from nm in ulist1.DefaultIfEmpty()
                             select new SoTheoDoi_KSK
                             {
                                 ID_STD = a.ID_STD,
                                 ID_NV = (int)a.ID_NV,
                                 MaNV = nv.MaNV,
                                 HoTen = nv.HoTen,
                                 CCCD = nv.CMND,
                                 NgaySinh = (DateTime?)nv.NgaySinh ?? default,
                                 NgayNhanViec = (DateTime?)nv.NgayVaoLam ?? default,
                                 ID_PhongBan = (int)a.ID_PhongBan,
                                 TenPhongBan = bp.TenPhongBan,
                                 ID_ViTri = (int)nv.ID_NV,
                                 TenViTri = vt.TenViTri,
                                 ID_Kip = (int)nv.ID_Kip,
                                 TenKip = k.TenKip,
                                 ID_NhomMau = (int?)a.ID_NhomMau ?? default,
                                 TenNhomMau = nm.TenNhomMau,
                                 ID_GioiTinh = (int)a.ID_GioiTinh,
                                 TenGioiTinh = gt.TenGioiTinh,
                                 ThoiHanSKS_TiepTheo = (DateTime)a.ThoiHanSKS_TiepTheo
                             }).ToListAsync();
            if (check.ID_Quyen != 1 && check.ID_Quyen != 2)
            {
                res = res.Where(x => x.ID_PhongBan == check.ID_PhongBan).ToList();
            }
            if (IDPhongBan != null)
            {
                res = res.Where(x => x.ID_PhongBan == IDPhongBan).ToList();
            }
            var ct_vt = _context.ViTriLamViec.ToList();
            ViewData["ViTriLamViec"] = ct_vt;
            const int pageSize = 20;
            if (page < 1)
            {
                page = 1;
            }
            int resCount = res.Count;
            var pager = new Pager(resCount, page, pageSize);
            int recSkip = (page - 1) * pageSize;
            var data = res.Skip(recSkip).Take(pager.PageSize).ToList();
            this.ViewBag.Pager = pager;
            return View(data);

        }
        private List<SoTheoDoi_KSK> GetDemarcation(DateTime? begind, DateTime? endd, int? IDPhongBan)
        {
            var res = (from a in _context.SoTheoDoi_KSK.Where(x => x.ThoiHanSKS_TiepTheo >= begind && x.ThoiHanSKS_TiepTheo <= endd)
                       join nv in _context.NhanVien on a.ID_NV equals nv.ID_NV
                       join gt in _context.GioiTinh on a.ID_GioiTinh equals gt.ID_GioiTinh
                       join m in _context.NhomMau on a.ID_NhomMau equals m.ID_NhomMau into ulist1
                       from m in ulist1.DefaultIfEmpty()
                       join bp in _context.PhongBan on nv.ID_PhongBan equals bp.ID_PhongBan
                       join vt in _context.ViTriLamViec on nv.ID_ViTri equals vt.ID_ViTri
                       join k in _context.KipLamViec on nv.ID_Kip equals k.ID_Kip
                       select new SoTheoDoi_KSK
                       {
                           ID_STD = a.ID_STD,
                           ID_NV = (int)a.ID_NV,
                           MaNV = nv.MaNV,
                           HoTen = nv.HoTen,
                           CCCD = nv.CMND,
                           NgaySinh = (DateTime?)nv.NgaySinh ?? default,
                           NgayNhanViec = (DateTime?)nv.NgayVaoLam ?? default,
                           ID_PhongBan = (int)nv.ID_PhongBan,
                           TenPhongBan = bp.TenPhongBan,
                           ID_ViTri = (int)nv.ID_NV,
                           TenViTri = vt.TenViTri,
                           ID_Kip = (int)nv.ID_Kip,
                           TenKip = k.TenKip,
                           ID_NhomMau = (int?)a.ID_NhomMau ?? default,
                           TenNhomMau = m.TenNhomMau,
                           ID_GioiTinh = (int)a.ID_GioiTinh,
                           TenGioiTinh = gt.TenGioiTinh,
                           ThoiHanSKS_TiepTheo = (DateTime)a.ThoiHanSKS_TiepTheo
                       }).ToList();
            if (IDPhongBan != null)
            {
                res = res.Where(x => x.ID_PhongBan == IDPhongBan).ToList();
            }

            return res;
        }
        public async Task<IActionResult> ExportToExcel(DateTime? begind, DateTime? endd, int? IDPhongBan)
        {
            string Ten_CBNV = "";
            try
            {
              
                string fileNamemau = AppDomain.CurrentDomain.DynamicDirectory + @"App_Data\BM_CBNV_KSK.xlsx";
                string fileNamemaunew = AppDomain.CurrentDomain.DynamicDirectory + @"App_Data\BM_CBNV_KSK_Temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNamemau);
                IXLWorksheet Worksheet = Workbook.Worksheet("CBNV");
                List<SoTheoDoi_KSK> Data = GetDemarcation(begind, endd, IDPhongBan);
                int row = 5, stt = 0, icol = 1;
                if (Data.Count > 0)
                {
                    foreach (var item in Data)
                    {

                        row++; stt++; icol = 1;

                        Worksheet.Cell(row, icol).Value = stt;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                        icol++;

                        Worksheet.Cell(row, icol).Value = item.MaNV;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                        Ten_CBNV = item.MaNV;

                        icol++;
                        Worksheet.Cell(row, icol).Value = item.HoTen;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                        icol++;
                        Worksheet.Cell(row, icol).Value = item.NgaySinh;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                        Worksheet.Cell(row, icol).Style.DateFormat.Format = "dd-MM-yyyy";

                        icol++;
                        Worksheet.Cell(row, icol).Value = item.TenGioiTinh;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        icol++;
                        Worksheet.Cell(row, icol).Value = item.TenViTri;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                        icol++;
                        Worksheet.Cell(row, icol).Value = item.TenKip;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        icol++;
                        Worksheet.Cell(row, icol).Value = item.NgayNhanViec;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                        Worksheet.Cell(row, icol).Style.DateFormat.Format = "dd-MM-yyyy";


                        icol++;
                        Worksheet.Cell(row, icol).Value = item.TenPhongBan;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        icol++;
                        Worksheet.Cell(row, icol).Value = "Hòa Phát Dung Quất";
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        icol++;
                        Worksheet.Cell(row, icol).Value = "X";
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        icol++;
                        Worksheet.Cell(row, icol).Value = item.TenNhomMau;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        //Khám phụ khoa
                        icol++;
                        if (item.ID_GioiTinh == 2)
                        {
                            Worksheet.Cell(row, icol).Value = "X";
                        }
                        else
                        {
                            Worksheet.Cell(row, icol).Value = "";
                        }
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        //Khám vị trí lái xe
                        icol++;
                        var Check_LX = _context.ViTriLamViec.Where(x => x.TenViTri == item.TenViTri && x.LoaiViTri == 1).FirstOrDefault();
                        if (Check_LX != null)
                        {
                            Worksheet.Cell(row, icol).Value = "X";
                        }
                        else
                        {
                            Worksheet.Cell(row, icol).Value = "";
                        }
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                        //Khám vị trí thuyền viên
                        icol++;
                        var Check_VT = _context.ViTriLamViec.Where(x => x.TenViTri == item.TenViTri && x.LoaiViTri == 2).FirstOrDefault();
                        if (Check_VT != null)
                        {
                            Worksheet.Cell(row, icol).Value = "X";
                        }
                        else
                        {
                            Worksheet.Cell(row, icol).Value = "";
                        }
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                    }

                    Worksheet.Range("A7:P" + (row)).Style.Font.SetFontName("Times New Roman");
                    Worksheet.Range("A7:P" + (row)).Style.Font.SetFontSize(13);
                    Worksheet.Range("A7:P" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    Worksheet.Range("A7:P" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;


                    Workbook.SaveAs(fileNamemaunew);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNamemaunew);
                    string fileName = "Danh sách KSK Định kỳ - " + DateTime.Now.Date.ToString("dd/MM/yyyy") + ".xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {


                    Workbook.SaveAs(fileNamemaunew);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNamemaunew);
                    string fileName = "Danh sách KSK Định kỳ - " + DateTime.Now.Date.ToString("dd/MM/yyyy") + ".xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Có lỗi khi truy xuất dữ liệu. Vui lòng kiểm tra mã nhân viên: " + Ten_CBNV + "');</script>";
                return RedirectToAction("Index", "ThoiHan_KSK_DK");
            }
        }
    }
}
