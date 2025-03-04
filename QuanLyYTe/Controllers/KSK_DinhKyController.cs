using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyYTe.Models;
using QuanLyYTe.Repositorys;
using Microsoft.AspNetCore.Mvc.Rendering;
using ExcelDataReader;
using System.Data;
using System.Globalization;
using System.Text;
using Microsoft.Data.SqlClient;
using DocumentFormat.OpenXml.Office2010.Excel;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Hosting;
namespace QuanLyYTe.Controllers
{
    public class KSK_DinhKyController : Controller
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public KSK_DinhKyController(DataContext _context, IWebHostEnvironment webHostEnvironment)
        {
            this._context = _context;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index(DateTime? begind, DateTime? endd, int? IDPhongBan, int page = 1)
        {
                ViewBag.PBList = new SelectList(_context.PhongBan.ToList(), "ID_PhongBan", "TenPhongBan", IDPhongBan);
            var res = await (from a in _context.KSK_DinhKy.Where(x => x.NgayKSK >= begind && x.NgayKSK <= endd)
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
            if(IDPhongBan != null)
            {
                res= res.Where(x=>x.ID_PhongBan == IDPhongBan).ToList();

            }    

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
            ViewBag.begin = begind;
            ViewBag.end = endd;
            ViewBag.id = IDPhongBan;
            return View(data);   

        }


        public async Task<IActionResult> Deatail(int? ID_NV, int page = 1)
        {
          
            var res = await (from a in _context.KSK_DinhKy.Where(x => x.ID_NV == ID_NV)
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
            var id_nv = _context.NhanVien.Where(x => x.ID_NV == ID_NV).FirstOrDefault();
            if (id_nv != null)
            {
                ViewBag.ID_NV = id_nv.MaNV;
            }
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


        public FileResult TestDownloadPCF()
        {  
            string path = "Form files/BM_KSK_DinhKy.xlsx";
            HttpContext.Response.ContentType = "application/xlsx";
            string filePath = Path.Combine(_webHostEnvironment.ContentRootPath, path);

            if (!System.IO.File.Exists(filePath))
            {
                return null; // Xử lý lỗi nếu file không tồn tại
            }
            
            List<PhanLoaiKSK> lsk = _context.PhanLoaiKSK.ToList();
            List<NhomMau> nm = _context.NhomMau.ToList();

            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheet("Sheet2");
                for (var i = 0; i < lsk.Count; i++)
                {
                    worksheet.Cell(i + 2, 10).Value = lsk[i].TenLoaiKSK;
                }
                for (var i = 0; i < nm.Count; i++)
                {
                    worksheet.Cell(i + 2, 12).Value = nm[i].TenNhomMau;
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
        public async Task<IActionResult> ImportExcel()
        {
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return RedirectToAction("Index", "KSK_DinhKy");
                }

                string webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string dirPath = Path.Combine(webRootPath, "ReceivedReports");
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                string dataFileName = $"{DateTime.Now:yyyyMMddHHmm}.xlsx";  // Đảm bảo có phần mở rộng
                string saveToPath = Path.Combine(dirPath, dataFileName);

                using (FileStream stream = new FileStream(saveToPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);  // Dùng CopyToAsync thay vì CopyTo
                }

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                using var streamOpen = new FileStream(saveToPath, FileMode.Open);
                using var reader = ExcelReaderFactory.CreateReader(streamOpen);

                var ds = reader.AsDataSet();
                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable serviceDetails = ds.Tables[0];
                    for (int i = 5; i < serviceDetails.Rows.Count; i++)
                    {
                        string MaNV = serviceDetails.Rows[i][1]?.ToString()?.Trim() ?? string.Empty;
                        if (string.IsNullOrEmpty(MaNV)) continue;

                        var check_nv = await _context.NhanVien.FirstOrDefaultAsync(x => x.MaNV == MaNV);
                        if (check_nv == null)
                        {
                            TempData["msgSuccess"] = $"<script>alert('Vui lòng cập nhật dữ liệu nhân viên: {MaNV}');</script>";
                            return RedirectToAction("Index", "KSK_DinhKy");
                        }

                        string GioiTinh = serviceDetails.Rows[i][3]?.ToString()?.Trim() ?? string.Empty;
                        var check_gioitinh = await _context.GioiTinh.FirstOrDefaultAsync(x => x.TenGioiTinh == GioiTinh);
                        if (check_gioitinh == null)
                        {
                            TempData["msgSuccess"] = $"<script>alert('Vui lòng kiểm tra tên giới tính. Nhân viên: {MaNV}');</script>";
                            return RedirectToAction("Index", "KSK_DinhKy");
                        }

                        string NhomMau = serviceDetails.Rows[i][6]?.ToString()?.Trim() ?? string.Empty;
                        var check_nhommau = await _context.NhomMau.FirstOrDefaultAsync(x => x.TenNhomMau == NhomMau);
                        var SoTheoDoi = await _context.SoTheoDoi_KSK.FirstOrDefaultAsync(x => x.ID_NV == check_nv.ID_NV);

                        if (SoTheoDoi != null && (SoTheoDoi.ID_NhomMau == 0 || SoTheoDoi.ID_NhomMau == null))
                        {
                            if (check_nhommau != null)
                            {
                                await _context.Database.ExecuteSqlRawAsync("EXEC SoTheoDoi_KSK_update_nhommau {0},{1}", SoTheoDoi.ID_STD, check_nhommau.ID_NhomMau);
                            }
                        }

                        string XepLoai = serviceDetails.Rows[i][10]?.ToString()?.Trim() ?? string.Empty;
                        var check_xeploai = await _context.PhanLoaiKSK.FirstOrDefaultAsync(x => x.TenLoaiKSK == XepLoai);
                        if (check_xeploai == null)
                        {
                            TempData["msgSuccess"] = $"<script>alert('Vui lòng kiểm tra xếp loại sức khỏe. Nhân viên: {MaNV}');</script>";
                            return RedirectToAction("Index", "KSK_DinhKy");
                        }

                        string NgayKham = serviceDetails.Rows[i][12]?.ToString()?.Trim() ?? string.Empty;
                        if (DateTime.TryParseExact(NgayKham, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime Ngay_Kham))
                        {
                            List<SqlParameter> sp = new List<SqlParameter>{
                                new SqlParameter("@ID_NV", SqlDbType.Int) { Value = check_nv.ID_NV },
                                new SqlParameter("@ID_ViTri", SqlDbType.Int) { Value = check_nv.ID_ViTri },
                                new SqlParameter("@ID_GioiTinh", SqlDbType.Int) { Value = check_gioitinh.ID_GioiTinh },
                                new SqlParameter("@KhamTongQuat", SqlDbType.NVarChar) { Value = serviceDetails.Rows[i][4]?.ToString() ?? DBNull.Value.ToString() },
                                new SqlParameter("@KhamPhuKhoa", SqlDbType.NVarChar) { Value = serviceDetails.Rows[i][5]?.ToString() ?? DBNull.Value.ToString() },
                                new SqlParameter("@ID_NhomMau", SqlDbType.Int) { Value = (object?)check_nhommau?.ID_NhomMau ?? DBNull.Value },
                                new SqlParameter("@NhomMauRh", SqlDbType.NVarChar) { Value = serviceDetails.Rows[i][7]?.ToString() ?? DBNull.Value.ToString() },
                                new SqlParameter("@CongThucMau", SqlDbType.NVarChar) { Value = serviceDetails.Rows[i][8]?.ToString() ?? DBNull.Value.ToString() },
                                new SqlParameter("@NuocTieu", SqlDbType.NVarChar) { Value = serviceDetails.Rows[i][9]?.ToString() ?? DBNull.Value.ToString() },
                                new SqlParameter("@ID_PhanLoaiKSK", SqlDbType.Int) { Value = check_xeploai.ID_PhanLoaiKSK },
                                new SqlParameter("@KetLuanKSK", SqlDbType.NVarChar) { Value = serviceDetails.Rows[i][11]?.ToString() ?? DBNull.Value.ToString() },
                                new SqlParameter("@NgayKSK", SqlDbType.DateTime) { Value = Ngay_Kham }
                             };
                            var check_ = await _context.KSK_DinhKy.FirstOrDefaultAsync(x => x.ID_NV == check_nv.ID_NV && x.NgayKSK == Ngay_Kham);
                            if (check_ != null)
                            {
                                sp.Add(new SqlParameter("@ID_KSK_DK", SqlDbType.Int) { Value = check_.ID_KSK_DK });
                                var result = _context.Database.ExecuteSqlRaw("EXEC KSK_DinhKy_update @ID_KSK_DK,@ID_NV,@ID_ViTri,@ID_GioiTinh,@KhamTongQuat,@KhamPhuKhoa,@ID_NhomMau,@NhomMauRh,@CongThucMau,@NuocTieu,@ID_PhanLoaiKSK,@KetLuanKSK,@NgayKSK", sp);
                            }
                            else
                            {
                                await _context.Database.ExecuteSqlRawAsync(
                                     "EXEC KSK_DinhKy_insert @ID_NV, @ID_ViTri, @ID_GioiTinh, @KhamTongQuat, @KhamPhuKhoa, @ID_NhomMau, @NhomMauRh, @CongThucMau, @NuocTieu, @ID_PhanLoaiKSK, @KetLuanKSK, @NgayKSK",
                                     sp
                                 );
                            
                            }
                        }
                        else
                        {
                            TempData["msgError"] = $"<script>alert('Ngày khám không hợp lệ cho nhân viên: {MaNV}');</script>";
                            return RedirectToAction("Index", "KSK_DinhKy");
                        }
                    }
                }
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = $"<script>alert('Thêm mới thất bại: {e.Message}');</script>";
            }
            return RedirectToAction("Index", "KSK_DinhKy");
        }




        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";

                return RedirectToAction("Index", "KSK_ChuyenViTri");
            }

            var res = await (from a in _context.KSK_DinhKy.Where(x=>x.ID_KSK_DK == id)
                             join nv in _context.NhanVien on a.ID_NV equals nv.ID_NV
                             select new KSK_DinhKy
                             {
                                 ID_KSK_DK = a.ID_KSK_DK,
                                 ID_NV = a.ID_NV,
                                 NgaySinh = nv.NgaySinh,
                                 ID_ViTri = (int)a.ID_ViTri,
                                 ID_GioiTinh = (int)a.ID_GioiTinh,
                                 KhamTongQuat = a.KhamTongQuat,
                                 KhamPhuKhoa = a.KhamPhuKhoa,
                                 ID_NhomMau = (int?)a.ID_NhomMau ?? default,
                                 NhomMauRh = a.NhomMauRh,
                                 CongThucMau = a.CongThucMau,
                                 NuocTieu = a.NuocTieu,
                                 ID_PhanLoaiKSK = a.ID_PhanLoaiKSK,
                                 KetLuanKSK = a.KetLuanKSK,
                                 NgayKSK = (DateTime)a.NgayKSK

                             }).ToListAsync();

            KSK_DinhKy DO = new KSK_DinhKy();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID_KSK_DK = a.ID_KSK_DK;
                    DO.ID_NV = a.ID_NV;
                    DO.NgaySinh = a.NgaySinh;
                    DO.ID_ViTri = (int)a.ID_ViTri;
                    DO.ID_GioiTinh = (int)a.ID_GioiTinh;
                    DO.KhamTongQuat = a.KhamTongQuat;
                    DO.KhamPhuKhoa = a.KhamPhuKhoa;
                    DO.ID_NhomMau = (int?)a.ID_NhomMau ?? default;
                    DO.NhomMauRh = a.NhomMauRh;
                    DO.CongThucMau = a.CongThucMau;
                    DO.NuocTieu = a.NuocTieu;
                    DO.ID_PhanLoaiKSK = a.ID_PhanLoaiKSK;
                    DO.KetLuanKSK = a.KetLuanKSK;
                    DO.NgayKSK = (DateTime)a.NgayKSK;
                }

                var NhanVien = (from nv in _context.NhanVien
                                select new NhanVien
                                {
                                    ID_NV = (int)nv.ID_NV,
                                    HoTen = nv.MaNV + " : " + nv.HoTen
                                }).ToList();
                ViewBag.NVList = new SelectList(NhanVien, "ID_NV", "HoTen", DO.ID_NV);

                List<ViTriLamViec> vt = _context.ViTriLamViec.ToList();
                ViewBag.VTList = new SelectList(vt, "ID_ViTri", "TenViTri", DO.ID_ViTri);

                List<NhomMau> nm = _context.NhomMau.ToList();
                ViewBag.NMList = new SelectList(nm, "ID_NhomMau", "TenNhomMau", DO.ID_NhomMau);

                List<PhanLoaiKSK> l = _context.PhanLoaiKSK.ToList();
                ViewBag.LList = new SelectList(l, "ID_PhanLoaiKSK", "TenLoaiKSK", DO.ID_PhanLoaiKSK);

                List<GioiTinh> gt = _context.GioiTinh.ToList();
                ViewBag.GTList = new SelectList(gt, "ID_GioiTinh", "TenGioiTinh", DO.ID_GioiTinh);

                DateTime NK = (DateTime)DO.NgayKSK;
                ViewBag.NgayKSK = NK.ToString("yyyy-MM-dd");

                DateTime NS = (DateTime)DO.NgaySinh;
                ViewBag.NgaySinh = NS.ToString("yyyy-MM-dd");

            }
            else
            {
                return NotFound();
            }



