using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyYTe.Models;
using QuanLyYTe.Repositorys;
using Microsoft.AspNetCore.Mvc.Rendering;
using ExcelDataReader;
using System.Data;

namespace QuanLyYTe.Controllers
{
    public class DanhSachThuocController : Controller
    {
        private readonly DataContext _context;

        public DanhSachThuocController(DataContext _context)
        {
            this._context = _context;
        }
        public async Task<IActionResult> Index(string search, int page = 1)
        {
            var res = await (from a in _context.LoaiThuoc
                             select new LoaiThuoc
                             {
                                 ID_LoaiThuoc = a.ID_LoaiThuoc,
                                 TenThuoc = a.TenThuoc
                             }).ToListAsync();
            if (search != null)
            {
                res = res.Where(x => x.TenThuoc.ToLower().Contains(search.ToLower())).ToList();
            }
            const int pageSize = 10;
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

        public async Task<IActionResult> Delete(int id, int page)
        {
            try
            {

                var result = _context.Database.ExecuteSqlRaw("EXEC LoaiThuoc_delete {0}", id);

                TempData["msgSuccess"] = "<script>alert('Xóa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xóa dữ liệu thất bại');</script>";
            }


            return RedirectToAction("Index", "DanhSachThuoc", new { page = page });
        }
        public async Task<IActionResult> Create()
        {

            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LoaiThuoc _DO)
        {
            try
            {

                var result_dochai = _context.Database.ExecuteSqlRaw("EXEC LoaiThuoc_insert {0}", _DO.TenThuoc);
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Thêm mới thất bại');</script>";
            }

            return RedirectToAction("Index", "DanhSachThuoc");
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";

                return RedirectToAction("Index", "DanhSachThuoc");
            }

            var res = await (from a in _context.LoaiThuoc.Where(x => x.ID_LoaiThuoc == id)
                             select new LoaiThuoc
                             {
                                 ID_LoaiThuoc = a.ID_LoaiThuoc,
                                 TenThuoc = a.TenThuoc
                             }).ToListAsync();

            LoaiThuoc DO = new LoaiThuoc();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID_LoaiThuoc = a.ID_LoaiThuoc;
                    DO.TenThuoc = a.TenThuoc;
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
        public async Task<IActionResult> Edit(int id, int page, LoaiThuoc _DO)
        {
            try
            {

                var result = _context.Database.ExecuteSqlRaw("EXEC LoaiThuoc_update {0},{1}", id, _DO.TenThuoc);
                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";
            }


            return RedirectToAction("Index", "DanhSachThuoc", new { search = _DO.TenThuoc });
        }
    }
}
