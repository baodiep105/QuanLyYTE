using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyYTe.Models;
using QuanLyYTe.Repositorys;
using Microsoft.AspNetCore.Mvc.Rendering;
using ExcelDataReader;
using System.Data;
using ClosedXML.Excel;
using System.Security.Claims;
using DocumentFormat.OpenXml.Office2010.Excel;
using System.Globalization;
using Microsoft.Data.SqlClient;
using DocumentFormat.OpenXml.Wordprocessing;

namespace QuanLyYTe.Controllers
{
    public class ThoiHan_KSK_BNNController : Controller
    {
        private readonly DataContext _context;

        public ThoiHan_KSK_BNNController(DataContext _context)
        {
            this._context = _context;
        }
        public async Task<IActionResult> Index(int? IDPhongBan,int? idNV, DateTime? begind, DateTime? endd, int page = 1)
       
        {
            ViewBag.PBList = new SelectList(_context.PhongBan.ToList(), "ID_PhongBan", "TenPhongBan", IDPhongBan);
            var TenDangNhap = User.FindFirstValue(ClaimTypes.Name);
            var Check_q = _context.TaiKhoan.Where(x => x.TenDangNhap == TenDangNhap).FirstOrDefault();
            var Check_nv = _context.NhanVien.Where(x => x.MaNV == TenDangNhap).FirstOrDefault();

            var res = await (from a in _context.KSK_BenhNgheNghiep/*.Where(x => x.NgayLenDanhSach >= begind && x.NgayLenDanhSach <= endd *//*&& x.ID_PheDuyet == 1*//*)*/
                             join nv in _context.NhanVien on a.ID_NV equals nv.ID_NV
                             join bp in _context.PhongBan on a.ID_PhongBan equals bp.ID_PhongBan
                             join k in _context.KipLamViec on nv.ID_Kip equals k.ID_Kip into ulist3
                             from k in ulist3.DefaultIfEmpty()
                             join vt in _context.ViTriLamViec on nv.ID_ViTri equals vt.ID_ViTri into ulist4
                             from vt in ulist4.DefaultIfEmpty()
                             join vtld in _context.ViTriLaoDong on a.ID_ViTriLaoDong equals vtld.ID_ViTriLaoDong into ulist5
                             from vtld in ulist5.DefaultIfEmpty()
                             select new KSK_BenhNgheNghiep
                             {
                                 ID_KSK_BNN = a.ID_KSK_BNN,
                                 ID_NV = (int)a.ID_NV,
                                 MaNV = nv.MaNV,
                                 HoTen = nv.HoTen,
                                 NgaySinh = (DateTime?)nv.NgaySinh ?? default,
                                 ID_PhongBan = (int)a.ID_PhongBan,
                                 TenPhongBan = bp.TenPhongBan,
                                 TenKip = k.TenKip,
                                 TenViTri = vt.TenViTri,
                                 NgayKham = (DateTime?)a.NgayKham ?? default,
                                 NgayLenDanhSach = (DateTime?)a.NgayLenDanhSach ?? default,
                                 ID_ViTriLaoDong = (int)a.ID_ViTriLaoDong,
                                 TenViTriLaoDong = vtld.TenViTriLaoDong,
                                 XQuangTimPhoi=a.XQuangTimPhoi,
                                 DoCNHoHap=a.DoCNHoHap,
                                 XQuangCSTLThangNghien=a.XQuangCSTLThangNghien,
                                 DoThinhLuc=a.DoThinhLuc,
                                 DoNhanAp=a.DoNhanAp,
                                 DinhLuongHbCo =(double?) a.DinhLuongHbCo,
                                 DoDienTim = a.DoDienTim,
                                 ThoiGianMauChay = (double?)a.ThoiGianMauChay,
                                 ThoiGianMauDong = (double?)a.ThoiGianMauDong,
                                 TestHCV_HBsAg = a.TestHCV_HBsAg,
                                 SGOT = (double?)a.SGOT,
                                 SGPT = (double?)a.SGPT,
                                 NuocTieu = a.NuocTieu,
                                 HIV = a.HIV,
                                 DoPHda = (double?)a.DoPHda,
                                 DoLieuSinhHoc = a.DoLieuSinhHoc,
                                 KetLuan=a.KetLuan,
                                 GhiChu = a.GhiChu,
                                 ID_PheDuyet = (int?)a.ID_PheDuyet??default

                             }).ToListAsync();

            if (Check_q.ID_Quyen == 1 && Check_q.ID_Quyen == 2)
            {
                res = res.Where(x => x.ID_PhongBan == IDPhongBan).ToList();
            }
            else
            {
                res = res.Where(x => x.ID_PhongBan == Check_q.ID_PhongBan).ToList();
            }
            if (begind!=null && endd != null)
            {
                res = res.Where(x => x.NgayLenDanhSach >= begind && x.NgayLenDanhSach <= endd).ToList();
            }
            if (idNV != null)
            {
                res = res.Where(x => x.ID_NV == idNV).ToList();
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


        public async Task<IActionResult> Deatail(int? ID_NV, int page = 1)
        {

            var res = await (from a in _context.KSK_BenhNgheNghiep.Where(x => x.ID_NV == ID_NV)
                             join nv in _context.NhanVien on a.ID_NV equals nv.ID_NV
                             join bp in _context.PhongBan on a.ID_PhongBan equals bp.ID_PhongBan
                             join k in _context.KipLamViec on nv.ID_Kip equals k.ID_Kip into ulist3
                             from k in ulist3.DefaultIfEmpty()
                             join vt in _context.ViTriLamViec on nv.ID_ViTri equals vt.ID_ViTri into ulist4
                             from vt in ulist4.DefaultIfEmpty()
                             join vtld in _context.ViTriLaoDong on a.ID_ViTriLaoDong equals vtld.ID_ViTriLaoDong into ulist5
                             from vtld in ulist5.DefaultIfEmpty()
                             select new KSK_BenhNgheNghiep
                             {
                                 ID_KSK_BNN = a.ID_KSK_BNN,
                                 ID_NV = (int)a.ID_NV,
                                 MaNV = nv.MaNV,
                                 HoTen = nv.HoTen,
                                 NgaySinh = (DateTime?)nv.NgaySinh ?? default,
                                 ID_PhongBan = (int)a.ID_PhongBan,
                                 TenPhongBan = bp.TenPhongBan,
                                 TenKip = k.TenKip,
                                 TenViTri = vt.TenViTri,
                                 NgayKham = (DateTime?)a.NgayKham ?? default,
                                 NgayLenDanhSach = (DateTime?)a.NgayLenDanhSach ?? default,
                                 ID_ViTriLaoDong = (int)a.ID_ViTriLaoDong,
                                 TenViTriLaoDong = vtld.TenViTriLaoDong,
                                 GhiChu = a.GhiChu,
                                 ID_PheDuyet = (int?)a.ID_PheDuyet ?? default

                             }).ToListAsync();
            var id_nv = _context.NhanVien.Where(x => x.ID_NV == ID_NV).FirstOrDefault();
            if(id_nv != null )
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

        public async Task<IActionResult> Delete(int id, int? page)
        {
            try
            {
                var delete = _context.CT_KSK_BenhNgheNghiep.Where(x => x.ID_KSK_BNN == id).ToList();

                foreach (var x in delete)
                {
                    var result_delete = _context.Database.ExecuteSqlRaw("EXEC CT_KSK_BenhNgheNghiep_delete {0}", x.ID_CT_KSKBNN);
                }

                var result = _context.Database.ExecuteSqlRaw("EXEC KSK_BenhNgheNghiep_delete {0}", id);

                TempData["msgSuccess"] = "<script>alert('Xóa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xóa dữ liệu thất bại');</script>";
            }


            return RedirectToAction("Index", "ThoiHan_KSK_BNN", new { page = page });
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";

                return RedirectToAction("Index", "ThoiHan_KSK_BNN");
            }

            var res = await (from a in _context.KSK_BenhNgheNghiep.Where(x=>x.ID_KSK_BNN == id)
                             join nv in _context.NhanVien on a.ID_NV equals nv.ID_NV
                             join bp in _context.PhongBan on a.ID_PhongBan equals bp.ID_PhongBan
                             join k in _context.KipLamViec on nv.ID_Kip equals k.ID_Kip into ulist3
                             from k in ulist3.DefaultIfEmpty()
                             join vt in _context.ViTriLamViec on nv.ID_ViTri equals vt.ID_ViTri into ulist4
                             from vt in ulist4.DefaultIfEmpty()
                             join vtld in _context.ViTriLaoDong on a.ID_ViTriLaoDong equals vtld.ID_ViTriLaoDong into ulist5
                             from vtld in ulist5.DefaultIfEmpty()
                             select new KSK_BenhNgheNghiep
                             {
                                 ID_KSK_BNN = a.ID_KSK_BNN,
                                 ID_NV = (int)a.ID_NV,
                                 MaNV = nv.MaNV,
                                 HoTen = nv.HoTen,
                                 NgaySinh = (DateTime?)nv.NgaySinh ?? default,
                                 ID_PhongBan = (int)a.ID_PhongBan,
                                 TenPhongBan = bp.TenPhongBan,
                                 TenKip = k.TenKip,
                                 TenViTri = vt.TenViTri,
                                 NgayKham = (DateTime?)a.NgayKham ?? default,
                                 NgayLenDanhSach = (DateTime?)a.NgayLenDanhSach ?? default,
                                 ID_ViTriLaoDong = (int)a.ID_ViTriLaoDong,
                                 TenViTriLaoDong = vtld.TenViTriLaoDong,
                                 XQuangTimPhoi = a.XQuangTimPhoi,
                                 DoCNHoHap = a.DoCNHoHap,
                                 XQuangCSTLThangNghien = a.XQuangCSTLThangNghien,
                                 DoThinhLuc = a.DoThinhLuc,
                                 DoNhanAp = a.DoNhanAp,
                                 DinhLuongHbCo = (double?)a.DinhLuongHbCo,
                                 DoDienTim = a.DoDienTim,
                                 ThoiGianMauChay = (double?)a.ThoiGianMauChay,
                                 ThoiGianMauDong = (double?)a.ThoiGianMauDong,
                                 TestHCV_HBsAg = a.TestHCV_HBsAg,
                                 SGOT = (double?)a.SGOT,
                                 SGPT = (double?)a.SGPT,
                                 NuocTieu = a.NuocTieu,
                                 HIV = a.HIV,
                                 DoPHda = (double?)a.DoPHda,
                                 DoLieuSinhHoc = a.DoLieuSinhHoc,
                                 KetLuan = a.KetLuan,
                                 GhiChu = a.GhiChu,
                                 ID_PheDuyet = (int?)a.ID_PheDuyet ?? default

                             }).FirstOrDefaultAsync();

           
            if (res !=null)
            {
                var NhanVien = (from nv in _context.NhanVien
                                select new NhanVien
                                {
                                    ID_NV = (int)nv.ID_NV,
                                    HoTen = nv.MaNV + " : " + nv.HoTen
                                }).ToList();
                ViewBag.NVList = new SelectList(NhanVien, "ID_NV", "HoTen", res.ID_NV);

                List<ViTriLaoDong> gt = _context.ViTriLaoDong.ToList();
                ViewBag.VTLDList = new SelectList(gt, "ID_ViTriLaoDong", "TenViTriLaoDong", res.ID_ViTriLaoDong);

                DateTime NK = (DateTime)res.NgayLenDanhSach;
                ViewBag.NgayLenDanhSach = NK.ToString("yyyy-MM-dd");

            }
            else
            {
                return NotFound();
            }



            return PartialView(res);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KSK_BenhNgheNghiep _DO)
        {
            try
            {

                var check = _context.KSK_BenhNgheNghiep.Where(x => x.ID_KSK_BNN == _DO.ID_KSK_BNN).FirstOrDefault();
                SqlParameter[] sqlParameters = new SqlParameter[]
                                   {
                                    new SqlParameter("@ID_KSK_BNN", SqlDbType.Int) { Value =_DO.ID_KSK_BNN },
                                    new SqlParameter("@ID_NV", SqlDbType.Int) { Value = _DO.ID_NV },
                                    new SqlParameter("@ID_ViTriLaoDong", SqlDbType.Int) { Value =_DO.ID_ViTriLaoDong },
                                    new SqlParameter("@NgayLenDanhSach", SqlDbType.Date) { Value = _DO.NgayLenDanhSach ?? (object)DBNull.Value },
                                    new SqlParameter("@XQuangTimPhoi", SqlDbType.NVarChar) { Value = _DO.XQuangTimPhoi ?? (object)DBNull.Value },
                                    new SqlParameter("@DoCNHoHap", SqlDbType.NVarChar) { Value = _DO.DoCNHoHap ?? (object)DBNull.Value },
                                    new SqlParameter("@XQuangCSTLThangNghien", SqlDbType.NVarChar) { Value = _DO.XQuangCSTLThangNghien ?? (object)DBNull.Value },
                                    new SqlParameter("@DoThinhLuc", SqlDbType.NVarChar) { Value = _DO.DoThinhLuc ?? (object)DBNull.Value },
                                    new SqlParameter("@DoNhanAp", SqlDbType.NVarChar) { Value = _DO.DoNhanAp ?? (object)DBNull.Value },
                                    new SqlParameter("@DinhLuongHbCo", SqlDbType.Float) { Value = _DO.DinhLuongHbCo ?? (object)DBNull.Value },
                                    new SqlParameter("@DoDienTim", SqlDbType.NVarChar) { Value = _DO.DoDienTim ?? (object)DBNull.Value },
                                    new SqlParameter("@ThoiGianMauChay", SqlDbType.Float) { Value = _DO.ThoiGianMauChay ?? (object)DBNull.Value },
                                    new SqlParameter("@ThoiGianMauDong", SqlDbType.Float) { Value = _DO.ThoiGianMauDong ?? (object)DBNull.Value },
                                    new SqlParameter("@TestHCV_HBsAg", SqlDbType.NVarChar) { Value = _DO.TestHCV_HBsAg?? (object)DBNull.Value },
                                    new SqlParameter("@SGOT", SqlDbType.Float) { Value = _DO.SGOT ?? (object)DBNull.Value },
                                    new SqlParameter("@SGPT", SqlDbType.Float) { Value = _DO.SGPT ?? (object)DBNull.Value },
                                    new SqlParameter("@NuocTieu", SqlDbType.NVarChar) { Value = _DO.NuocTieu ?? (object)DBNull.Value },
                                    new SqlParameter("@HIV", SqlDbType.NVarChar) { Value = _DO.HIV ?? (object)DBNull.Value },
                                    new SqlParameter("@DoPHda", SqlDbType.Float) { Value = _DO.DoPHda ?? (object)DBNull.Value },
                                    new SqlParameter("@DoLieuSinhHoc", SqlDbType.NVarChar) { Value = _DO.DoLieuSinhHoc ?? (object)DBNull.Value },
                                    new SqlParameter("@KetLuan", SqlDbType.NVarChar) { Value = _DO.KetLuan ?? (object)DBNull.Value },
                                    new SqlParameter("@GhiChu", SqlDbType.NVarChar) { Value = _DO.GhiChu ?? (object)DBNull.Value }
                                   };

                _context.Database.ExecuteSqlRaw(
                    "EXEC KSK_BenhNgheNghiep_update @ID_KSK_BNN, @ID_NV, @ID_ViTriLaoDong, @NgayLenDanhSach, @XQuangTimPhoi, " +
                    "@DoCNHoHap, @XQuangCSTLThangNghien, @DoThinhLuc, @DoNhanAp, @DinhLuongHbCo, @DoDienTim, " +
                    "@ThoiGianMauChay, @ThoiGianMauDong, @TestHCV_HBsAg, @SGOT, @SGPT, @NuocTieu, @HIV, " +
                    "@DoPHda, @DoLieuSinhHoc, @KetLuan, @GhiChu",
                    sqlParameters
                );




                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";
            }


            return RedirectToAction("Index", "ThoiHan_KSK_BNN");
        }

        public FileResult TestDownloadPCF()
        {
            string filePath = "Form files/BM_KSK_BenhNgheNghiep_kq.xlsx";
            HttpContext.Response.ContentType = "application/xlsx";
            FileContentResult result = new FileContentResult
            (System.IO.File.ReadAllBytes(filePath), "application/xlsx")
            {
                FileDownloadName = "BM_KSK_BenhNgheNghiep_kq.xlsx"
            };
            return result;
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
                        int ID_KSK_BNN = 0;
                        for (int i = 6; i < serviceDetails.Rows.Count; i++)
                        {

                            string Check = serviceDetails.Rows[i][1].ToString().Trim();
                            if (Check != "")
                            {
                                /*string ID_KSK = serviceDetails.Rows[i][1].ToString().Trim();
                                ID_KSK_BNN = Convert.ToInt32(ID_KSK);*/

                                string MNV = serviceDetails.Rows[i][1].ToString().Trim();
                                string HoVaTen = serviceDetails.Rows[i][1].ToString().Trim();
                                var check_nv = _context.NhanVien.Where(x => x.MaNV == MNV).FirstOrDefault();
                                if (check_nv == null)
                                {
                                    TempData["msgSuccess"] = "<script>alert('Vui lòng cập nhật dữ liệu nhân viên: " + MNV + "');</script>";
                                    return RedirectToAction("Index", "KSK_ChuyenViTri");
                                }
                                /*string GioiTinh = serviceDetails.Rows[i][3].ToString().Trim();
                                var check_gioitinh = _context.GioiTinh.Where(x => x.TenGioiTinh == GioiTinh).FirstOrDefault();
                                if (check_gioitinh == null)
                                {
                                    TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra tên giới tính. Nhân viên: " + HoVaTen + "');</script>";
                                    return RedirectToAction("Index", "KSK_TuyenDung");
                                }*/
                                string ngaytrinhky = serviceDetails.Rows[i][3].ToString();
                                DateTime ngay = DateTime.ParseExact(ngaytrinhky, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);
                                string xqtim = serviceDetails.Rows[i][4].ToString().Trim();
                                string hohap = serviceDetails.Rows[i][5].ToString().Trim();
                                string xqCSTL = serviceDetails.Rows[i][6].ToString().Trim();
                                string doThinhLuc = serviceDetails.Rows[i][7].ToString().Trim();
                                string doNhanAp = serviceDetails.Rows[i][8].ToString().Trim();
                                string dinhLuong = serviceDetails.Rows[i][9].ToString().Trim();
                                string doDienTim = serviceDetails.Rows[i][10].ToString().Trim();
                                string TGMauChay = serviceDetails.Rows[i][11].ToString().Trim();
                                string TGMauDong = serviceDetails.Rows[i][12].ToString().Trim();
                                string testHCV = serviceDetails.Rows[i][13].ToString().Trim();
                                string sGOT = serviceDetails.Rows[i][14].ToString().Trim();
                                string sGPT = serviceDetails.Rows[i][15].ToString().Trim();
                                string nuocTieu = serviceDetails.Rows[i][16].ToString().Trim();
                                string HIV = serviceDetails.Rows[i][17].ToString().Trim();
                                string doPHda = serviceDetails.Rows[i][18].ToString().Trim();
                                string doLieuSinhHoc = serviceDetails.Rows[i][19].ToString().Trim();
                                string KL = serviceDetails.Rows[i][20].ToString().Trim();
                                string ghiChu = serviceDetails.Rows[i][21].ToString().Trim();

                                // mai lam
                                var Check_bnn = _context.KSK_BenhNgheNghiep.Where(x => x.NgayLenDanhSach == ngay && x.ID_PhongBan==check_nv.ID_PhongBan && x.ID_NV==check_nv.ID_NV).FirstOrDefault();
                                if (Check_bnn != null)
                                {

                                    SqlParameter[] sqlParameters = new SqlParameter[]
                                    {
                                    new SqlParameter("@ID_KSK_BNN", SqlDbType.Int) { Value =Check_bnn.ID_KSK_BNN },
                                    new SqlParameter("@ID_NV", SqlDbType.Int) { Value = check_nv.ID_NV },
                                    new SqlParameter("@ID_ViTriLaoDong", SqlDbType.Int) { Value =Check_bnn.ID_ViTriLaoDong },
                                    new SqlParameter("@NgayLenDanhSach", SqlDbType.Date) { Value = Check_bnn.NgayLenDanhSach ?? (object)DBNull.Value },
                                    new SqlParameter("@XQuangTimPhoi", SqlDbType.NVarChar) { Value = xqtim ?? (object)DBNull.Value },
                                    new SqlParameter("@DoCNHoHap", SqlDbType.NVarChar) { Value = hohap ?? (object)DBNull.Value },
                                    new SqlParameter("@XQuangCSTLThangNghien", SqlDbType.NVarChar) { Value = xqCSTL ?? (object)DBNull.Value },
                                    new SqlParameter("@DoThinhLuc", SqlDbType.NVarChar) { Value = doThinhLuc ?? (object)DBNull.Value },
                                    new SqlParameter("@DoNhanAp", SqlDbType.NVarChar) { Value = doNhanAp ?? (object)DBNull.Value },
                                    new SqlParameter("@DinhLuongHbCo", SqlDbType.Float) { Value = dinhLuong ?? (object)DBNull.Value },
                                    new SqlParameter("@DoDienTim", SqlDbType.NVarChar) { Value = doDienTim ?? (object)DBNull.Value },
                                    new SqlParameter("@ThoiGianMauChay", SqlDbType.Float) { Value = TGMauChay ?? (object)DBNull.Value },
                                    new SqlParameter("@ThoiGianMauDong", SqlDbType.Float) { Value = TGMauDong ?? (object)DBNull.Value },
                                    new SqlParameter("@TestHCV_HBsAg", SqlDbType.NVarChar) { Value = testHCV?? (object)DBNull.Value },
                                    new SqlParameter("@SGOT", SqlDbType.Float) { Value = sGOT ?? (object)DBNull.Value },
                                    new SqlParameter("@SGPT", SqlDbType.Float) { Value = sGPT ?? (object)DBNull.Value },
                                    new SqlParameter("@NuocTieu", SqlDbType.NVarChar) { Value = nuocTieu ?? (object)DBNull.Value },
                                    new SqlParameter("@HIV", SqlDbType.NVarChar) { Value = HIV ?? (object)DBNull.Value },
                                    new SqlParameter("@DoPHda", SqlDbType.Float) { Value = doPHda ?? (object)DBNull.Value },
                                    new SqlParameter("@DoLieuSinhHoc", SqlDbType.NVarChar) { Value = doLieuSinhHoc ?? (object)DBNull.Value },
                                    new SqlParameter("@KetLuan", SqlDbType.NVarChar) { Value = KL ?? (object)DBNull.Value },
                                    new SqlParameter("@GhiChu", SqlDbType.NVarChar) { Value = ghiChu ?? (object)DBNull.Value }
                                    };

                                    _context.Database.ExecuteSqlRaw(
                                        "EXEC KSK_BenhNgheNghiep_update @ID_KSK_BNN, @ID_NV, @ID_ViTriLaoDong, @NgayLenDanhSach, @XQuangTimPhoi, " +
                                        "@DoCNHoHap, @XQuangCSTLThangNghien, @DoThinhLuc, @DoNhanAp, @DinhLuongHbCo, @DoDienTim, " +
                                        "@ThoiGianMauChay, @ThoiGianMauDong, @TestHCV_HBsAg, @SGOT, @SGPT, @NuocTieu, @HIV, " +
                                        "@DoPHda, @DoLieuSinhHoc, @KetLuan, @GhiChu",
                                        sqlParameters
                                    );
                                }
                                else
                                {
                                    TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra ngày trình ký đã đúng chưa. Nhân viên: " + HoVaTen + "');</script>";
                                    return RedirectToAction("Index", "ThoiHan_KSK_BNN");
                                }
                            }


                        }
                    }
                }
                TempData["msgSuccess"] = "<script>alert('Import thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Import thất bại');</script>";
            }

