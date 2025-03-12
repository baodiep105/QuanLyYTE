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
    public class NhanVienController : Controller
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public NhanVienController(DataContext _context, IWebHostEnvironment webHostEnvironment)
        {
            this._context = _context;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index(string search, int? tt,int page = 1)
        {
            tt = tt ?? 1;
            var res = await (from a in _context.NhanVien
                             join bp in _context.PhongBan on a.ID_PhongBan equals bp.ID_PhongBan
                             join px in _context.PhanXuong on a.ID_PhanXuong equals px.ID_PhanXuong into ulist1
                             from px in ulist1.DefaultIfEmpty()
                             join to in _context.ToLamViec on a.ID_To equals to.ID_To into ulist2
                             from to in ulist2.DefaultIfEmpty()
                             join k in _context.KipLamViec on a.ID_Kip equals k.ID_Kip into ulist3
                             from k in ulist3.DefaultIfEmpty()
                             join vt in _context.ViTriLamViec on a.ID_ViTri equals vt.ID_ViTri into ulist4
                             from vt in ulist4.DefaultIfEmpty()
                             select new NhanVien
                             {
                                 MaNV = a.MaNV,
                                 HoTen = a.HoTen,
                                 CMND = a.CMND,
                                 NgaySinh = (DateTime?)a.NgaySinh ?? default,
                                 DiaChi = a.DiaChi,
                                 NgayVaoLam = (DateTime?)a.NgayVaoLam ?? default,
                                 ID_PhongBan = (int?)a.ID_PhongBan ?? default,
                                 TenPhongBan = bp.TenPhongBan,
                                 ID_PhanXuong = (int?)a.ID_PhanXuong ?? default,
                                 TenPhanXuong = px.TenPhanXuong,
                                 ID_To = (int?)a.ID_To ?? default,
                                 TenTo = to.TenTo,
                                 ID_Kip = (int?)a.ID_Kip ?? default,
                                 TenKip = k.TenKip,
                                 ID_ViTri = (int?)a.ID_ViTri ?? default,
                                 TenViTri = vt.TenViTri,
                                 ID_TinhTrangLamViec = (int)a.ID_TinhTrangLamViec
                             }).ToListAsync();
            if (search != null)
            {
                res = res.Where(x => x.HoTen.ToLower().Contains(search.ToLower()) || x.MaNV.ToLower().Contains(search.ToLower())).ToList();

            }
            if (tt != null)
            {
                res=res.Where(x=>x.ID_TinhTrangLamViec==tt).ToList();
            }
            const int pageSize = 20;
            if (page < 1)
            {
                page = 1;
            }
            int resCount = res.Count;
            Pager pager = new Pager(resCount, page, pageSize);
            int recSkip = (page - 1) * pageSize;
            var data = res.Skip(recSkip).Take(pager.PageSize).ToList();
            ViewBag.Pager = pager;
            return View(data);


        }

        public async Task<IActionResult> Export(int? tt)
        {
            try
            {
                string path = "Form files/danh_sach_nhan_vien.xlsx";
                HttpContext.Response.ContentType = "application/xlsx";
                string filePath = Path.Combine(_webHostEnvironment.ContentRootPath, path);

                if (!System.IO.File.Exists(filePath))
                {
                    return null; // Xử lý lỗi nếu file không tồn tại
                }
                var res = await (from a in _context.NhanVien.Where(x => x.ID_TinhTrangLamViec == tt)
                                 join bp in _context.PhongBan on a.ID_PhongBan equals bp.ID_PhongBan
                                 join px in _context.PhanXuong on a.ID_PhanXuong equals px.ID_PhanXuong into ulist1
                                 from px in ulist1.DefaultIfEmpty()
                                 join to in _context.ToLamViec on a.ID_To equals to.ID_To into ulist2
                                 from to in ulist2.DefaultIfEmpty()
                                 join k in _context.KipLamViec on a.ID_Kip equals k.ID_Kip into ulist3
                                 from k in ulist3.DefaultIfEmpty()
                                 join vt in _context.ViTriLamViec on a.ID_ViTri equals vt.ID_ViTri into ulist4
                                 from vt in ulist4.DefaultIfEmpty()
                                 select new NhanVien
                                 {
                                     MaNV = a.MaNV,
                                     HoTen = a.HoTen,
                                     CMND = a.CMND,
                                     NgaySinh = (DateTime?)a.NgaySinh ?? default,
                                     DiaChi = a.DiaChi,
                                     NgayVaoLam = (DateTime?)a.NgayVaoLam ?? default,

                                     TenPhongBan = bp.TenPhongBan,

                                     TenPhanXuong = px.TenPhanXuong,

                                     TenTo = to.TenTo,

                                     TenKip = k.TenKip,

                                     TenViTri = vt.TenViTri,
                                     ID_TinhTrangLamViec = (int)a.ID_TinhTrangLamViec
                                 }).ToListAsync();

                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1);
                    for (int i = 0; i < res.Count(); i++)
                    {
                        worksheet.Cell(i + 5, 2).Value = i + 1;
                        worksheet.Cell(i + 5, 3).Value = res[i].MaNV;
                        worksheet.Cell(i + 5, 4).Value = res[i].HoTen;
                        worksheet.Cell(i + 5, 5).Value = res[i].CMND;
                        worksheet.Cell(i + 5, 6).Value = res[i].TenPhongBan;
                        worksheet.Cell(i + 5, 7).Value = res[i].TenPhanXuong;
                        worksheet.Cell(i + 5, 8).Value = res[i].TenTo;
                        worksheet.Cell(i + 5, 9).Value = res[i].TenViTri;
                        worksheet.Cell(i + 5, 10).Value = res[i].TenKip;
                        worksheet.Cell(i + 5, 11).Value = (tt == 1) ? "Đang làm việc" : "Đã nghỉ việc";
                    }
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Seek(0, SeekOrigin.Begin);
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", (tt == 1) ? "danh_sach_nhan_vien_dang_lam_viec.xlsx" : "danh_sach_nhan_vien_da_nghi_viec.xlsx");
                    }
                }
            }
            catch
            {
                TempData["msgSuccess"] = "<script>alert('Có lỗi khi truy xuất dữ liệu');</script>";
                return RedirectToAction("Index", "NhanVien");
            }
        }
    }
}
