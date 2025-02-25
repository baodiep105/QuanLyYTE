using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyYTe.Models;
using QuanLyYTe.Repositorys;
using Microsoft.AspNetCore.Mvc.Rendering;
using ExcelDataReader;
using System.Data;
using QuanLyYTe.Common;

namespace QuanLyYTe.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly DataContext _context;

        public TaiKhoanController(DataContext _context)
        {
            this._context = _context;
        }
        public async Task<IActionResult> Index(string search, int page = 1)
        {
            var res = await (from a in _context.TaiKhoan
                             join nv in _context.NhanVien on a.ID_NV equals nv.ID_NV
                             join bp in _context.PhongBan on a.ID_PhongBan equals bp.ID_PhongBan
                             join vt in _context.ViTriLamViec on nv.ID_ViTri equals vt.ID_ViTri
                             join q in _context.Quyen on a.ID_Quyen equals q.ID_Q
                             select new TaiKhoan
                             {
                                 ID_TK = a.ID_TK,
                                 ID_NV = (int)a.ID_NV,
                                 MaNV = nv.MaNV,
                                 HoTen = nv.HoTen,
                                 ID_PhongBan = (int?)a.ID_PhongBan,
                                 TenPhongBan = bp.TenPhongBan,
                                 TenViTri = vt.TenViTri,
                                 TenDangNhap = a.TenDangNhap,
                                 MatKhau = a.MatKhau,
                                 ID_Quyen = (int)a.ID_Quyen,
                                 TenQuyen = q.TenQuyen,
                                 IsLock = (int?)a.IsLock ?? default,
                                 BDA_ID = (int?)a.BDA_ID ?? default,
                                 ChuKy = a.ChuKy
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
            return View(data);

        }
        public async Task<IActionResult> Create()
        {
           
            List<Quyen> q = _context.Quyen.ToList();
            ViewBag.QList = new SelectList(q, "ID_Q", "TenQuyen");

            List<PhongBan> pb = _context.PhongBan.ToList();
            ViewBag.BPList = new SelectList(pb, "ID_PhongBan", "TenPhongBan");

            var NhanVien = (from nv in _context.NhanVien
                            select new NhanVien
                            {
                                ID_NV = (int)nv.ID_NV,
                                HoTen = nv.MaNV + " : " + nv.HoTen
                            }).ToList();
            ViewBag.NVList = new SelectList(NhanVien, "ID_NV", "HoTen");

            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaiKhoan _DO)
        {
            try
            {
                string MatKhau = "123456a@";
                var ID_NV = _context.NhanVien.Where(x => x.ID_NV == _DO.ID_NV).FirstOrDefault(); 
                if(ID_NV == null)
                {
                    TempData["msgSuccess"] = "<script>alert('Vui lòng cập nhật dữ liệu nhân viên');</script>";
                    return RedirectToAction("Index", "TaiKhoan");
                }
                if (_DO.BDA_ID == null)
                {
                    var result = _context.Database.ExecuteSqlRaw("EXEC TaiKhoan_insert {0},{1},{2},{3},{4},{5},{6},{7}",
                                                            _DO.ID_NV, ID_NV.ID_PhongBan, ID_NV.MaNV, Encryptor.MD5Hash(MatKhau), _DO.ID_Quyen, 1, null, null);
                }
                else
                {
                    var result = _context.Database.ExecuteSqlRaw("EXEC TaiKhoan_insert {0},{1},{2},{3},{4},{5},{6},{7}",
                                                               _DO.ID_NV, ID_NV.ID_PhongBan, ID_NV.MaNV, Encryptor.MD5Hash(MatKhau), _DO.ID_Quyen, 1, _DO.BDA_ID, null);
                }
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Thêm mới thất bại');</script>";
            }

            return RedirectToAction("Index", "TaiKhoan");
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";

                return RedirectToAction("Index", "TaiKhoan");
            }

            var res = await (from a in _context.TaiKhoan.Where(x=>x.ID_TK == id)
                             select new TaiKhoan
                             {
                                 ID_TK = a.ID_TK,
                                 ID_NV = (int)a.ID_NV,
                                 ID_PhongBan = (int?)a.ID_PhongBan,
                                 TenDangNhap = a.TenDangNhap,
                                 MatKhau = a.MatKhau,
                                 ID_Quyen = (int)a.ID_Quyen,
                                 IsLock = (int?)a.IsLock ?? default,
                                 BDA_ID = (int?)a.BDA_ID ?? default,
                                 ChuKy = a.ChuKy
                             }).ToListAsync();

            TaiKhoan DO = new TaiKhoan();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID_TK = a.ID_TK;
                    DO.ID_NV = (int)a.ID_NV;
                    DO.ID_PhongBan = (int?)a.ID_PhongBan;
                    DO.TenDangNhap = a.TenDangNhap;
                    DO.MatKhau = a.MatKhau;
                    DO.ID_Quyen = (int)a.ID_Quyen;
                    DO.IsLock = (int?)a.IsLock ?? default;
                    DO.IsLock = (int?)a.IsLock ?? default;
                    DO.BDA_ID = (int?)a.BDA_ID ?? default;
                    DO.ChuKy = a.ChuKy;
                }

                var NhanVien = (from nv in _context.NhanVien
                                select new NhanVien
                                {
                                    ID_NV = (int)nv.ID_NV,
                                    HoTen = nv.MaNV + " : " + nv.HoTen
                                }).ToList();
                ViewBag.NVList = new SelectList(NhanVien, "ID_NV", "HoTen", DO.ID_NV);

                List<PhongBan> pb = _context.PhongBan.ToList();
                ViewBag.PBList = new SelectList(pb, "ID_PhongBan", "TenPhongBan", DO.ID_PhongBan);

                List<PhongBan> bda = _context.PhongBan.ToList();
                ViewBag.BDAList = new SelectList(bda, "ID_PhongBan", "TenPhongBan", DO.BDA_ID);

                List<Quyen> q = _context.Quyen.ToList();
                ViewBag.QList = new SelectList(q, "ID_Q", "TenQuyen", DO.ID_Quyen);
            }
            else
            {
                return NotFound();
            }



            return PartialView(DO);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(IFormFile image, TaiKhoan _DO)
        {
            try
            {

                var ID_NV = _context.NhanVien.Where(x => x.ID_NV == _DO.ID_NV).FirstOrDefault();
                if (image != null || image.Length != 0)
                {
                   
                
                    // Create the Directory if it is not exist
                    string webRootPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    string dirPath = Path.Combine(webRootPath, "ReceivedReports");
                    if (!Directory.Exists(dirPath))
                    {
                        Directory.CreateDirectory(dirPath);
                    }



                    // MAke sure that only Excel file is used 
                    string ImageName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    //string FileExtension = _DO.ChuKy != null ? Path.GetExtension(_DO.ChuKy.dataFileName) : "";

                    string extension = Path.GetExtension(ImageName);
                    string saveToPath = Path.Combine(dirPath, ImageName);
                    using (FileStream stream = new FileStream(saveToPath, FileMode.Create))
                    {
                        image.CopyTo(stream);
                    }

                    _DO.ChuKy = "~/ReceivedReports/" + ImageName;

                    var result = _context.Database.ExecuteSqlRaw("EXEC TaiKhoan_update {0},{1},{2},{3},{4},{5}",
                                                                _DO.ID_TK, ID_NV.ID_NV, ID_NV.MaNV, _DO.ID_Quyen, _DO.BDA_ID, _DO.ChuKy);
                }
                else
                {
                    var result = _context.Database.ExecuteSqlRaw("EXEC TaiKhoan_update {0},{1},{2},{3},{4},{5}",
                                                               _DO.ID_TK, ID_NV.ID_NV, ID_NV.MaNV, _DO.ID_Quyen, _DO.BDA_ID, null);
                }    


                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";
            }


            return RedirectToAction("Index", "TaiKhoan");
        }




        public async Task<IActionResult> Lock(int id, int page)
        {
            try
            {

                var result = _context.Database.ExecuteSqlRaw("EXEC TaiKhoan_lock {0},{1}", id, 0);

                TempData["msgSuccess"] = "<script>alert('Khóa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Khóa dữ liệu thất bại');</script>";
            }


            return RedirectToAction("Index", "TaiKhoan", new { page = page });
        }
        public async Task<IActionResult> Unlock(int id, int page)
        {
            try
            {

                var result = _context.Database.ExecuteSqlRaw("EXEC TaiKhoan_lock {0},{1}", id, 1);

                TempData["msgSuccess"] = "<script>alert('Mở khóa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Mở khóa dữ liệu thất bại');</script>";
            }


            return RedirectToAction("Index", "TaiKhoan", new { page = page });
        }
    }
}
