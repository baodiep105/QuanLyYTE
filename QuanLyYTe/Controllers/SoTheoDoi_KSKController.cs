using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyYTe.Models;
using QuanLyYTe.Repositorys;
using Microsoft.AspNetCore.Mvc.Rendering;
using ExcelDataReader;
using System.Data;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace QuanLyYTe.Controllers
{
    public class SoTheoDoi_KSKController : Controller
    {
        private readonly DataContext _context;

        public SoTheoDoi_KSKController(DataContext _context)
        {
            this._context = _context;
        }
        public async Task<IActionResult> Index(string search, int page = 1)
        {
            var MNV = User.FindFirstValue(ClaimTypes.Name);
            var check = _context.TaiKhoan.Where(x => x.TenDangNhap == MNV).FirstOrDefault();
            /*var res = await (from a in _context.SoTheoDoi_KSK
                             join nv in _context.NhanVien on a.ID_NV equals nv.ID_NV
                             join gt in _context.GioiTinh on a.ID_GioiTinh equals gt.ID_GioiTinh
                             join vt in _context.ViTriLaoDong on a.ID_ViTriLaoDong equals vt.ID_ViTriLaoDong into ulist1
                             from vt in ulist1.DefaultIfEmpty()
                             join pl in _context.PhanLoaiThoiHan on a.ID_PhanLoai equals pl.ID_PhanLoai
                             select new SoTheoDoi_KSK
                             {
                                 ID_STD = a.ID_STD,
                                 ID_NV = (int)a.ID_NV,
                                 MaNV = nv.MaNV,
                                 HoTen = nv.HoTen,
                                 CCCD = nv.CMND,
                                 NgaySinh = (DateTime?)nv.NgaySinh ?? default,
                                 NgayNhanViec = (DateTime?)nv.NgayVaoLam ?? default,
                                 ID_ViTriLaoDong = (int?)a.ID_ViTriLaoDong??default,
                                 TenViTriLaoDong = vt.TenViTriLaoDong,
                                 ID_NhomMau = (int?)a.ID_NhomMau ?? default,
                                 ID_GioiTinh = (int)a.ID_GioiTinh,
                                 TenGioiTinh = gt.TenGioiTinh,
                                 ID_PhanLoai = a.ID_PhanLoai,
                                 TenLoai = pl.TenLoai
                             }).ToListAsync();*/
            var res = await(from a in _context.NhanVien 
                            join std in _context.SoTheoDoi_KSK on a.ID_NV equals std.ID_NV into stdtemp
                            from std in stdtemp.DefaultIfEmpty()
                            join plth in _context.PhanLoaiThoiHan on std.ID_PhanLoai equals plth.ID_PhanLoai into plthtemp
                            from plth in plthtemp.DefaultIfEmpty()
                            join b in _context.KSK_DinhKy on a.ID_NV equals b.ID_NV into btemp
                            from b in btemp.DefaultIfEmpty()
                            join gt in _context.GioiTinh on b.ID_GioiTinh equals gt.ID_GioiTinh into gttemp
                            from gt in gttemp.DefaultIfEmpty()
                            join vt in _context.ViTriLamViec on a.ID_ViTri equals vt.ID_ViTri into ulist1
                            from vt in ulist1.DefaultIfEmpty()
                            join pl in _context.PhanLoaiThoiHan on b.ID_PhanLoaiKSK equals pl.ID_PhanLoai into pltemp
                            from pl in pltemp.DefaultIfEmpty()
                            join nm in _context.NhomMau on b.ID_NhomMau equals nm.ID_NhomMau into nmtemp
                            from nm in nmtemp.DefaultIfEmpty()
                            select new SoTheoDoi_KSK
                            {
                              //  ID_STD = a.ID_STD,
                                ID_NV = (int)a.ID_NV,
                                MaNV = a.MaNV,
                                HoTen = a.HoTen,

                                CCCD = a.CMND,
                                NgaySinh = (DateTime?)a.NgaySinh ?? default,
                                NgayNhanViec = (DateTime?)a.NgayVaoLam ?? default,
                                TenViTri=vt.TenViTri,
                                //ID_NhomMau = nm.ID_NhomMau,
                                TenNhomMau= nm.TenNhomMau ?? "",
                               // ID_GioiTinh = (int)gt.ID_GioiTinh,
                                TenGioiTinh = gt.TenGioiTinh,
                              //  ID_PhanLoai = pl.ID_PhanLoai,
                                TenLoai = plth.TenLoai,
                                TenPLSK = pl.TenLoai

                            }).ToListAsync();

            if (check.ID_Quyen != 1 && check.ID_Quyen != 2)
            {
                res = res.Where(x => x.ID_PhongBan == check.ID_PhongBan).ToList();
            }
            if (search != null)
            {
                res = res.Where(x => x.MaNV.ToLower().Contains(search.ToLower()) || x.HoTen.ToLower().Contains(search.ToLower())).ToList();
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
            var ct_nd = _context.KSK_DinhKy.ToList();
            ViewData["KSK_DinhKy"] = ct_nd;
            var ct_pl = _context.PhanLoaiKSK.ToList();
            ViewData["PhanLoaiKSK"] = ct_pl;
            var ct_vtld = _context.ViTriLaoDong.ToList();
            ViewData["ViTriLaoDong"] = ct_vtld;
            return View(data);

        }
        public FileResult TestDownloadPCF()
        {
            string filePath = "Form files/BM_SoTheoDoi.xlsx";
            HttpContext.Response.ContentType = "application/xlsx";
            FileContentResult result = new FileContentResult
            (System.IO.File.ReadAllBytes(filePath), "application/xlsx")
            {
                FileDownloadName = "BM_SoTheoDoi.xlsx"
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
                    return RedirectToAction("Index", "SoTheoDoi_KSK");
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

                        for (int i = 5; i < serviceDetails.Rows.Count; i++)
                        {
                            string MNV = serviceDetails.Rows[i][1].ToString().Trim();
                            var check_nv = _context.NhanVien.Where(x => x.MaNV == MNV).FirstOrDefault();
                            if (check_nv == null)
                            {
                                TempData["msgSuccess"] = "<script>alert('Vui lòng cập nhật dữ liệu nhân viên: " + MNV + "');</script>";

                                return RedirectToAction("Index", "SoTheoDoi_KSK");
                            }

                            string PhanLoailaodong = serviceDetails.Rows[i][3].ToString().Trim();
                            var check_pl = _context.PhanLoaiThoiHan.Where(x=>x.TenLoai == PhanLoailaodong).FirstOrDefault();
                            if(check_pl == null)
                            {
                                TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra phân loại lao động nhân viên: " + MNV + "');</script>";

                                return RedirectToAction("Index", "SoTheoDoi_KSK");
                            }    

                            string GioiTinh = serviceDetails.Rows[i][4].ToString().Trim();
                            var check_gt = _context.GioiTinh.Where(x => x.TenGioiTinh == GioiTinh).FirstOrDefault();
                            if (check_gt == null)
                            {
                                TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra giới tính nhân viên: " + MNV + "');</script>";
                                return RedirectToAction("Index", "SoTheoDoi_KSK");
                            }

                            var check_sdt = _context.SoTheoDoi_KSK.Where(x => x.ID_NV == check_nv.ID_NV).FirstOrDefault();
                            if(check_sdt == null)
                            {
                                var result = _context.Database.ExecuteSqlRaw("EXEC SoTheoDoi_KSK_insert {0},{1},{2},{3},{4},{5},{6},{7}",
                                                                               check_nv.ID_NV, null, null, null,
                                                                               check_gt.ID_GioiTinh, check_pl.ID_PhanLoai, DateTime.Now, DateTime.Now);
                            }
                            else
                            {
                                var result = _context.Database.ExecuteSqlRaw("EXEC SoTheoDoi_KSK_update {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                                                                check_sdt.ID_STD, check_nv.ID_NV, null, null,
                                                                                null, check_gt.ID_GioiTinh, check_pl.ID_PhanLoai, check_sdt.ThoiHanSKS_Truoc, check_sdt.ThoiHanSKS_TiepTheo);
                            }    
                            //var result = _context.Database.ExecuteSqlRaw("EXEC SoTheoDoi_KSK_insert {0},{1},{2},{3},{4}",
                            // check_nv.ID_NV, check_vt.ID_ViTriLaoDong, null, check_gt.ID_GioiTinh, check_nv.NgayVaoLam, check_nv.NgayVaoLam);

                           
                        }
                    }
                }
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Thêm mới thất bại');</script>";
            }

            return RedirectToAction("Index", "SoTheoDoi_KSK");
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";

                return RedirectToAction("Index", "SoTheoDoi_KSK");
            }

            var res = await (from nv in _context.NhanVien
                             join a in _context.SoTheoDoi_KSK on nv.ID_NV equals a.ID_NV into atemp
                             from a in atemp.DefaultIfEmpty()
                             join gt in _context.GioiTinh on a.ID_GioiTinh equals gt.ID_GioiTinh into gttemp
                             from gt in gttemp.DefaultIfEmpty()
                             join pl in _context.PhanLoaiThoiHan on a.ID_PhanLoai equals pl.ID_PhanLoai into pltemp
                             from pl in pltemp.DefaultIfEmpty()
                             where nv.ID_NV== id
                             select new SoTheoDoi_KSK
                             {
                                 ID_STD =(int?) a.ID_STD ?? default,
                                 ID_NV = (int?)a.ID_NV ?? default,
                                 MaNV = nv.MaNV,
                                 HoTen = nv.HoTen,
                                 CCCD = nv.CMND,
                                 NgaySinh = (DateTime?)nv.NgaySinh ?? default,
                                 NgayNhanViec = (DateTime?)nv.NgayVaoLam ?? default,
                                 ID_NhomMau = (int?)a.ID_NhomMau ?? default,
                                 ID_GioiTinh = (int?)a.ID_GioiTinh??default,
                                 TenGioiTinh = gt.TenGioiTinh,
                                 ID_PhanLoai=(int?)pl.ID_PhanLoai ?? default,
                                 ThoiHanSKS_TiepTheo=a.ThoiHanSKS_TiepTheo??default,
                                 ThoiHanSKS_Truoc=a.ThoiHanSKS_TiepTheo??default,
                             }).ToListAsync();

            SoTheoDoi_KSK DO = new SoTheoDoi_KSK();
            if (res.Count> 0)
            {
                foreach (var a in res)
                {
                    DO.ID_STD = (int?)a.ID_STD ?? default;
                    DO.ID_NV = (int?)a.ID_NV ?? default;
                    DO.ID_ViTriLaoDong = (int?)a.ID_ViTriLaoDong ?? default;
                    DO.ID_NhomMau = (int?)a.ID_NhomMau ?? default;
                    DO.ID_GioiTinh = (int?)a.ID_GioiTinh??default;
                    DO.ID_PhanLoai= (int?)a.ID_PhanLoai??default;
                }

                var NhanVien = (from nv in _context.NhanVien
                                select new NhanVien
                                {
                                    ID_NV = (int)nv.ID_NV,
                                    HoTen = nv.MaNV + " : " + nv.HoTen
                                }).ToList();
                ViewBag.NVList = new SelectList(NhanVien, "ID_NV", "HoTen", DO.ID_NV);
                List<GioiTinh> gt = _context.GioiTinh.ToList();
                ViewBag.GTList = new SelectList(gt, "ID_GioiTinh", "TenGioiTinh", DO.ID_GioiTinh);
                List<PhanLoaiThoiHan> plld = _context.PhanLoaiThoiHan.ToList();
                ViewBag.plList = new SelectList(plld, "ID_PhanLoai", "TenLoai", DO.ID_PhanLoai);
            }
            else
            {
                return NotFound();
            }



            return PartialView(DO);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SoTheoDoi_KSK _DO)
        {
            try
            {
                if(_DO.ID_STD != null)
                {
                    var result = _context.Database.ExecuteSqlRaw("EXEC SoTheoDoi_KSK_update {0},{1},{2},{3},{4},{5},{6},{7}", _DO.ID_STD, _DO.ID_NV,null, null, _DO.ID_GioiTinh, _DO.ID_PhanLoai, _DO.ThoiHanSKS_Truoc,_DO.ThoiHanSKS_TiepTheo);
                }    
                else
                {
                   var result = _context.Database.ExecuteSqlRaw("EXEC SoTheoDoi_KSK_insert {0},{1},{2},{3},{4},{5},{6},{7}",  _DO.ID_NV, null, null,null, _DO.ID_GioiTinh, _DO.ID_PhanLoai,DateTime.Now, DateTime.Now);
                }    

                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";
            }


            return RedirectToAction("Index", "SoTheoDoi_KSK");
        }
        public async Task<IActionResult> Delete(int id, int page)
        {
            try
            {

                var result = _context.Database.ExecuteSqlRaw("EXEC SoTheoDoi_KSK_delete {0}", id);

                TempData["msgSuccess"] = "<script>alert('Xóa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xóa dữ liệu thất bại');</script>";
            }


            return RedirectToAction("Index", "SoTheoDoi_KSK", new { page = page });
        }
    }
}
