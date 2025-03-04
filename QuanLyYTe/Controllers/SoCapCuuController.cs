using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyYTe.Models;
using QuanLyYTe.Repositorys;
using Microsoft.AspNetCore.Mvc.Rendering;
using ExcelDataReader;
using System.Data;
using Microsoft.AspNetCore.StaticFiles;
using DocumentFormat.OpenXml.VariantTypes;
using Microsoft.Data.SqlClient;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Hosting;

namespace QuanLyYTe.Controllers
{
    public class SoCapCuuController : Controller
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public SoCapCuuController(DataContext _context, IWebHostEnvironment webHostEnvironment)
        {
            this._context = _context;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index(string search, int page = 1)
        {
            var res = await (from a in _context.SoCapCuu
                             join nv in _context.NhanVien on a.ID_NV equals nv.ID_NV
                             join pb in _context.PhongBan on nv.ID_PhongBan equals pb.ID_PhongBan
                             join gt in _context.GioiTinh on a.ID_GioiTinh equals gt.ID_GioiTinh
                             join tn in _context.NhomTaiNan on a.TaiNan equals tn.ID_NhomTaiNan into ulist1
                             from tn in ulist1.DefaultIfEmpty()
                             join bl in _context.NhomBenhLy on a.BenhLy equals bl.ID_BenhLy into ulist2
                             from bl in ulist2.DefaultIfEmpty()
                             select new SoCapCuu
                             {
                                 ID_SCC = a.ID_SCC,
                                 NgayThangNam = a.NgayThangNam,
                                 ID_NV = a.ID_NV,
                                 MaNV = nv.MaNV,
                                 HoTen = nv.HoTen,
                                 NgaySinh = (DateTime?)nv.NgaySinh ?? default,
                                 TenPhongBan = pb.TenPhongBan,
                                 ID_GioiTinh = a.ID_GioiTinh,
                                 TenGioiTinh = gt.TenGioiTinh,
                                 ThoiGianTiepNhan = a.ThoiGianTiepNhan,
                                 ThoiGianCapCuu = a.ThoiGianCapCuu,
                                 TaiNan = (int?)a.TaiNan ?? default,
                                 TenTaiNan = tn.TenNhomTaiNan,
                                 BenhLy = (int?)a.BenhLy ?? default,
                                 TenBenhLy = bl.TenBenhLy,
                                 DienBien = a.DienBien,
                                 PhanLoaiNT = a.PhanLoaiNT,
                                 YeuToGayTaiNan = a.YeuToGayTaiNan,
                                 XuLyCapCuu = a.XuLyCapCuu,
                                 ThoiGianNghiViec = a.ThoiGianNghiViec,
                                 KetQuaGiamDinh = a.KetQuaGiamDinh,
                                 SoDienThoai = a.SoDienThoai,
                                 BienBan24h = a.BienBan24h,
                                 TongChiPhi = a.TongChiPhi,
                                 KhongCanKT_SK = a.KhongCanKT_SK,
                                 KetQuaKT_SK = a.KetQuaKT_SK,
                                 GhiChu = a.GhiChu
                             }).ToListAsync();
            if (search != null)
            {
                res = res.Where(x => x.HoTen.ToLower().Contains(search.ToLower()) || x.MaNV.ToLower().Contains(search.ToLower())).ToList();
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
            this.ViewBag.search = search;
            var ct_nd = _context.TuyenBenhVien.ToList();
            ViewData["TuyenBenhVien"] = ct_nd;
            return View(data);

        }
        public async Task<IActionResult> Deatail(int? ID_NV, int page = 1)
        {
            var res = await (from a in _context.SoCapCuu.Where(x=>x.ID_NV == ID_NV)
                             join nv in _context.NhanVien on a.ID_NV equals nv.ID_NV
                             join pb in _context.PhongBan on nv.ID_PhongBan equals pb.ID_PhongBan
                             join gt in _context.GioiTinh on a.ID_GioiTinh equals gt.ID_GioiTinh
                             join tn in _context.NhomTaiNan on a.TaiNan equals tn.ID_NhomTaiNan into ulist1
                             from tn in ulist1.DefaultIfEmpty()
                             join bl in _context.NhomBenhLy on a.BenhLy equals bl.ID_BenhLy into ulist2
                             from bl in ulist2.DefaultIfEmpty()
                             select new SoCapCuu
                             {
                                 ID_SCC = a.ID_SCC,
                                 NgayThangNam = a.NgayThangNam,
                                 ID_NV = a.ID_NV,
                                 MaNV = nv.MaNV,
                                 HoTen = nv.HoTen,
                                 NgaySinh = (DateTime?)nv.NgaySinh ?? default,
                                 TenPhongBan = pb.TenPhongBan,
                                 ID_GioiTinh = a.ID_GioiTinh,
                                 TenGioiTinh = gt.TenGioiTinh,
                                 ThoiGianTiepNhan = a.ThoiGianTiepNhan,
                                 ThoiGianCapCuu = a.ThoiGianCapCuu,
                                 TaiNan = (int?)a.TaiNan ?? default,
                                 TenTaiNan = tn.TenNhomTaiNan,
                                 BenhLy = (int?)a.BenhLy ?? default,
                                 TenBenhLy = bl.TenBenhLy,
                                 DienBien = a.DienBien,
                                 PhanLoaiNT = a.PhanLoaiNT,
                                 YeuToGayTaiNan = a.YeuToGayTaiNan,
                                 XuLyCapCuu = a.XuLyCapCuu,
                                 ThoiGianNghiViec = a.ThoiGianNghiViec,
                                 KetQuaGiamDinh = a.KetQuaGiamDinh,
                                 SoDienThoai = a.SoDienThoai,
                                 BienBan24h = a.BienBan24h,
                                 TenBenhVien = a.TenBenhVien,
                                 YTePhuTrach = a.YTePhuTrach,
                                 ThoiGianDiChuyenVien = a.ThoiGianDiChuyenVien,
                                 TamUng = a.TamUng,
                                 ThanhToan = a.ThanhToan,
                                 ChungTu = a.ChungTu,
                                 ThoiGianDieuTri = a.ThoiGianDieuTri,
                                 BVTuyenHai = a.BVTuyenHai,
                                 TongChiPhi = a.TongChiPhi,
                                 KhongCanKT_SK = a.KhongCanKT_SK,
                                 KetQuaKT_SK = a.KetQuaKT_SK,
                                 GhiChu = a.GhiChu
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
            var ct_nd = _context.TuyenBenhVien.ToList();
            ViewData["TuyenBenhVien"] = ct_nd;
            return View(data);

        }





        public async Task<IActionResult> Delete(int id, int page)
        {
            try
            {

                var result = _context.Database.ExecuteSqlRaw("EXEC SoCapCuu_delete {0}", id);

                TempData["msgSuccess"] = "<script>alert('Xóa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xóa dữ liệu thất bại');</script>";
            }


            return RedirectToAction("Index", "SoCapCuu", new { page = page });
        }
        [HttpPost]
        public async Task<IActionResult> DeleteCheck([FromBody]int[] id)
        {
            try
            {
                if (id.Count() >= 1)
                {
                    for(int i = 0; i < id.Count(); i++)
                    {
                    var result = await _context.Database.ExecuteSqlRawAsync("EXEC SoCapCuu_delete {0}", id[i]);
                    }
                    return Ok(new { status = 1,msg= "Xóa thành công" });
               
                }
                else
                {
                    return Ok(new { status = 0, msg = "Xóa không thành công" });

                }

            }
            catch (Exception e)
            {
                return Ok(new { status = 1, msg = "Xóa dữ liệu thất bại "});
              
            }

        }
        public FileResult TestDownloadPCF()
        {
            string path = "Form files/BM_SoCapCuu.xlsx";
            HttpContext.Response.ContentType = "application/xlsx";
            string filePath = Path.Combine(_webHostEnvironment.ContentRootPath, path);

            if (!System.IO.File.Exists(filePath))
            {
                return null; // Xử lý lỗi nếu file không tồn tại
            }
            List<NhomTaiNan> loaiTNLD = _context.NhomTaiNan.ToList();
            List<NhomBenhLy> loaiBL = _context.NhomBenhLy.ToList();

            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheet(2);
                for (var i = 0; i < loaiTNLD.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = loaiTNLD[i].TenNhomTaiNan;
                }
                for (var i = 0; i < loaiBL.Count; i++)
                {
                    worksheet.Cell(i + 2, 3).Value = loaiBL[i].TenBenhLy;
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
                    return RedirectToAction("Index", "SoCapCuu");
                }
                string webRootPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string dirPath = Path.Combine(webRootPath, "ReceivedReports");
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                // MAke sure that only Excel file is used 
                string dataFileName = Path.GetFileName(DateTime.Now.ToString("yyyyMMddHHmm"));

                string extension = Path.GetExtension(dataFileName);

                string[] allowedExtsnions = new string[] { ".xls", ".xlsx" };
                // Make a Copy of the Posted File from the Received HTTP Request
                string saveToPath = Path.Combine(dirPath, dataFileName);

                using (FileStream stream = new FileStream(saveToPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                // read the excel file
                IExcelDataReader reader = null;
                using (var stream = new FileStream(saveToPath, FileMode.Open))
                {
                    if (extension == ".xlsx")
                        reader = ExcelReaderFactory.CreateBinaryReader(stream);
                    else
                        reader = ExcelReaderFactory.CreateOpenXmlReader(stream);

                    DataSet ds = new DataSet();
                    ds = reader.AsDataSet();
                    reader.Close();
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        System.Data.DataTable serviceDetails = ds.Tables[0];
                        for (int i = 9; i < serviceDetails.Rows.Count; i++)
                        {
                            string Ngay = serviceDetails.Rows[i][1].ToString();
                            DateTime NgayThangNam = DateTime.ParseExact(Ngay, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                            string MaNV = serviceDetails.Rows[i][2].ToString().Trim();
                            var check_nv = _context.NhanVien.Where(x => x.MaNV == MaNV).FirstOrDefault();
                            if (check_nv == null)
                            {
                                TempData["msgSuccess"] = "<script>alert('Vui lòng cập nhật dữ liệu nhân viên: " + MaNV + "');</script>";

                                return RedirectToAction("Index", "SoCapCuu");
                            }
                            int gt = (serviceDetails.Rows[i][4].ToString().Trim().ToLower().Equals("nam")) ? 1 : 2;
                            string ThoiGianTiepNhan = serviceDetails.Rows[i][5].ToString();

                            string ThoiGianCapCuu = serviceDetails.Rows[i][6].ToString();

                            string TaiNan = serviceDetails.Rows[i][7].ToString().Trim();
                            var check_tn = _context.NhomTaiNan.Where(x=>x.TenNhomTaiNan == TaiNan).FirstOrDefault();
                            if(check_tn == null && TaiNan != "")
                            {
                                TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra nhóm tai nạn: " + MaNV + "');</script>";
                                return RedirectToAction("Index", "SoCapCuu");
                            }
                            string BenhLy = serviceDetails.Rows[i][8].ToString().Trim();
                            var check_bl = _context.NhomBenhLy.Where(x=>x.TenBenhLy == BenhLy).FirstOrDefault();
                            if(check_bl == null && BenhLy != "")
                            {
                                TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra nhóm bệnh lý: " + MaNV + "');</script>";
                                return RedirectToAction("Index", "SoCapCuu");
                            }

                            string DienBien = serviceDetails.Rows[i][9].ToString().Trim();

                            string PhanLoaiNT = serviceDetails.Rows[i][10].ToString().Trim();

                            string YeuToGayTaiNan = serviceDetails.Rows[i][11].ToString().Trim();

                            string XuLyCapCuu = serviceDetails.Rows[i][12].ToString().Trim();

                            string ThoiGian = serviceDetails.Rows[i][13].ToString().Trim();

                            int ThoiGianNghiViec = Convert.ToInt32(ThoiGian);

                            string KetQuaGiamDinh = serviceDetails.Rows[i][14].ToString().Trim();

                            string SoDienThoai = serviceDetails.Rows[i][15].ToString().Trim();

                            string BienBan24h = serviceDetails.Rows[i][16].ToString().Trim();

                            string TongChiPhi = serviceDetails.Rows[i][17].ToString().Trim();

                            string KhongCanKT_SK = serviceDetails.Rows[i][18].ToString().Trim();

                            string KetQuaKT_SK = serviceDetails.Rows[i][19].ToString().Trim();

                            string GhiChu = serviceDetails.Rows[i][20].ToString().Trim();

                            if(check_tn == null)
                            {
                                var result = _context.Database.ExecuteSqlRaw("EXEC SoCapCuu_insert {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18}",
                                                                     NgayThangNam, check_nv.ID_NV,gt, ThoiGianTiepNhan, ThoiGianCapCuu, null, check_bl.ID_BenhLy, DienBien, PhanLoaiNT, YeuToGayTaiNan, XuLyCapCuu,
                                                                     ThoiGianNghiViec, KetQuaGiamDinh, SoDienThoai, BienBan24h,TongChiPhi, KhongCanKT_SK, KetQuaKT_SK, GhiChu);
                            }    
                            else if(check_bl == null)
                            {
                                var result = _context.Database.ExecuteSqlRaw("EXEC SoCapCuu_insert {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18}",
                                                                     NgayThangNam, check_nv.ID_NV, gt,  ThoiGianTiepNhan, ThoiGianCapCuu, check_tn.ID_NhomTaiNan, null, DienBien, PhanLoaiNT, YeuToGayTaiNan, XuLyCapCuu,
                                                                     ThoiGianNghiViec, KetQuaGiamDinh, SoDienThoai, BienBan24h,TongChiPhi, KhongCanKT_SK, KetQuaKT_SK, GhiChu);
                            } 
                            else if(check_tn != null && check_tn != null)
                            {
                                var result = _context.Database.ExecuteSqlRaw("EXEC SoCapCuu_insert {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18}",
                                                                     NgayThangNam, check_nv.ID_NV, gt, ThoiGianTiepNhan, ThoiGianCapCuu, check_tn.ID_NhomTaiNan, check_bl.ID_BenhLy, DienBien, PhanLoaiNT, YeuToGayTaiNan, XuLyCapCuu,
                                                                     ThoiGianNghiViec, KetQuaGiamDinh, SoDienThoai, BienBan24h, TongChiPhi, KhongCanKT_SK, KetQuaKT_SK, GhiChu);
                            }    
                         
                        }
                    }
                }
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Thêm mới thất bại');</script>";
            }

            return RedirectToAction("Index", "SoCapCuu");
        }

        public async Task<IActionResult> edit(int id)
        {
            SoCapCuu res;
            try
            {
                res = await (from a in _context.SoCapCuu.Where(x => x.ID_SCC == id)
                             join nv in _context.NhanVien on a.ID_NV equals nv.ID_NV
                             join pb in _context.PhongBan on nv.ID_PhongBan equals pb.ID_PhongBan
                             join gt in _context.GioiTinh on a.ID_GioiTinh equals gt.ID_GioiTinh
                             join tn in _context.NhomTaiNan on a.TaiNan equals tn.ID_NhomTaiNan into ulist1
                             from tn in ulist1.DefaultIfEmpty()
                             join bl in _context.NhomBenhLy on a.BenhLy equals bl.ID_BenhLy into ulist2
                             from bl in ulist2.DefaultIfEmpty()
                             select new SoCapCuu
                             {
                                 ID_SCC = a.ID_SCC,
                                 NgayThangNam = a.NgayThangNam,
                                 ID_NV = a.ID_NV,
                                 MaNV = nv.MaNV,
                                 HoTen = nv.HoTen,
                                 NgaySinh = (DateTime?)nv.NgaySinh ?? default,
                                 TenPhongBan = pb.TenPhongBan,
                                 ID_GioiTinh = a.ID_GioiTinh,
                                 TenGioiTinh = gt.TenGioiTinh,
                                 ThoiGianTiepNhan = a.ThoiGianTiepNhan,
                                 ThoiGianCapCuu = a.ThoiGianCapCuu,
                                 TaiNan = (int?)a.TaiNan ?? default,
                                 TenTaiNan = tn.TenNhomTaiNan,
                                 BenhLy = (int?)a.BenhLy ?? default,
                                 TenBenhLy = bl.TenBenhLy,
                                 DienBien = a.DienBien,
                                 PhanLoaiNT = a.PhanLoaiNT,
                                 YeuToGayTaiNan = a.YeuToGayTaiNan,
                                 XuLyCapCuu = a.XuLyCapCuu,
                                 ThoiGianNghiViec = a.ThoiGianNghiViec,
                                 KetQuaGiamDinh = a.KetQuaGiamDinh,
                                 SoDienThoai = a.SoDienThoai,
                                 BienBan24h = a.BienBan24h,
                                 TongChiPhi = a.TongChiPhi,
                                 KhongCanKT_SK = a.KhongCanKT_SK,
                                 KetQuaKT_SK = a.KetQuaKT_SK,
                                 GhiChu = a.GhiChu,
                                
                             }).FirstOrDefaultAsync();
                var nv1 = await _context.NhanVien.Select(x => new { id = x.ID_NV, nhanvien = $"{x.MaNV} - {x.HoTen}" }).ToListAsync();
                ViewBag.nv = new SelectList(nv1, "id", "nhanvien", res?.ID_NV);
                var gt1 = await _context.GioiTinh.ToListAsync();
                ViewBag.gt = new SelectList(gt1, "ID_GioiTinh", "TenGioiTinh", res?.ID_GioiTinh);
                var bl1 = await _context.NhomBenhLy.ToListAsync();
                ViewBag.bl = new SelectList(bl1, "ID_BenhLy", "TenBenhLy", res?.BenhLy);
                var tn1 = await _context.NhomTaiNan.ToListAsync();
                ViewBag.tn = new SelectList(tn1, "ID_NhomTaiNan", "TenNhomTaiNan", res?.TaiNan);

                if (res != null)
                {
                    return PartialView(res);
                }
                TempData["msgError"] = "<script>alert('không tìm thấy dòng dữ liệu cần xóa');</script>";

            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xóa dữ liệu thất bại');</script>";

            }
            return RedirectToAction("Index", "SoCapCuu");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SoCapCuu _DO)
        {

            try
            {
                string a= (_DO.KhongCanKT_SK.ToLower().Equals("false")) ?(_DO.KetQuaKT_SK.ToLower().Equals("false")? "Không" : "Đạt") :"";
                SqlParameter[] param =
                {
                    new SqlParameter("@id", SqlDbType.Int) { Value = _DO.ID_SCC },
                    new SqlParameter("@NgayThangNam", SqlDbType.Date) { Value = _DO.NgayThangNam ?? (object)DBNull.Value },
                    new SqlParameter("@ID_NV", SqlDbType.Int) { Value = _DO.ID_NV },
                    new SqlParameter("@ID_GioiTinh", SqlDbType.Int) { Value = _DO.ID_GioiTinh },
                    new SqlParameter("@ThoiGianTiepNhan", SqlDbType.NVarChar, 50) { Value = _DO.ThoiGianTiepNhan ?? (object)DBNull.Value },
                    new SqlParameter("@ThoiGianCapCuu", SqlDbType.NVarChar, 50) { Value = _DO.ThoiGianCapCuu ?? (object)DBNull.Value },
                    new SqlParameter("@TaiNan", SqlDbType.Int) { Value = _DO.TaiNan != null ? _DO.TaiNan : (object)DBNull.Value },
                    new SqlParameter("@BenhLy", SqlDbType.Int) { Value = _DO.BenhLy != null ? _DO.BenhLy : (object)DBNull.Value },
                    new SqlParameter("@DienBien", SqlDbType.NVarChar, -1) { Value = _DO.DienBien ?? (object)DBNull.Value },
                    new SqlParameter("@PhanLoaiNT", SqlDbType.NVarChar, 50) { Value = _DO.PhanLoaiNT ?? (object)DBNull.Value },
                    new SqlParameter("@YeuToGayTaiNan", SqlDbType.NVarChar, -1) { Value = _DO.YeuToGayTaiNan ?? (object)DBNull.Value },
                    new SqlParameter("@XuLyCapCuu", SqlDbType.NVarChar, -1) { Value = _DO.XuLyCapCuu ?? (object)DBNull.Value },
                    new SqlParameter("@ThoiGianNghiViec", SqlDbType.Int) { Value = _DO.ThoiGianNghiViec != null ? _DO.ThoiGianNghiViec : (object)DBNull.Value },
                    new SqlParameter("@KetQuaGiamDinh", SqlDbType.NVarChar, 50) { Value = _DO.KetQuaGiamDinh ?? (object)DBNull.Value },
                    new SqlParameter("@SoDienThoai", SqlDbType.NVarChar, 50) { Value = _DO.SoDienThoai ?? (object)DBNull.Value },
                    new SqlParameter("@BienBan24h", SqlDbType.NVarChar, 50) { Value = (_DO.BienBan24h == "true") ? "X" : "" },
                    new SqlParameter("@TongChiPhi", SqlDbType.NVarChar, 50) { Value = _DO.TongChiPhi ?? (object)DBNull.Value },
                    new SqlParameter("@KhongCanKT_SK", SqlDbType.NVarChar, 50) { Value = (_DO.KhongCanKT_SK == "true") ? "X" : "" },
                    new SqlParameter("@KetQuaKT_SK", SqlDbType.NVarChar, 50) { Value =a },
                    new SqlParameter("@GhiChu", SqlDbType.NVarChar, -1) { Value = _DO.GhiChu ?? (object)DBNull.Value }
                };

                var result = _context.Database.ExecuteSqlRaw(
                    "EXEC SoCapCuu_update @id, @NgayThangNam, @ID_NV, @ID_GioiTinh, @ThoiGianTiepNhan, @ThoiGianCapCuu, " +
                    "@TaiNan, @BenhLy, @DienBien, @PhanLoaiNT, @YeuToGayTaiNan, @XuLyCapCuu, @ThoiGianNghiViec, " +
                    "@KetQuaGiamDinh, @SoDienThoai, @BienBan24h, @TongChiPhi, @KhongCanKT_SK, @KetQuaKT_SK, @GhiChu",
                    param
                );



                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";
            }


            return RedirectToAction("Index", "SoCapCuu");
        }
    }
}
