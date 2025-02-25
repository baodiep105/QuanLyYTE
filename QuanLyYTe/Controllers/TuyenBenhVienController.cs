using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyYTe.Models;
using QuanLyYTe.Repositorys;
using Microsoft.AspNetCore.Mvc.Rendering;
using ExcelDataReader;
using System.Data;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Presentation;

namespace QuanLyYTe.Controllers
{
    public class TuyenBenhVienController : Controller
    {
        private readonly DataContext _context;
        public TuyenBenhVienController(DataContext _context)
        {
            this._context = _context;
        }
        public async Task<IActionResult> Index(string search,int? id, int page = 1)
        {
            var res = await (from a in _context.TuyenBenhVien.Where(x=>x.ID_SCC == id)
                             select new TuyenBenhVien
                             {
                                 ID_TuyenBenhVien = a.ID_TuyenBenhVien,
                                 ID_SCC = (int)a.ID_SCC,
                                 TenBenhVien = a.TenBenhVien,
                                 Ytephutrach = a.Ytephutrach,
                                 ThoiGianChuyenVien = (DateTime?)a.ThoiGianChuyenVien ?? default,
                                 TamUng = (int?)a.TamUng??default,
                                 ThanhToan = (int?)a.ThanhToan??default,
                                 ChungTu = a.ChungTu,
                                 ThoiGianDieuTri = a.ThoiGianDieuTri
                             }).ToListAsync();
            ViewBag.ID_SCC = id;
            if (search != null)
            {
                res = res.Where(x => x.TenBenhVien.Contains(search) || x.TenBenhVien.Contains(search)).ToList();
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
            return View(data.OrderBy(x=>x.ThuTu));

        }

        public async Task<IActionResult> Create(int? id)
        {

            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TuyenBenhVien _DO, int? id, IFormFile uploadedFile)
        {
            try
            {
                int count = _context.TuyenBenhVien.Where(x => x.ID_SCC == id).Count();
                if (uploadedFile != null || uploadedFile.Length != 0)
                {
                    // Create the Directory if it is not exist
                    string webRootPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    string dirPath = Path.Combine(webRootPath, "ReceivedReports");
                    if (!Directory.Exists(dirPath))
                    {
                        Directory.CreateDirectory(dirPath);
                    }



                    // MAke sure that only Excel file is used 
                    string ImageName = Guid.NewGuid().ToString() + Path.GetExtension(uploadedFile.FileName);
                    //string FileExtension = _DO.ChuKy != null ? Path.GetExtension(_DO.ChuKy.dataFileName) : "";

                    string extension = Path.GetExtension(ImageName);
                    string saveToPath = Path.Combine(dirPath, ImageName);
                    using (FileStream stream = new FileStream(saveToPath, FileMode.Create))
                    {
                        uploadedFile.CopyTo(stream);
                    }
                    _DO.ChungTu = "~/ReceivedReports/" + ImageName;
                  
                    var result = _context.Database.ExecuteSqlRaw("EXEC TuyenBenhVien_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                                       id,count+1, _DO.TenBenhVien, _DO.Ytephutrach, _DO.ThoiGianChuyenVien, _DO.TamUng, _DO.ThanhToan, _DO.ChungTu,
                                                       _DO.ThoiGianDieuTri);
                }
                else
                {
                    var result = _context.Database.ExecuteSqlRaw("EXEC TuyenBenhVien_insert {0},{1},{2},{3},{4},{5},{6},{7}",
                                                    id, count + 1, _DO.TenBenhVien, _DO.Ytephutrach, _DO.ThoiGianChuyenVien, _DO.TamUng, _DO.ThanhToan,null,
                                                    _DO.ThoiGianDieuTri);
                }    



              


                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Thêm mới thất bại');</script>";
            }

            return RedirectToAction("Index", "TuyenBenhVien", new {id = id });
        }
        public async Task<IActionResult> Delete(int id)
        {
            var ID = _context.TuyenBenhVien.Where(x => x.ID_TuyenBenhVien == id).FirstOrDefault();
            try
            {
              
                var result = _context.Database.ExecuteSqlRaw("EXEC TuyenBenhVien_delete {0}", id);

                TempData["msgSuccess"] = "<script>alert('Xóa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xóa dữ liệu thất bại');</script>";
            }
            return RedirectToAction("Index", "TuyenBenhVien", new {id = ID.ID_SCC});
        }
        public async Task<IActionResult> Edit(int? id, int? page)
        {
            if (id == null)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";

                return RedirectToAction("Index", "TuyenBenhVien");
            }

            var res = await (from a in _context.TuyenBenhVien.Where(x => x.ID_TuyenBenhVien == id)
                             select new TuyenBenhVien
                             {
                                 ID_TuyenBenhVien = a.ID_TuyenBenhVien,
                                 ID_SCC = (int)a.ID_SCC,
                                 TenBenhVien = a.TenBenhVien,
                                 Ytephutrach = a.Ytephutrach,
                                 ThoiGianChuyenVien = (DateTime?)a.ThoiGianChuyenVien ?? default,
                                 TamUng = a.TamUng,
                                 ThanhToan = a.ThanhToan,
                                 ChungTu = a.ChungTu,
                                 ThuTu= a.ThuTu,
                                 ThoiGianDieuTri = a.ThoiGianDieuTri
                             }).ToListAsync();

            TuyenBenhVien DO = new TuyenBenhVien();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID_TuyenBenhVien = a.ID_TuyenBenhVien;
                    DO.ID_SCC = (int)a.ID_SCC;
                    DO.TenBenhVien = a.TenBenhVien;
                    DO.Ytephutrach = a.Ytephutrach;
                    DO.ThoiGianChuyenVien = (DateTime?)a.ThoiGianChuyenVien ?? default;
                    DO.TamUng = a.TamUng;
                    DO.ThanhToan = a.ThanhToan;
                    DO.ChungTu = a.ChungTu;
                    DO.ThuTu = a.ThuTu;
                    DO.ThoiGianDieuTri = a.ThoiGianDieuTri;
                }
                DateTime TGCV = (DateTime)DO.ThoiGianChuyenVien;