            return RedirectToAction("Index", "ThoiHan_KSK_BNN");
        }

        private List<KSK_BenhNgheNghiep> GetDemarcation(DateTime? begind, DateTime? endd,int? IDPhongBan)
        {
            var res = (from a in _context.KSK_BenhNgheNghiep.Where(x => x.NgayLenDanhSach >= begind && x.NgayLenDanhSach <= endd)
                       join nv in _context.NhanVien on a.ID_NV equals nv.ID_NV
                       join bp in _context.PhongBan on a.ID_PhongBan equals bp.ID_PhongBan
                       join k in _context.KipLamViec on nv.ID_Kip equals k.ID_Kip into ulist3
                       from k in ulist3.DefaultIfEmpty()
                       join vt in _context.ViTriLamViec on nv.ID_ViTri equals vt.ID_ViTri into ulist4
                       from vt in ulist4.DefaultIfEmpty()
                       join vtld in _context.ViTriLaoDong on a.ID_ViTriLaoDong equals vtld.ID_ViTriLaoDong into ulist5
                       from vtld in ulist5.DefaultIfEmpty()
                       select new KSK_BenhNgheNghiep
                       {
                           ID_KSK_BNN = a.ID_KSK_BNN,
                           ID_NV = (int)a.ID_NV,
                           MaNV = nv.MaNV,
                           HoTen = nv.HoTen,
                           NgaySinh = (DateTime?)nv.NgaySinh ?? default,
                           NgayNhanViec = (DateTime?)nv.NgayVaoLam ?? default,
                           ID_PhongBan = (int)a.ID_PhongBan,
                           TenPhongBan = bp.TenPhongBan,
                           TenKip = k.TenKip,
                           TenViTri = vt.TenViTri,
                           NgayKham = (DateTime?)a.NgayKham ?? default,
                           NgayLenDanhSach = (DateTime?)a.NgayLenDanhSach ?? default,
                           ID_ViTriLaoDong = (int)a.ID_ViTriLaoDong,
                           TenViTriLaoDong = vtld.TenViTriLaoDong,
                           GhiChu = a.GhiChu,
                           ID_PheDuyet = (int?)a.ID_PheDuyet ?? default

                       }).ToList();
            if(IDPhongBan != null )
            {
                res = res.Where(x=>x.ID_PhongBan == IDPhongBan).ToList();
            }    
            return res;
        }
        private List<CT_KSK_BenhNgheNghiep> GetDetai(int id)
        {

            var res = (from a in _context.CT_KSK_BenhNgheNghiep.Where(x => x.ID_KSK_BNN == id)
                       select new CT_KSK_BenhNgheNghiep
                       {
                           ID_CT_KSKBNN = a.ID_CT_KSKBNN,
                           ID_KSK_BNN = a.ID_KSK_BNN,
                           TenChiTieu = a.TenChiTieu,
                           TenNoiDung = a.TenNoiDung
                       }).ToList();
            return res;

        }
        public async Task<IActionResult> ExportToExcel(DateTime? begind, DateTime? endd, int? IDPhongBan)
        {
            string Ten_CBNV = "";
            try
            {

                string fileNamemau = AppDomain.CurrentDomain.DynamicDirectory + @"App_Data\BM_KSK_BNN_KQ.xlsx";
                string fileNamemaunew = AppDomain.CurrentDomain.DynamicDirectory + @"App_Data\BM_KSK_BNN_KQ_Temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNamemau);
                IXLWorksheet Worksheet = Workbook.Worksheet("QK");
                List<KSK_BenhNgheNghiep> Data = GetDemarcation(begind, endd, IDPhongBan);
                List<CT_KSK_BenhNgheNghiep> Detai = null;
                int row = 6, stt = 0, icol = 1;
                if (Data.Count > 0)
                {
                    foreach (var item in Data)
                    {

                        row++; stt++; icol = 1;

                        Worksheet.Cell(row, icol).Value = stt;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                        icol++;

                        Worksheet.Cell(row, icol).Value = item.ID_KSK_BNN;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                        icol++;
                        Worksheet.Cell(row, icol).Value = item.TenViTriLaoDong;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                        Ten_CBNV = item.MaNV;

                        icol++;
                        Worksheet.Cell(row, icol).Value = item.MaNV;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

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
                        Worksheet.Cell(row, icol).Style.DateFormat.Format = "dd/MM/yyyy";

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
                        Worksheet.Cell(row, icol).Style.DateFormat.Format = "dd/MM/yyyy";


                        icol++;
                        Worksheet.Cell(row, icol).Value = " ";
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        Detai = GetDetai(item.ID_KSK_BNN);
                        icol++;
                        foreach (var d in Detai)
                         {
                         
                            Worksheet.Cell(row, icol).Value = d.TenNoiDung;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                            row++;
                        }
                        row = row - 1;
                    }

                    Worksheet.Range("A7:L" + (row)).Style.Font.SetFontName("Times New Roman");
                    Worksheet.Range("A7:L" + (row)).Style.Font.SetFontSize(13);
                    Worksheet.Range("A7:L" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    Worksheet.Range("A7:L" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;


                    Workbook.SaveAs(fileNamemaunew);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNamemaunew);
                    string fileName = "Danh sách KSK_BNN - " + DateTime.Now.Date.ToString("dd/MM/yyyy") + ".xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {


                    Workbook.SaveAs(fileNamemaunew);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNamemaunew);
                    string fileName = "Danh sách KSK_BNN - " + DateTime.Now.Date.ToString("dd/MM/yyyy") + ".xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Có lỗi khi truy xuất dữ liệu. Vui lòng kiểm tra mã nhân viên: " + Ten_CBNV + "');</script>";

                return RedirectToAction("Index", "ThoiHan_KSK_DK", new { IDPhongBan = IDPhongBan , begind = begind , endd = endd });
            }
        }
    }
}
