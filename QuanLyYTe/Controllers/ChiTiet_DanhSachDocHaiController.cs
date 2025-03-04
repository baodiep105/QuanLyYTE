using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyYTe.Models;
using QuanLyYTe.Repositorys;
using Microsoft.AspNetCore.Mvc.Rendering;
using ExcelDataReader;
using System.Data;
using Microsoft.Data.SqlClient;

namespace QuanLyYTe.Controllers
{
    public class ChiTiet_DanhSachDocHaiController : Controller
    {
        private readonly DataContext _context;

        public ChiTiet_DanhSachDocHaiController(DataContext _context)
        {
            this._context = _context;
        }
        public async Task<IActionResult> Index(int id, int page = 1)
        {
            var res = await (from a in _context.ChiTieuNoiDung.Where(x=>x.ID_DocHai == id)
                             select new ChiTieuNoiDung
                             {
                                 ID_CTND = a.ID_CTND,
                                 ID_DocHai = (int)a.ID_DocHai,
                                 TenChiTieu = a.TenChiTieu,
                                 TenNoiDung = a.TenNoiDung
                             }).ToListAsync();

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
            ViewBag.Data = id;
            return View(data);
        }
        public async Task<IActionResult> Create(int? id)
        {

            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChiTieuNoiDung _DO, int? id)
        {
            try
            {

                var result_dochai = _context.Database.ExecuteSqlRaw("EXEC ChiTieuNoiDung_insert {0},{1},{2}", id,_DO.TenChiTieu, _DO.TenNoiDung);

                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Thêm mới thất bại');</script>";
            }

            return RedirectToAction("Index", "ChiTiet_DanhSachDocHai", new { id = id});
        }

        public async Task<IActionResult> Delete(int id)
        {
            var ID = _context.ChiTieuNoiDung.Where(x => x.ID_CTND == id).FirstOrDefault();
            int id_ct = ID.ID_DocHai;
            try
            {

                var result = _context.Database.ExecuteSqlRaw("EXEC ChiTieuNoiDung_delete {0}", id);

                TempData["msgSuccess"] = "<script>alert('Xóa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xóa dữ liệu thất bại');</script>";
            }

           

            return RedirectToAction("Index", "ChiTiet_DanhSachDocHai", new { id = id_ct });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";

                return RedirectToAction("Index", "DanhSachDocHai");
            }

            var res = await (from a in _context.ChiTieuNoiDung.Where(x => x.ID_CTND == id)
                             select new ChiTieuNoiDung
                             {
                                 ID_CTND = a.ID_CTND,
                                 ID_DocHai = (int)a.ID_DocHai,
                                 TenChiTieu = a.TenChiTieu,
                                 TenNoiDung = a.TenNoiDung
                             }).ToListAsync();

            ChiTieuNoiDung DO = new ChiTieuNoiDung();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID_CTND = a.ID_CTND;
                    DO.ID_DocHai = (int)a.ID_DocHai;
                    DO.TenChiTieu = a.TenChiTieu;
                    DO.TenNoiDung = a.TenNoiDung;
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
        public async Task<IActionResult> Edit(int id, ChiTieuNoiDung _DO)
        {
            try
            {

                var result = _context.Database.ExecuteSqlRaw("EXEC ChiTieuNoiDung_update {0},{1},{2},{3}",_DO.ID_CTND, _DO.ID_DocHai,_DO.TenChiTieu, _DO.TenNoiDung);

                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";
            }


            return RedirectToAction("Index", "ChiTiet_DanhSachDocHai", new { id = _DO.ID_DocHai });
        }

    }
}