                ViewBag.ThoiGianChuyenVien = TGCV.ToString("yyyy-MM-dd");
            }
            else
            {
                return NotFound();
            }



            return PartialView(DO);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TuyenBenhVien _DO,IFormFile? uploadedFile)
        {
            try
            {
                if (uploadedFile != null )
                {
                    // Create the Directory if it is not exist
                    string webRootPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    string dirPath = Path.Combine(webRootPath, "ReceivedReports");
                    if (!Directory.Exists(dirPath))
                    {
                        Directory.CreateDirectory(dirPath);
                    }



                    // MAke sure that only Excel file is used 
                    string ImageName = Guid.NewGuid().ToString() + Path.GetExtension(uploadedFile.FileName);
                    //string FileExtension = _DO.ChuKy != null ? Path.GetExtension(_DO.ChuKy.dataFileName) : "";

                    string extension = Path.GetExtension(ImageName);
                    string saveToPath = Path.Combine(dirPath, ImageName);
                    using (FileStream stream = new FileStream(saveToPath, FileMode.Create))
                    {
                        uploadedFile.CopyTo(stream);
                    }

                    var result = _context.Database.ExecuteSqlRaw("EXEC TuyenBenhVien_update {0},{1},{2},{3},{4},{5},{6},{7},{8}", id,
                                                             _DO.ThuTu, _DO.TenBenhVien, _DO.Ytephutrach, _DO.ThoiGianChuyenVien, _DO.TamUng, _DO.ThanhToan, "~/ReceivedReports/" + ImageName, _DO.ThoiGianDieuTri);

                }
                else
                {
                    var result = _context.Database.ExecuteSqlRaw("EXEC TuyenBenhVien_update {0},{1},{2},{3},{4},{5},{6},{7},{8}", id,
                                                           _DO.ThuTu, _DO.TenBenhVien, _DO.Ytephutrach, _DO.ThoiGianChuyenVien, _DO.TamUng, _DO.ThanhToan, _DO.ChungTu, _DO.ThoiGianDieuTri);

                }
                if (System.IO.File.Exists(_DO.ChungTu))
                {
                    System.IO.File.Delete(_DO.ChungTu);
                }
                    TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Chính sửa thất bại');</script>";
            }

            return RedirectToAction("Index", "TuyenBenhVien", new { id = _DO.ID_SCC });
        }
    }
}
