using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyYTe.Models;
using QuanLyYTe.Repositorys;
using Microsoft.AspNetCore.Mvc.Rendering;
using ExcelDataReader;
using System.Data;
using Microsoft.Data.SqlClient;
using QuanLyYTe.Common;

namespace QuanLyYTe.Controllers
{
    public class ChiTiet_CapPhatThuocController : Controller
    {
        private readonly DataContext _context;

        public ChiTiet_CapPhatThuocController(DataContext _context)
        {
            this._context = _context;
        }
        public async Task<IActionResult> Index(int id, int page = 1)
        {
            var c = _context.ChiTiet_CapPhatThuoc;
            var res = await (from a in _context.ChiTiet_CapPhatThuoc
                             join lt in _context.LoaiThuoc on a.ID_LoaiThuoc equals lt.ID_LoaiThuoc
                             where a.ID_CapThuoc == id
                             select new ChiTiet_CapPhatThuoc
                             {
                                 ID_CT_CapThuoc = a.ID_CT_CapThuoc,
                                 ID_CapThuoc = a.ID_CapThuoc ?? 0, // Tránh NULL
                                 ID_LoaiThuoc = a.ID_LoaiThuoc ?? 0, // Tránh NULL
                                 TenLoaiThuoc = lt != null ? lt.TenThuoc : "", // Tránh lỗi nếu lt là NULL
                                 SoLuong = a.SoLuong ?? ""  // Tránh NULL
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
        public async Task<IActionResult> Delete(int id)
        {
            // int id_ct = (int)ID.ID_CapThuoc;
            var check =(from c in _context.ChiTiet_CapPhatThuoc where c.ID_CT_CapThuoc == id select  new ChiTiet_CapPhatThuoc() { ID_CT_CapThuoc=c.ID_CT_CapThuoc,ID_CapThuoc=c.ID_CapThuoc,ID_LoaiThuoc=c.ID_LoaiThuoc,SoLuong=c.SoLuong}).FirstOrDefault();
            try
            {
            //var a = _context.ChiTiet_CapPhatThuoc.Where(x => x.ID_CT_CapThuoc == id).Select(x=>new ChiTiet_CapPhatThuoc() { ID_CT_CapThuoc=x.ID_CT_CapThuoc,ID_CapThuoc=x.ID_CapThuoc,ID_LoaiThuoc=x.ID_LoaiThuoc,SoLuong=x.SoLuong}).FirstOrDefault();
                if (check != null)
                {
                    await _context.Database.ExecuteSqlRawAsync("EXEC ChiTiet_CapPhatThuoc_delete {0}", id);

                    TempData["msgSuccess"] = "<script>alert('Xóa thành công');</script>";
                }
                else
                {
                    TempData["msgSuccess"] = "<script>alert('Không tìm thấy dữ liệu cẩn xóa');</script>";
                }
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xóa dữ liệu thất bại');</script>";
            }



            return RedirectToAction("Index", "ChiTiet_CapPhatThuoc", new { id = check?.ID_CapThuoc });
        }
        public async Task<IActionResult> Create(int? id)
        {

            List<LoaiThuoc> lt = await _context.LoaiThuoc.ToListAsync();
            ViewBag.lt = new SelectList(lt, "ID_LoaiThuoc", "TenThuoc");

            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChiTiet_CapPhatThuoc _DO, int? id)
        {
            try
            {
                var result_ID_LoaiThuoc = await _context.Database.ExecuteSqlRawAsync("EXEC ChiTiet_CapPhatThuoc_insert {0},{1},{2}",
                                   id, _DO.ID_LoaiThuoc, _DO.SoLuong);
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Thêm mới thất bại');</script>";
            }

            return RedirectToAction("Index", "ChiTiet_CapPhatThuoc", new { id = id });
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";

                return RedirectToAction("Index", "ChiTiet_CapPhatThuoc");
            }

            var res = await (from a in _context.ChiTiet_CapPhatThuoc.Where(x => x.ID_CT_CapThuoc == id)
                             join lt in _context.LoaiThuoc on a.ID_LoaiThuoc equals lt.ID_LoaiThuoc
                             select new ChiTiet_CapPhatThuoc
                             {
                                 ID_CT_CapThuoc = a.ID_CT_CapThuoc,
                                 ID_CapThuoc = (int)a.ID_CapThuoc,
                                 ID_LoaiThuoc = (int)a.ID_LoaiThuoc,
                                 TenLoaiThuoc = lt.TenThuoc??"",
                                 SoLuong = a.SoLuong??""
                             }).ToListAsync();

            ChiTiet_CapPhatThuoc DO = new ChiTiet_CapPhatThuoc();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID_CT_CapThuoc = a.ID_CT_CapThuoc;
                    DO.ID_CapThuoc = (int)a.ID_CapThuoc;
                    DO.ID_LoaiThuoc = (int)a.ID_LoaiThuoc;
                    DO.SoLuong = a.SoLuong;
                }
                List<LoaiThuoc> lt = _context.LoaiThuoc.ToList();
                ViewBag.LTList = new SelectList(lt, "ID_LoaiThuoc", "TenThuoc", DO.ID_LoaiThuoc);
            }
            else
            {
                return NotFound();
            }



            return PartialView(DO);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ChiTiet_CapPhatThuoc _DO)
        {
            try
            {

                var result = _context.Database.ExecuteSqlRaw("EXEC ChiTiet_CapPhatThuoc_update {0},{1},{2}", _DO.ID_CT_CapThuoc, _DO.ID_LoaiThuoc, _DO.SoLuong);

                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";
            }


            return RedirectToAction("Index", "ChiTiet_CapPhatThuoc", new { id = _DO.ID_CapThuoc });
        }
    }
}