            return PartialView(DO);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KSK_DinhKy _DO)
        {
            try
            {
                SqlParameter[] param = {
                new SqlParameter("@ID_KSK_DK", SqlDbType.Int) { Value = _DO.ID_KSK_DK },
                new SqlParameter("@ID_NV", SqlDbType.Int) { Value = _DO.ID_NV },
                new SqlParameter("@ID_ViTri", SqlDbType.Int) { Value =(int?) _DO.ID_ViTri ?? (object)DBNull.Value}, // Cho phép NULL
                new SqlParameter("@ID_GioiTinh", SqlDbType.Int) { Value = _DO.ID_GioiTinh ??(object) DBNull.Value },
                new SqlParameter("@KhamTongQuat", SqlDbType.NVarChar) { Value = _DO.KhamTongQuat ??(object) DBNull.Value },
                new SqlParameter("@KhamPhuKhoa", SqlDbType.NVarChar) { Value = _DO.KhamPhuKhoa ??(object) DBNull.Value },
                new SqlParameter("@ID_NhomMau", SqlDbType.Int) { Value = _DO.ID_NhomMau ??(object) DBNull.Value },
                new SqlParameter("@NhomMauRh", SqlDbType.NVarChar, 50) { Value = _DO.NhomMauRh ??(object) DBNull.Value },
                new SqlParameter("@CongThucMau", SqlDbType.NVarChar) { Value = _DO.CongThucMau ??(object) DBNull.Value },
                new SqlParameter("@NuocTieu", SqlDbType.NVarChar) { Value = _DO.NuocTieu ??(object) DBNull.Value },
                new SqlParameter("@ID_PhanLoaiKSK", SqlDbType.Int) { Value =(int?) _DO.ID_PhanLoaiKSK ?? (object)DBNull.Value },
                new SqlParameter("@KetLuanKSK", SqlDbType.NVarChar) { Value = _DO.KetLuanKSK ??(object) DBNull.Value },
                new SqlParameter("@NgayKSK", SqlDbType.Date) { Value = _DO.NgayKSK }
            };
                var result = _context.Database.ExecuteSqlRaw("EXEC KSK_DinhKy_update @ID_KSK_DK,@ID_NV,@ID_ViTri,@ID_GioiTinh,@KhamTongQuat,@KhamPhuKhoa,@ID_NhomMau,@NhomMauRh,@CongThucMau,@NuocTieu,@ID_PhanLoaiKSK,@KetLuanKSK,@NgayKSK",param);
                                                                                                
                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";     
            }                                                                                   
            catch (Exception e)                                                                 
            {                                                                                   
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";         
            }
            var a = TempData["begin"];
            var b = TempData["end"];
            var c = TempData["id"];
            var d = TempData["Pager"];


            return RedirectToAction("Index", "KSK_DinhKy", new { begind = TempData["begin"] ,end= TempData["end"] , IDPhongBan = TempData["id"], page = TempData["Pager"] });
         
        }                                                                                       
        public async Task<IActionResult> Delete(int id, int page)
        {
            try
            {

                var result = await _context.Database.ExecuteSqlRawAsync("EXEC KSK_DinhKy_delete {0}", id);

                TempData["msgSuccess"] = "<script>alert('Xóa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xóa dữ liệu thất bại');</script>";
            }


            return RedirectToAction("Index", "KSK_DinhKy");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCheck([FromBody] int[] id)
        {
            try
            {
                if (id.Count() >= 1)
                {
                    for (int i = 0; i < id.Count(); i++)
                    {
                        var result = await _context.Database.ExecuteSqlRawAsync("EXEC KSK_DinhKy_delete {0}", id[i]);
                    }
                    return Ok(new { status = 1, msg = "Xóa thành công" });

                }
                else
                {
                    return Ok(new { status = 0, msg = "Xóa không thành công" });

                }

            }
            catch (Exception e)
            {
                return Ok(new { status = 1, msg = "Xóa dữ liệu thất bại " });

            }

        }
    }
}
