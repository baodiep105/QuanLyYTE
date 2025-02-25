using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyYTe.Models;
using QuanLyYTe.Repositorys;
using Microsoft.AspNetCore.Mvc.Rendering;
using ExcelDataReader;
using System.Data;
using System.Security.Claims;
using Microsoft.Kiota.Abstractions;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Hosting;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.VariantTypes;
using Microsoft.Office.Interop.Excel;

namespace QuanLyYTe.Controllers
{
    public class TrinhKyController : Controller
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public TrinhKyController(DataContext _context, IWebHostEnvironment webHostEnvironment)
        {
            this._context = _context;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index(string search, int page = 1)
        {   var MNV = User.FindFirstValue(ClaimTypes.Name);
            var check = _context.TaiKhoan.Where(x => x.TenDangNhap == MNV).FirstOrDefault();
            var res = await (from a in _context.TrinhKy
                             join nv in _context.NhanVien on a.NguoiLap equals nv.ID_NV
                             join bp in _context.PhongBan on a.ID_PhongBan equals bp.ID_PhongBan
                             select new TrinhKy
                             {
                                 ID_TK = a.ID_TK,
                                 ID_PhongBan = (int)a.ID_PhongBan,
                                 TenPhongBan = bp.TenPhongBan,
                                 NoiDung = a.NoiDung,
                                 NgayTrinhKy = (DateTime?)a.NgayTrinhKy ?? default,
                                 FilePath = a.FilePath,
                                 NguoiLap = (int?)a.NguoiLap??default,
                                 HoTen_NguoiLap = nv.HoTen,
                                 TinhTrang_NguoiLap = (int?)a.TinhTrang_NguoiLap?? default,
                                 Ngay_NguoiLap = (DateTime?)a.Ngay_NguoiLap?? default,
                                 TruongPho = (int?)a.TruongPho ?? default,
                                 TinhTrang_TruongPho = (int?)a.TinhTrang_TruongPho?? default,
                                 Ngay_TruongPho = (DateTime?)a.Ngay_TruongPho??default,
                                 GhiChu = a.GhiChu

                             }).ToListAsync();
            if (check.ID_Quyen != 1 && check.ID_Quyen != 2)
            {
                res = res.Where(x => x.ID_PhongBan == check.ID_PhongBan).ToList();
            }
            if (search != null)
            {
                res = res.Where(x => x.TenPhongBan.Contains(search) || x.NoiDung.Contains(search)).ToList();
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
            string filePath = "Form files/BM_KSK_BenhNgheNghiep.xlsx";
            HttpContext.Response.ContentType = "application/xlsx";
            FileContentResult result = new FileContentResult
            (System.IO.File.ReadAllBytes(filePath), "application/xlsx")
            {
                FileDownloadName = "BM_KSK_BenhNgheNghiep.xlsx"
            };
            return result;
        }
        public async Task<IActionResult> Create()
        {
            List<PhongBan> pb = _context.PhongBan.ToList();
            ViewBag.PBList = new SelectList(pb, "ID_PhongBan", "TenPhongBan");

            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile file, TrinhKy _DO)
        {
            try
            {
                var Ma_NV = User.FindFirstValue(ClaimTypes.Name);
                var check_nv_kt = _context.NhanVien.Where(x=>x.MaNV == Ma_NV).FirstOrDefault();
                int ID_ky = 0;
                if (check_nv_kt != null)
                {
                    var idOutputParam = new SqlParameter
                    {
                        ParameterName = "@ID_OUT",
                        SqlDbType = SqlDbType.Int,
                        Direction = ParameterDirection.Output
                    };

                    SqlParameter[] sqlParameters = new SqlParameter[]
                    {
                        new SqlParameter("@ID_PhongBan", SqlDbType.Int) { Value = _DO.ID_PhongBan },
                        new SqlParameter("@NoiDung", SqlDbType.NVarChar) { Value = (object)_DO.NoiDung ?? DBNull.Value },
                        new SqlParameter("@NgayTrinhKy", SqlDbType.Date) { Value = _DO.NgayTrinhKy.HasValue ? _DO.NgayTrinhKy.Value : DBNull.Value },
                        new SqlParameter("@FilePath", SqlDbType.NVarChar) { Value = DBNull.Value },
                        new SqlParameter("@NguoiLap", SqlDbType.Int) { Value = check_nv_kt.ID_NV },
                        new SqlParameter("@TinhTrang_NguoiLap", SqlDbType.Int) { Value = 0 },
                        idOutputParam // OUTPUT Parameter
                    };

                    // Gọi stored procedure
                    _context.Database.ExecuteSqlRaw(
                        "EXEC TrinhKy_insert1 @ID_PhongBan, @NoiDung, @NgayTrinhKy, @FilePath, @NguoiLap, @TinhTrang_NguoiLap, @ID_OUT OUTPUT",
                        sqlParameters
                    );
                    ID_ky = (int)idOutputParam.Value;
                   
                }
                if (file == null || file.Length == 0)
                {
                    return RedirectToAction("Index", "ThoiHan_KSK_BNN");
                }


                // Create the Directory if it is not exist
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

                // USe this to handle Encodeing differences in .NET Core
              /*  System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                read the excel file*/
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

                        for (int i = 5; i < serviceDetails.Rows.Count; i++)
                        {
                            string MNV = serviceDetails.Rows[i][1].ToString().Trim();
                            var check_nv = _context.NhanVien.Where(x => x.MaNV == MNV).FirstOrDefault();
                            if (check_nv == null)
                            {
                                var Delete_BNN = _context.KSK_BenhNgheNghiep.Where(x => x.ID_TK == ID_ky).ToList();
                                foreach (var y in Delete_BNN)
                                {
                                    var Delete_ = _context.Database.ExecuteSqlRaw("EXEC KSK_BenhNgheNghiep_delete {0}", y.ID_KSK_BNN);
                                }
                                var Delete = _context.TrinhKy.Where(x => x.ID_TK == ID_ky).ToList();
                                var Delete_TK = _context.Database.ExecuteSqlRaw("EXEC TrinhKy_delete {0}", ID_ky);
                                TempData["msgSuccess"] = "<script>alert('Vui lòng cập nhật dữ liệu nhân viên: " + MNV + "');</script>";
                                return RedirectToAction("Index", "KSK_ChuyenViTri");
                            }

                            string ViTriLaoDong = serviceDetails.Rows[i][3].ToString().Trim();
                            var check_vt = _context.ViTriLaoDong.Where(x => x.TenViTriLaoDong.Trim().Contains(ViTriLaoDong.Trim())).FirstOrDefault();
                            if (check_vt == null)
                            {
                                var Delete_BNN = _context.KSK_BenhNgheNghiep.Where(x => x.ID_TK == ID_ky).ToList();
                                foreach (var y in Delete_BNN)
                                {
                                    var Delete_ = _context.Database.ExecuteSqlRaw("EXEC KSK_BenhNgheNghiep_delete {0}", y.ID_KSK_BNN);
                                }
                                var Delete = _context.TrinhKy.Where(x => x.ID_TK == ID_ky).ToList();
                                var Delete_TK = _context.Database.ExecuteSqlRaw("EXEC TrinhKy_delete {0}", ID_ky);

                                TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra vị trí lao động: " + ViTriLaoDong + "');</script>";
                                return RedirectToAction("Index", "KSK_ChuyenViTri");
                            }


                            string GhiChu = serviceDetails.Rows[i][4].ToString().Trim();

                            var Output_ID_KSK_BNN = new SqlParameter
                            {
                                ParameterName = "ID_KSK_BNN",
                                SqlDbType = System.Data.SqlDbType.Int,
                                Direction = System.Data.ParameterDirection.Output,
                            };
                            SqlParameter[] sqlParameters = new SqlParameter[]
                            {
                                new SqlParameter("@ID_NV", SqlDbType.Int) { Value = check_nv.ID_NV },
                                new SqlParameter("@ID_PhongBan", SqlDbType.Int) { Value = _DO.ID_PhongBan },
                                new SqlParameter("@ID_ViTriLaoDong", SqlDbType.Int) { Value = check_vt.ID_ViTriLaoDong },
                                new SqlParameter("@NgayLenDanhSach", SqlDbType.Date) { Value = _DO.NgayTrinhKy },
                                new SqlParameter("@XQuangTimPhoi", SqlDbType.NVarChar) { Value =  DBNull.Value },
                                new SqlParameter("@DoCNHoHap", SqlDbType.NVarChar) { Value = DBNull.Value },
                                new SqlParameter("@XQuangCSTLThangNghien", SqlDbType.NVarChar) { Value = DBNull.Value },
                                new SqlParameter("@DoThinhLuc", SqlDbType.NVarChar) { Value =  DBNull.Value },
                                new SqlParameter("@DoNhanAp", SqlDbType.NVarChar) { Value =  DBNull.Value },
                                new SqlParameter("@DinhLuongHbCo", SqlDbType.Float) { Value = DBNull.Value },
                                new SqlParameter("@DoDienTim", SqlDbType.NVarChar) { Value =DBNull.Value },
                                new SqlParameter("@ThoiGianMauChay", SqlDbType.Float) { Value =DBNull.Value },
                                new SqlParameter("@ThoiGianMauDong", SqlDbType.Float) { Value =DBNull.Value},
                                new SqlParameter("@TestHCV_HBsAg", SqlDbType.NVarChar) { Value = DBNull.Value },
                                new SqlParameter("@SGOT", SqlDbType.Float) { Value = DBNull.Value },
                                new SqlParameter("@SGPT", SqlDbType.Float) { Value = DBNull.Value },
                                new SqlParameter("@NuocTieu", SqlDbType.NVarChar) { Value = DBNull.Value },
                                new SqlParameter("@HIV", SqlDbType.NVarChar) { Value = DBNull.Value },
                                new SqlParameter("@DoPHda", SqlDbType.Float) { Value = DBNull.Value },
                                new SqlParameter("@DoLieuSinhHoc", SqlDbType.NVarChar) { Value = DBNull.Value },
                                new SqlParameter("@KetLuan", SqlDbType.NVarChar) { Value = DBNull.Value },
                                new SqlParameter("@GhiChu", SqlDbType.NVarChar) { Value = GhiChu },
                                new SqlParameter("@ID_PheDuyet", SqlDbType.Int) { Value = 0 },
                                new SqlParameter("@ID_TK", SqlDbType.Int) { Value = ID_ky },
                                Output_ID_KSK_BNN
                            };
                            var result = _context.Database.ExecuteSqlRaw("EXEC KSK_BenhNgheNghiep_insert {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23}, @ID_KSK_BNN OUTPUT",
                                                                          sqlParameters);
                            int ID_KSK_BNN = (int)Output_ID_KSK_BNN.Value;

                            /* var check_ct = _context.ChiTiet_ChiTieuNoiDung_ViTri.Where(x => x.ID_ViTriLaoDong == check_vt.ID_ViTriLaoDong).ToList();
                             foreach (var item in check_ct)
                             {
                                 var check_dh = _context.ChiTieuNoiDung.Where(x => x.ID_DocHai == item.ID_DocHai).ToList();
                                 foreach (var item1 in check_dh)
                                 {
                                     var result_ct = _context.Database.ExecuteSqlRaw("EXEC CT_KSK_BenhNgheNghiep_insert {0},{1},{2}",
                                                                          ID_KSK_BNN, item1.TenChiTieu, item1.TenNoiDung);
                                 }

                             }*/

                        }
                    }
                }

                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Thêm mới thất bại');</script>";
            }

            return RedirectToAction("Index", "TrinhKy");
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";

                return RedirectToAction("Index", "KSK_TuyenDung");
            }

