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
    public class ThongKe_KSK_DinhKyController : Controller
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ThongKe_KSK_DinhKyController(DataContext _context, IWebHostEnvironment webHostEnvironment)
        {
            this._context = _context;
            _webHostEnvironment = webHostEnvironment;
        }
        
        
        public async Task<IActionResult> Index(DateTime? begind, DateTime? endd, int page = 1)
        {
            DateTime Now = DateTime.Now;
            DateTime startDay = new DateTime(Now.Year, Now.Month, 1);
            DateTime endDay = startDay.AddMonths(1).AddDays(-1);

            var res = await (from a in _context.KSK_DinhKy
                             join nv in _context.NhanVien on a.ID_NV equals nv.ID_NV
                             join pb in _context.PhongBan on nv.ID_PhongBan equals pb.ID_PhongBan
                             join gt in _context.GioiTinh on a.ID_GioiTinh equals gt.ID_GioiTinh
                             join l in _context.PhanLoaiKSK on a.ID_PhanLoaiKSK equals l.ID_PhanLoaiKSK
                             join vt in _context.ViTriLamViec on a.ID_ViTri equals vt.ID_ViTri
                             join m in _context.NhomMau on a.ID_NhomMau equals m.ID_NhomMau into ulist1
                             from m in ulist1.DefaultIfEmpty()
                             select new KSK_DinhKy
                             {
                                 ID_KSK_DK = a.ID_KSK_DK,
                                 ID_NV = a.ID_NV,
                                 MaNV = nv.MaNV,
                                 HoVaTen = nv.HoTen,
                                 NgaySinh = nv.NgaySinh,
                                 ID_ViTri = (int)a.ID_ViTri,
                                 TenViTri = vt.TenViTri,
                                 ID_PhongBan = (int)nv.ID_PhongBan,
                                 TenPhongBan = pb.TenPhongBan,
                                 ID_GioiTinh = (int)a.ID_GioiTinh,
                                 TenGioiTinh = gt.TenGioiTinh,
                                 KhamTongQuat = a.KhamTongQuat,
                                 KhamPhuKhoa = a.KhamPhuKhoa,
                                 ID_NhomMau = (int?)a.ID_NhomMau ?? default,
                                 TenNhomMau = m.TenNhomMau,
                                 NhomMauRh = a.NhomMauRh,
                                 CongThucMau = a.CongThucMau,
                                 NuocTieu = a.NuocTieu,
                                 ID_PhanLoaiKSK = a.ID_PhanLoaiKSK,
                                 TenLoaiKSK = l.TenLoaiKSK,
                                 KetLuanKSK = a.KetLuanKSK,
                                 NgayKSK = (DateTime)a.NgayKSK

                             }).ToListAsync();
            if (begind == null && endd == null)
            {
                res = res.Where(x => x.NgayKSK >= startDay && x.NgayKSK <= endDay).ToList();
            }
            else
            {
                res = res.Where(x => x.NgayKSK >= begind && x.NgayKSK <= endd).ToList();
            }
            const int pageSize = 10000;
            var bp_nm = _context.PhongBan.ToList();
            if (page < 1)
            {
                page = 1;
            }
            int resCount = bp_nm.Count;
            ViewData["tong"] = resCount;
            var pager = new Pager(resCount, page, pageSize);
            int recSkip = (page - 1) * pageSize;
            var data = bp_nm.Skip(recSkip).Take(pager.PageSize).ToList();
            this.ViewBag.Pager = pager;
            
            ViewData["data"] = res;
            var ct_pl = _context.PhanLoaiKSK.ToList();
            ViewData["PhanLoaiKSK"] = ct_pl;
            ViewData["endd"] = endd?.ToString("yyyy-MM-dd");
            ViewData["begind"] = begind?.ToString("yyyy-MM-dd");
            return View(bp_nm);

        }

        public async Task<IActionResult> ExportToExcel(DateTime? begind, DateTime? endd, int? IDPhongBan)
        {

            try
            {

                string path = "Form files/BM_KSK_DinhKy.xlsx";
                HttpContext.Response.ContentType = "application/xlsx";
                string filePath = Path.Combine(_webHostEnvironment.ContentRootPath, path);

                if (!System.IO.File.Exists(filePath))
                {
                    return null; // Xử lý lỗi nếu file không tồn tại
                }
                
                DateTime Now = DateTime.Now;
                DateTime startDay = new DateTime(Now.Year, Now.Month, 1);
                DateTime endDay = startDay.AddMonths(1).AddDays(-1);
                var res = await (from a in _context.KSK_DinhKy
                                 join nv in _context.NhanVien on a.ID_NV equals nv.ID_NV
                                 join pb in _context.PhongBan on nv.ID_PhongBan equals pb.ID_PhongBan
                                 join gt in _context.GioiTinh on a.ID_GioiTinh equals gt.ID_GioiTinh
                                 join l in _context.PhanLoaiKSK on a.ID_PhanLoaiKSK equals l.ID_PhanLoaiKSK
                                 join m in _context.NhomMau on a.ID_NhomMau equals m.ID_NhomMau into ulist1
                                 from m in ulist1.DefaultIfEmpty()
                                 select new KSK_DinhKy
                                 {
                                     MaNV = nv.MaNV,
                                     HoVaTen = nv.HoTen,
                                     TenGioiTinh = gt.TenGioiTinh,
                                     KhamTongQuat = a.KhamTongQuat,
                                     KhamPhuKhoa = a.KhamPhuKhoa,
                                     TenNhomMau = m.TenNhomMau,
                                     NhomMauRh = a.NhomMauRh,
                                     CongThucMau = a.CongThucMau,
                                     NuocTieu = a.NuocTieu,
                                     TenLoaiKSK = l.TenLoaiKSK,
                                     KetLuanKSK = a.KetLuanKSK,
                                     NgayKSK = (DateTime)a.NgayKSK

                                 }).ToListAsync();
                if (begind == null && endd == null)
                {
                    res = res.Where(x => x.NgayKSK >= startDay && x.NgayKSK <= endDay).ToList();
                }
                else
                {
                    res = res.Where(x => x.NgayKSK >= begind && x.NgayKSK <= endd).ToList();
                }
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1);
                    for (int i = 0; i < res.Count(); i++)
                    {
                        worksheet.Cell(i + 6, 1).Value = i + 1;
                        worksheet.Cell(i + 6, 2).Value = res[i].MaNV;
                        worksheet.Cell(i + 6, 3).Value = res[i].HoVaTen;
                        worksheet.Cell(i + 6, 4).Value = res[i].TenGioiTinh;
                        worksheet.Cell(i + 6, 5).Value = res[i].KhamTongQuat;
                        worksheet.Cell(i + 6, 6).Value = res[i].KhamPhuKhoa;
                        worksheet.Cell(i + 6, 7).Value = res[i].TenNhomMau;
                        worksheet.Cell(i + 6, 8).Value = res[i].NhomMauRh;
                        worksheet.Cell(i + 6, 9).Value = res[i].CongThucMau;
                        worksheet.Cell(i + 6, 10).Value = res[i].NuocTieu;
                        worksheet.Cell(i + 6, 11).Value = res[i].TenLoaiKSK;
                        worksheet.Cell(i + 6, 12).Value = res[i].KetLuanKSK;
                        worksheet.Cell(i + 6, 13).Value = res[i].NgayKSK;
                    }
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

