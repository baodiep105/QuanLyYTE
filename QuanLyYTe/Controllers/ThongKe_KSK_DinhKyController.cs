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
            begind = (begind == null && endd == null) ? new DateTime(Now.Year, Now.Month, 1) : begind;
            endd = (begind == null && endd == null) ? begind?.AddMonths(1).AddDays(-1) : endd;

            var res = await (from a in _context.KSK_DinhKy
                             join nv in _context.NhanVien on a.ID_NV equals nv.ID_NV
                             join pb in _context.PhongBan on nv.ID_PhongBan equals pb.ID_PhongBan
                             join gt in _context.GioiTinh on a.ID_GioiTinh equals gt.ID_GioiTinh
                             join l in _context.PhanLoaiKSK on a.ID_PhanLoaiKSK equals l.ID_PhanLoaiKSK
                             join vt in _context.ViTriLamViec on a.ID_ViTri equals vt.ID_ViTri
                             join m in _context.NhomMau on a.ID_NhomMau equals m.ID_NhomMau into ulist1
                             from m in ulist1.DefaultIfEmpty()
                             where a.NgayKSK>=begind && a.NgayKSK<=endd
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

                string path = "Form files/thong_ke_KDK.xlsx";
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
                                 join nv in _context.NhanVien on a.ID_PhongBan equals nv.ID_PhongBan into list
                                 from nv in list.DefaultIfEmpty()
                                 join ksk in _context.KSK_DinhKy on nv.ID_NV equals ksk.ID_NV into list1
                                 from ksk in list1.DefaultIfEmpty()
                                 join l in _context.PhanLoaiKSK on ksk.ID_PhanLoaiKSK equals l.ID_PhanLoaiKSK into list2
                                 from l in list2.DefaultIfEmpty()
                                 select new 
                                 {
                                    ID_PhongBan=a.ID_PhongBan,
                                    TenPhongBan=a.TenPhongBan,
                                    ID_KSK_DK=(int?)ksk.ID_PhanLoaiKSK,
                                    ID_PhanLoaiKSK=(int?)l.ID_PhanLoaiKSK,
                                    NgayKSK=(DateTime?)ksk.NgayKSK

                                 }).ToListAsync();
             
               
                var data = res.GroupBy(x =>x.TenPhongBan).Select(y =>
                new
                {
                    pb = y.Key,
                    tong =  y.Count(z =>  z.NgayKSK >= begind && z.NgayKSK <= endd),
                    loai1 =y.Count(z=>  z.ID_PhanLoaiKSK == 1 && z.NgayKSK>=begind && z.NgayKSK<=endd ),
                    loai2 =y.Count(z=>  z.ID_PhanLoaiKSK == 2 && z.NgayKSK >=begind && z.NgayKSK<=endd ),
                    loai3 =y.Count(z=>  z.ID_PhanLoaiKSK == 3 && z.NgayKSK >=begind && z.NgayKSK<=endd ),
                    loai4 =y.Count(z=>  z.ID_PhanLoaiKSK == 4 && z.NgayKSK >=begind && z.NgayKSK<=endd ),
                    loai5 =y.Count(z=>  z.ID_PhanLoaiKSK == 5 && z.NgayKSK >=begind && z.NgayKSK<=endd ),
                    loai6 = y.Count(z=> z.ID_PhanLoaiKSK == 6 && z.NgayKSK >=begind && z.NgayKSK<=endd )
                }).ToList() ;
                var data1 = new
                {
                    tong =   res.Count(z => z.NgayKSK >= begind && z.NgayKSK <= endd),
                    loai1 =  res.Count(z => z.ID_PhanLoaiKSK == 1 && z.NgayKSK >= begind && z.NgayKSK <= endd),
                    loai2 =  res.Count(z => z.ID_PhanLoaiKSK == 2 && z.NgayKSK >= begind && z.NgayKSK <= endd),
                    loai3 =  res.Count(z => z.ID_PhanLoaiKSK == 3 && z.NgayKSK >= begind && z.NgayKSK <= endd),
                    loai4 =  res.Count(z => z.ID_PhanLoaiKSK == 4 && z.NgayKSK >= begind && z.NgayKSK <= endd),
                    loai5 =  res.Count(z => z.ID_PhanLoaiKSK == 5 && z.NgayKSK >= begind && z.NgayKSK <= endd),
                    loai6 =  res.Count(z => z.ID_PhanLoaiKSK == 6 && z.NgayKSK >= begind && z.NgayKSK <= endd)

                };
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1);
                    for (int i = 0; i < data.Count(); i++)
                    {
                        worksheet.Cell(i + 6, 2).Value = i + 1;
                        worksheet.Cell(i + 6, 3).Value = data[i].pb;
                        worksheet.Cell(i + 6, 4).Value = data[i].tong;
                        worksheet.Cell(i + 6, 5).Value = data[i].loai1;
                        worksheet.Cell(i + 6, 6).Value = data[i].loai2;
                        worksheet.Cell(i + 6, 7).Value = data[i].loai3;
                        worksheet.Cell(i + 6, 8).Value = data[i].loai4;
                        worksheet.Cell(i + 6, 9).Value = data[i].loai5;
                        worksheet.Cell(i + 6, 10).Value = data[i].loai6;
                    }
                    var range = worksheet.Range($"B{data.Count()+6}:C{data.Count()+6}");
                    range.Merge();
                    range.Value= "Tổng số Khu liên hợp";
                    worksheet.Range($"B{res.Count + 6}:J{res.Count + 6}").Style.Fill.BackgroundColor = XLColor.FromArgb(70, 128, 255);
                    worksheet.Range($"B{res.Count + 6}:J{res.Count + 6}").Style.Font.FontColor = XLColor.White;

                    worksheet.Cell(data.Count() + 6, 4).Value = data1.tong;
                    worksheet.Cell(data.Count() + 6, 5).Value = data1.loai1;
                    worksheet.Cell(data.Count() + 6, 6).Value = data1.loai2;
                    worksheet.Cell(data.Count() + 6, 7).Value = data1.loai3;
                    worksheet.Cell(data.Count() + 6, 8).Value = data1.loai4;
                    worksheet.Cell(data.Count() + 6, 9).Value = data1.loai5;
                    worksheet.Cell(data.Count() + 6, 10).Value = data1.loai6;

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