            var res = await (from a in _context.TrinhKy.Where(x=>x.ID_TK == id)
                             join nv in _context.NhanVien on a.NguoiLap equals nv.ID_NV
                             join bp in _context.PhongBan on a.ID_PhongBan equals bp.ID_PhongBan
                             select new TrinhKy
                             {
                                 ID_TK = a.ID_TK,
                                 ID_PhongBan = (int)a.ID_PhongBan,
                                 TenPhongBan = bp.TenPhongBan,
                                 NoiDung = a.NoiDung,
                                 NgayTrinhKy = (DateTime?)a.NgayTrinhKy ?? default,
                                 FilePath = a.FilePath,
                                 NguoiLap = (int?)a.NguoiLap ?? default,
                                 HoTen_NguoiLap = nv.HoTen

                             }).ToListAsync();
            TrinhKy DO = new TrinhKy();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID_TK = a.ID_TK;
                    DO.ID_PhongBan = (int)a.ID_PhongBan;
                    DO.NoiDung = a.NoiDung;
                    DO.NgayTrinhKy = (DateTime?)a.NgayTrinhKy ?? default;
                    DO.FilePath = a.FilePath;
                }

                List<PhongBan> pb = _context.PhongBan.ToList();
                ViewBag.PBList = new SelectList(pb, "ID_PhongBan", "TenPhongBan", DO.ID_PhongBan);
                DateTime NK = (DateTime)DO.NgayTrinhKy;
                ViewBag.NgayTrinhKy = NK.ToString("yyyy-MM-dd");
            }
            else
            {
                return NotFound();
            }



            return PartialView(DO);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TrinhKy _DO)
        {

            try
            {

                var result = _context.Database.ExecuteSqlRaw("EXEC TrinhKy_update {0},{1},{2},{3},{4}", _DO.ID_TK, _DO.ID_PhongBan, _DO.NoiDung, _DO.NgayTrinhKy, null);

                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";
            }

            return RedirectToAction("Index", "TrinhKy");
        }
        public async Task<IActionResult> Delete(int id, int? page)
        {
            try
            {
               
                var Delete = _context.TrinhKy.Where(x => x.ID_TK == id).FirstOrDefault();

                var Delete_BNN = _context.KSK_BenhNgheNghiep.Where(x =>x.ID_TK==id).ToList();

                foreach (var y in Delete_BNN)
                {
                    var Delete_ = _context.Database.ExecuteSqlRaw("EXEC KSK_BenhNgheNghiep_delete {0}", y.ID_KSK_BNN);
                }

                var result = _context.Database.ExecuteSqlRaw("EXEC TrinhKy_delete {0}", id);

                TempData["msgSuccess"] = "<script>alert('Xóa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xóa dữ liệu thất bại');</script>";
            }


            return RedirectToAction("Index", "TrinhKy", new { page = page });
        }

        public async Task<IActionResult> CheckInformation(int? id)
        {
            if (id == null)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";

                return RedirectToAction("Index", "KSK_TuyenDung");
            }

            var res = await (from a in _context.TrinhKy.Where(x => x.ID_TK == id)
                             join nv in _context.NhanVien on a.NguoiLap equals nv.ID_NV
                             join bp in _context.PhongBan on a.ID_PhongBan equals bp.ID_PhongBan
                             select new TrinhKy
                             {
                                 ID_TK = a.ID_TK,
                                 ID_PhongBan = (int)a.ID_PhongBan,
                                 TenPhongBan = bp.TenPhongBan,
                                 NoiDung = a.NoiDung,
                                 NgayTrinhKy = (DateTime?)a.NgayTrinhKy ?? default,
                                 FilePath = a.FilePath,
                                 NguoiLap = (int?)a.NguoiLap ?? default,
                                 HoTen_NguoiLap = nv.HoTen

                             }).ToListAsync();
            TrinhKy DO = new TrinhKy();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID_TK = a.ID_TK;
                    DO.ID_PhongBan = (int)a.ID_PhongBan;
                    DO.NoiDung = a.NoiDung;
                    DO.NgayTrinhKy = (DateTime?)a.NgayTrinhKy ?? default;
                    DO.FilePath = a.FilePath;
                }

                //TP BP/NM
                var TPBP = (from au in _context.TaiKhoan.Where(x => x.ID_Quyen == 4)
                            join nv in _context.NhanVien on au.ID_NV equals nv.ID_NV
                            select new TaiKhoan
                            {
                                ID_NV = (int)au.ID_NV,
                                HoTen = nv.HoTen + " : " + nv.MaNV,
                            }).ToList();

                ViewBag.TPBPList = new SelectList(TPBP, "ID_NV", "HoTen");
            }
            else
            {
                return NotFound();
            }

            return PartialView(DO);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckInformation(TrinhKy _DO)
        {
            try
            {
                var check_tk = _context.TrinhKy.Where(x => x.ID_TK == _DO.ID_TK).FirstOrDefault();
                var result_ViTri = _context.Database.ExecuteSqlRaw("EXEC TrinhKy_update_trinhky {0},{1},{2},{3},{4}",
                                                    _DO.ID_TK,1,DateTime.Now,_DO.TruongPho, 0, null);

                TempData["msgSuccess"] = "<script>alert('Trình ký thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Trình ký thất bại');</script>";
            }

            return RedirectToAction("Index", "TrinhKy");
        }
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var check_tk = _context.TrinhKy.Where(x => x.ID_TK == id).FirstOrDefault();

                var result_ViTri = _context.Database.ExecuteSqlRaw("EXEC TrinhKy_update_trinhky {0},{1},{2},{3},{4}",
                                                      check_tk.ID_TK, 0, null, null, null, null);

                TempData["msgSuccess"] = "<script>alert('Hủy trình ký thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Hủy trình ký thất bại');</script>";
            }


            return RedirectToAction("Index", "TrinhKy");
        }

        public async Task<IActionResult> Processing(DateTime? begind, DateTime? endd, int page = 1)
        {

            DateTime Now = DateTime.Now;
            DateTime startDay = new DateTime(Now.Year, Now.Month, 1);
            DateTime endDay = startDay.AddMonths(1).AddDays(-1);
            var TenDangNhap = User.FindFirstValue(ClaimTypes.Name);
            var Check_q = _context.TaiKhoan.Where(x => x.TenDangNhap == TenDangNhap).FirstOrDefault();
            var Check_nv = _context.NhanVien.Where(x => x.MaNV == TenDangNhap).FirstOrDefault();

            var res = await (from a in _context.TrinhKy
                             join nv in _context.NhanVien on a.NguoiLap equals nv.ID_NV
                             join bp in _context.PhongBan on a.ID_PhongBan equals bp.ID_PhongBan
                             select new TrinhKy
                             {
                                 ID_TK = a.ID_TK,
                                 ID_PhongBan = (int)a.ID_PhongBan,
                                 TenPhongBan = bp.TenPhongBan,
                                 NoiDung = a.NoiDung,
                                 NgayTrinhKy = (DateTime?)a.NgayTrinhKy ?? default,
                                 FilePath = a.FilePath,
                                 NguoiLap = (int?)a.NguoiLap ?? default,
                                 HoTen_NguoiLap = nv.HoTen,
                                 TinhTrang_NguoiLap = (int?)a.TinhTrang_NguoiLap ?? default,
                                 Ngay_NguoiLap = (DateTime?)a.Ngay_NguoiLap ?? default,
                                 TruongPho = (int?)a.TruongPho ?? default,
                                 TinhTrang_TruongPho = (int?)a.TinhTrang_TruongPho ?? default,
                                 Ngay_TruongPho = (DateTime?)a.Ngay_TruongPho ?? default

                             }).ToListAsync();

            if (Check_q.ID_Quyen == 4)
            {
                if (begind == null && endd == null)
                {
                    res = res.Where(x => x.NgayTrinhKy >= startDay && x.NgayTrinhKy <= endDay && x.TruongPho == Check_nv.ID_NV && x.TinhTrang_TruongPho == 1).ToList();
                }
                else
                {
                    res = res.Where(x => x.NgayTrinhKy >= begind && x.NgayTrinhKy <= endd && x.TruongPho == Check_nv.ID_NV && x.TinhTrang_TruongPho == 1).ToList();
                }
            }
            else
            {
                if (begind == null && endd == null)
                {
                    res = res.Where(x => x.NgayTrinhKy >= startDay && x.NgayTrinhKy <= endDay && x.TruongPho != 0 && x.TinhTrang_TruongPho == 1).ToList();
                }
                else
                {
                    res = res.Where(x => x.NgayTrinhKy >= begind && x.NgayTrinhKy <= endd && x.TruongPho != 0 && x.TinhTrang_TruongPho == 1).ToList();
                }
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

        public async Task<IActionResult> No_Processing(DateTime? begind, DateTime? endd, int page = 1)
        {

            DateTime Now = DateTime.Now;
            DateTime startDay = new DateTime(Now.Year, Now.Month, 1);
            DateTime endDay = startDay.AddMonths(1).AddDays(-1);
            var TenDangNhap = User.FindFirstValue(ClaimTypes.Name);
            var Check_q = _context.TaiKhoan.Where(x=>x.TenDangNhap == TenDangNhap).FirstOrDefault();
            var Check_nv = _context.NhanVien.Where(x => x.MaNV == TenDangNhap).FirstOrDefault();
            var res = await (from a in _context.TrinhKy
                             join nv in _context.NhanVien on a.NguoiLap equals nv.ID_NV
                             join bp in _context.PhongBan on a.ID_PhongBan equals bp.ID_PhongBan
                             select new TrinhKy
                             {
                                 ID_TK = a.ID_TK,
                                 ID_PhongBan = (int)a.ID_PhongBan,
                                 TenPhongBan = bp.TenPhongBan,
                                 NoiDung = a.NoiDung,
                                 NgayTrinhKy = (DateTime?)a.NgayTrinhKy ?? default,
                                 FilePath = a.FilePath,
                                 NguoiLap = (int?)a.NguoiLap ?? default,
                                 HoTen_NguoiLap = nv.HoTen,
                                 TinhTrang_NguoiLap = (int?)a.TinhTrang_NguoiLap ?? default,
                                 Ngay_NguoiLap = (DateTime?)a.Ngay_NguoiLap ?? default,
                                 TruongPho = (int?)a.TruongPho ?? default,
                                 TinhTrang_TruongPho = (int?)a.TinhTrang_TruongPho ?? default,
                                 Ngay_TruongPho = (DateTime?)a.Ngay_TruongPho ?? default

                             }).ToListAsync();
            if(Check_q.ID_Quyen == 4)
            {
                if (begind == null && endd == null)
                {
                    res = res.Where(x => x.NgayTrinhKy >= startDay && x.NgayTrinhKy <= endDay && x.TruongPho == Check_nv.ID_NV && x.TinhTrang_TruongPho == 0).ToList();
                }
                else
                {
                    res = res.Where(x => x.NgayTrinhKy >= begind && x.NgayTrinhKy <= endd && x.TruongPho == Check_nv.ID_NV && x.TinhTrang_TruongPho == 0).ToList();
                }
            }    
            else
            {
                if (begind == null && endd == null)
                {
                    res = res.Where(x => x.NgayTrinhKy >= startDay && x.NgayTrinhKy <= endDay && x.TruongPho != 0 && x.TinhTrang_TruongPho == 0).ToList();
                }
                else
                {
                    res = res.Where(x => x.NgayTrinhKy >= begind && x.NgayTrinhKy <= endd && x.TruongPho != 0 && x.TinhTrang_TruongPho == 0).ToList();
                }
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

        public async Task<IActionResult> Approve(int? id, int? id_tt)
        {
            if (id == null)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";

                return RedirectToAction("Index", "KSK_TuyenDung");
            }

            var res = await (from a in _context.TrinhKy.Where(x => x.ID_TK == id)
                             join nv in _context.NhanVien on a.NguoiLap equals nv.ID_NV
                             join bp in _context.PhongBan on a.ID_PhongBan equals bp.ID_PhongBan
                             select new TrinhKy
                             {
                                 ID_TK = a.ID_TK,
                                 ID_PhongBan = (int)a.ID_PhongBan,
                                 TenPhongBan = bp.TenPhongBan,
                                 NoiDung = a.NoiDung,
                                 NgayTrinhKy = (DateTime?)a.NgayTrinhKy ?? default,
                                 FilePath = a.FilePath,
                                 NguoiLap = (int?)a.NguoiLap ?? default,
                                 HoTen_NguoiLap = nv.HoTen

                             }).ToListAsync();
            TrinhKy DO = new TrinhKy();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID_TK = a.ID_TK;
                    DO.ID_PhongBan = (int)a.ID_PhongBan;
                    DO.NoiDung = a.NoiDung;
                    DO.NgayTrinhKy = (DateTime?)a.NgayTrinhKy ?? default;
                    DO.FilePath = a.FilePath;
                    DO.TinhTrang_PheDuyet = id_tt;
                }
            }
            else
            {
                return NotFound();
            }



            return PartialView(DO);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, TrinhKy _DO)
        {

            try
            {
                var ID_TK = _context.TrinhKy.Where(x=>x.ID_TK == _DO.ID_TK).FirstOrDefault();
                var result = _context.Database.ExecuteSqlRaw("EXEC TrinhKy_update_TP {0},{1},{2},{3}", _DO.ID_TK, 1, DateTime.Now, _DO.GhiChu);

                var List = _context.KSK_BenhNgheNghiep.Where(x =>x.ID_TK==_DO.ID_TK).ToList();
                foreach (var item in List)
                {
                    var result_pd = _context.Database.ExecuteSqlRaw("EXEC KSK_BenhNgheNghiep_update_pd {0},{1}", item.ID_KSK_BNN, _DO.TinhTrang_PheDuyet);

                    var check_std = _context.SoTheoDoi_KSK.Where(x => x.ID_NV == item.ID_NV).FirstOrDefault();
                    if (check_std != null)
                    {
                        if (check_std.ID_ViTriLaoDong == null)
                        {
                            var result_cN = _context.Database.ExecuteSqlRaw("EXEC SoTheoDoi_KSK_update_vitri {0},{1}",
                                                                             check_std.ID_STD, item.ID_ViTriLaoDong);
                        }
                        else
                        {
                            if (check_std.ID_ViTriLaoDong != item.ID_ViTriLaoDong)
                            {
                                var result_cN = _context.Database.ExecuteSqlRaw("EXEC SoTheoDoi_KSK_update_vitri {0},{1}",
                                                                             check_std.ID_STD, item.ID_ViTriLaoDong);
                            }
                        }

                    }
                }

                TempData["msgSuccess"] = "<script>alert('Phê duyệt thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Phê duyệt thất bại');</script>";
            }

            return RedirectToAction("No_Processing", "TrinhKy", new { begind = _DO.NgayTrinhKy, endd = _DO.NgayTrinhKy });
        }
        public async Task<IActionResult> export()
        {
            List<PhongBan> pb =await _context.PhongBan.ToListAsync();
            ViewBag.PBList = new SelectList(pb, "ID_PhongBan", "TenPhongBan");
            return PartialView();
        }
        public async Task<IActionResult> exportEX(int? id, DateTime? ngay)
        {
            var res=await (from a in _context.KSK_BenhNgheNghiep
                           join b in _context.ViTriLaoDong on a.ID_ViTriLaoDong equals b.ID_ViTriLaoDong
                           join e in _context.NhanVien on a.ID_NV equals e.ID_NV
                           join f in _context.PhongBan on e.ID_PhongBan equals f.ID_PhongBan
                           join g in _context.ViTriLamViec on e.ID_ViTri equals g.ID_ViTri
                           join h in _context.KipLamViec on e.ID_Kip equals h.ID_Kip
                           where a.ID_PhongBan==id && a.NgayLenDanhSach==ngay
                           select new KSK_BenhNgheNghiep()
                           {
                               MaNV=e.MaNV,
                               TenViTriLaoDong=b.TenViTriLaoDong,
                               TenPhongBan=f.TenPhongBan,
                               HoTen=e.HoTen,
                               NgaySinh=e.NgaySinh,
                               TenViTri=g.TenViTri,
                               TenKip=h.TenKip,
                               NgayNhanViec=e.NgayVaoLam,
                               ID_ViTriLaoDong=a.ID_ViTriLaoDong,
                               TGtiepxuc=(h.TenKip.ToLower().Contains("HC"))? "Tiếp xúc 6-8 giờ/20-24 ngày/Tháng": "Tiếp xúc 10-12 giờ/18-20 ngày/Tháng",
                               GhiChu=a.GhiChu
                           }).ToArrayAsync();
            string path = "Form files/Danh_sach_CBNV_kham BNN.xlsx";
            HttpContext.Response.ContentType = "application/xlsx";
            string filePath = Path.Combine(_webHostEnvironment.ContentRootPath, path);
            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheet(1);
                for (int i = 0; i < res.Count(); i++)
                {
                    worksheet.Cell(i + 7,  1).Value = i+1;
                    worksheet.Cell(i + 7,  2).Value = res[i].TenPhongBan;
                    worksheet.Cell(i + 7,  3).Value = res[i].TenViTriLaoDong;
                    worksheet.Cell(i + 7,  4).Value = res[i].MaNV;
                    worksheet.Cell(i + 7,  5).Value = res[i].HoTen;
                    worksheet.Cell(i + 7,  6).Value = res[i].NgaySinh;
                    worksheet.Cell(i + 7,  7).Value = res[i].TenViTri;
                    worksheet.Cell(i + 7,  8).Value = res[i].TenKip;
                    worksheet.Cell(i + 7,  9).Value = res[i].NgayNhanViec;
                    worksheet.Cell(i + 7,  10).Value = res[i].TGtiepxuc;

                   var chitieu= await (from a in _context.ChiTiet_ChiTieuNoiDung_ViTri.Where(x => x.ID_ViTriLaoDong == res[i].ID_ViTriLaoDong)
                                  join b in _context.DanhSachDocHai on a.ID_DocHai equals b.ID_DocHai
                                  join c in _context.ChiTieuNoiDung on b.ID_DocHai equals c.ID_DocHai
                                  select new
                                  {
                                      tenChiTieu=b.TenDocHai.ToLower(),
                                      tennd=c.TenNoiDung.ToLower()
                                  }).ToListAsync();
                    worksheet.Cell(i + 7, 11).Value = (chitieu.Where(x=>x.tenChiTieu.Contains("bức xạ nhiệt")).Any())?"X":"";
                    worksheet.Cell(i + 7, 12).Value = (chitieu.Where(x=>x.tenChiTieu.Contains("tiếng ồn")).Any())?"X":"";
                    worksheet.Cell(i + 7, 13).Value = (chitieu.Where(x=>x.tenChiTieu.Contains("rung toàn thân")).Any())?"X":"";
                    worksheet.Cell(i + 7, 14).Value = (chitieu.Where(x=>x.tenChiTieu.Contains("bụi than")).Any())?"X":"";
                    worksheet.Cell(i + 7, 15).Value = (chitieu.Where(x=>x.tenChiTieu.Contains("bụi toàn phần")).Any())?"X":"";
                    worksheet.Cell(i + 7, 16).Value = (chitieu.Where(x=>x.tenChiTieu.Contains("hơi độc")).Any())?"X":"";
                    worksheet.Cell(i + 7, 17).Value = (chitieu.Where(x=>x.tenChiTieu.Contains("cacbon")).Any())?"X":"";
                    worksheet.Cell(i + 7, 18).Value = (chitieu.Where(x=>x.tenChiTieu.Contains("benzen")).Any())?"X":"";
                    worksheet.Cell(i + 7, 19).Value = (chitieu.Where(x=>x.tenChiTieu.Contains("viêm gan")).Any())?"X":"";
                    worksheet.Cell(i + 7, 20).Value = (chitieu.Where(x=>x.tenChiTieu.Contains("môi trường ẩm ướt")).Any())?"X":"";
                    worksheet.Cell(i + 7, 21).Value = (chitieu.Where(x=>x.tenChiTieu.Contains("dầu mở bẩn")).Any())?"X":"";
                    worksheet.Cell(i + 7, 22).Value = (chitieu.Where(x=>x.tenChiTieu.Contains("yếu tố gây sạm da")).Any())?"X":"";
                    worksheet.Cell(i + 7, 23).Value = (chitieu.Where(x=>x.tenChiTieu.Contains("hiv")).Any())?"X":"";
                    worksheet.Cell(i + 7, 24).Value = (chitieu.Where(x=>x.tennd.Contains("tim phổi")).Any())?"X":"";
                    worksheet.Cell(i + 7, 25).Value = (chitieu.Where(x=>x.tennd.Contains("hô hấp")).Any())?"X":"";
                    worksheet.Cell(i + 7, 26).Value = (chitieu.Where(x=>x.tennd.Contains("cột sống")).Any())?"X":"";
                    worksheet.Cell(i + 7, 27).Value = (chitieu.Where(x=>x.tennd.Contains("thính lực")).Any())?"X":"";
                    worksheet.Cell(i + 7, 28).Value = (chitieu.Where(x=>x.tennd.Contains("nhãn áp")).Any())?"X":"";
                    worksheet.Cell(i + 7, 29).Value = (chitieu.Where(x=>x.tennd.Contains("điện tim")).Any())?"X":"";
                    worksheet.Cell(i + 7, 30).Value = (chitieu.Where(x=>x.tennd.Contains("hbco")).Any())?"X":"";
                    worksheet.Cell(i + 7, 31).Value = (chitieu.Where(x=>x.tennd.Contains("ts-tc")).Any())?"X":"";
                    worksheet.Cell(i + 7, 32).Value = (chitieu.Where(x=>x.tennd.Contains("hcv")).Any())?"X":"";
                    worksheet.Cell(i + 7, 33).Value = (chitieu.Where(x=>x.tennd.Contains("nước tiểu")).Any())?"X":"";
                    worksheet.Cell(i + 7, 34).Value = (chitieu.Where(x=>x.tennd.Contains("hiv")).Any())?"X":"";
                    worksheet.Cell(i + 7, 35).Value = (chitieu.Where(x=>x.tennd.Contains("ph da")).Any())?"X":"";
                    worksheet.Cell(i + 7, 36).Value = (chitieu.Where(x=>x.tennd.Contains("biodose")).Any())?"X":"";
                    worksheet.Cell(i + 7, 37).Value = res[i].GhiChu;
                }
                // Lưu lại file Excel
                using (var stream = new MemoryStream())
                {
                    worksheet.Cell("A1").Select();
                    workbook.SaveAs(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", path);
                }
            }
        }

    }
}
