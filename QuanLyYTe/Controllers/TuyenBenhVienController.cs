using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyYTe.Models;
using QuanLyYTe.Repositorys;
using Microsoft.AspNetCore.Mvc.Rendering;
using ExcelDataReader;
using System.Data;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.InkML;
using Microsoft.Data.SqlClient;

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
                                 TamUng = (decimal?)a.TamUng??default,
                                 ThanhToan = (decimal?)a.ThanhToan??default,
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
                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@ID_SCC", SqlDbType.Int) { Value = id },
                    new SqlParameter("@ThuTu", SqlDbType.Int) { Value = count+1 },
                    new SqlParameter("@TenBenhVien", SqlDbType.NVarChar, 500) { Value = (object)_DO.TenBenhVien ??DBNull.Value},
                    new SqlParameter("@Ytephutrach", SqlDbType.NVarChar, 500) { Value =(object) _DO.Ytephutrach ??DBNull.Value},
                    new SqlParameter("@ThoiGianChuyenVien", SqlDbType.Date) { Value = (object)_DO.ThoiGianChuyenVien??DBNull.Value },
                    new SqlParameter("@TamUng", SqlDbType.Decimal) { Value =(object)_DO.TamUng??DBNull.Value },
                    new SqlParameter("@ThanhToan", SqlDbType.Decimal) { Value = (object)_DO.ThanhToan??DBNull.Value },
                   
                    new SqlParameter("@ThoiGianDieuTri", SqlDbType.NVarChar, 500) { Value = _DO.ThoiGianDieuTri }
                };
                if (uploadedFile != null)
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
                    parameters.Add(new SqlParameter("@ChungTu", SqlDbType.NVarChar, 500) { Value = _DO.ChungTu });
                    _context.Database.ExecuteSqlRaw("EXEC TuyenBenhVien_insert @ID_SCC, @ThuTu, @TenBenhVien, @Ytephutrach, @ThoiGianChuyenVien, @TamUng, @ThanhToan, @ChungTu, @ThoiGianDieuTri", parameters);
                }
                else
                {
                    parameters.Add(new SqlParameter("@ChungTu", SqlDbType.NVarChar, 500) { Value = (object?)DBNull.Value });
                    _context.Database.ExecuteSqlRaw("EXEC TuyenBenhVien_insert @ID_SCC, @ThuTu, @TenBenhVien, @Ytephutrach, @ThoiGianChuyenVien, @TamUng, @ThanhToan, @ChungTu, @ThoiGianDieuTri", parameters);
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
                if (_context.TuyenBenhVien.Where(x => x.ID_TuyenBenhVien == id).Any())
                {
                    var parameters = new List<SqlParameter>
                    {
                        new SqlParameter("@ID_TuyenBenhVien", SqlDbType.Int) { Value =  _DO.ID_TuyenBenhVien},
                        new SqlParameter("@ThuTu", SqlDbType.Int) { Value =  _DO.ThuTu },
                        new SqlParameter("@TenBenhVien", SqlDbType.NVarChar, 500) { Value = _DO.TenBenhVien },
                        new SqlParameter("@Ytephutrach", SqlDbType.NVarChar, 500) { Value = _DO.Ytephutrach },
                        new SqlParameter("@ThoiGianChuyenVien", SqlDbType.Date) { Value = _DO.ThoiGianChuyenVien },
                        new SqlParameter("@TamUng", SqlDbType.Int) { Value = _DO.TamUng },
                        new SqlParameter("@ThanhToan", SqlDbType.Int) { Value = _DO.ThanhToan },

                        new SqlParameter("@ThoiGianDieuTri", SqlDbType.NVarChar, 500) { Value = _DO.ThoiGianDieuTri }
                    };
                    if (uploadedFile != null)
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
                        parameters.Add(new SqlParameter("@ChungTu", SqlDbType.NVarChar, 500) { Value = "~/ReceivedReports/" + ImageName });
                        await _context.Database.ExecuteSqlRawAsync("EXEC TuyenBenhVien_update @ID_TuyenBenhVien, @Thutu, @TenBenhVien, @Ytephutrach, @ThoiGianChuyenVien, @TamUng, @ThanhToan, @ChungTu, @ThoiGianDieuTri", parameters);

                    }
                    else
                    {
                        parameters.Add(new SqlParameter("@ChungTu", SqlDbType.NVarChar, 500) { Value = _DO.ChungTu });
                        await _context.Database.ExecuteSqlRawAsync("EXEC TuyenBenhVien_update @ID_TuyenBenhVien, @Thutu, @TenBenhVien, @Ytephutrach, @ThoiGianChuyenVien, @TamUng, @ThanhToan, @ChungTu, @ThoiGianDieuTri", parameters);

                    }
                    if (System.IO.File.Exists(_DO.ChungTu))
                    {
                        System.IO.File.Delete(_DO.ChungTu);
                    }
                    TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
                }
                else
                {
                    TempData["msgError"] = "<script>alert('không tìm thấy tuyến bên viện cần sửa');</script>";
                }
            }
            catch (Exception e)
            {
                TempData["msgError"] = $"<script>alert('Chính sửa thất bại,đã có lỗi xãy ra:{e.Message}');</script>";
            }

            return RedirectToAction("Index", "TuyenBenhVien", new { id = _DO.ID_SCC });
        }
    }
}
