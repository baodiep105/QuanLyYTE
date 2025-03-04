using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyYTe.Models;
using QuanLyYTe.Repositorys;
using Microsoft.AspNetCore.Mvc.Rendering;
using ExcelDataReader;
using Microsoft.Data.SqlClient;

namespace QuanLyYTe.Controllers
{
    public class ChiTiet_ChiTieuNoiDung_ViTriController : Controller
    {
        private readonly DataContext _context;

        public ChiTiet_ChiTieuNoiDung_ViTriController(DataContext _context)
        {
            this._context = _context;
        }
        public async Task<IActionResult> Index(int id,int page = 1)
        {
            var res = await (from a in _context.ChiTiet_ChiTieuNoiDung_ViTri.Where(X=>X.ID_ViTriLaoDong == id)
                             join dh in _context.DanhSachDocHai on a.ID_DocHai equals dh.ID_DocHai
                             select new ChiTiet_ChiTieuNoiDung_ViTri
                             {
                                 ID_CT_ViTriLaoDong = a.ID_CT_ViTriLaoDong,
                                 ID_ViTriLaoDong = (int)a.ID_ViTriLaoDong,
                                 ID_DocHai = (int)a.ID_DocHai,
                                 TenDocHai = dh.TenDocHai

                             }).ToListAsync();
            ViewBag.ID_VT = id;
            var ct_nd = _context.ChiTieuNoiDung.ToList();
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
            ViewData["ChiTieuNoiDung"] = ct_nd;
            return View(data);
        }
        public async Task<IActionResult> Create(int? id)
        {

            List<ViTriLaoDong> vt = _context.ViTriLaoDong.ToList();
            ViewBag.VTList = new SelectList(vt, "ID_ViTriLaoDong", "TenViTriLaoDong", id);

            List<DanhSachDocHai> th = _context.DanhSachDocHai.ToList();
            ViewBag.DHList = new SelectList(th, "ID_DocHai", "TenDocHai");

            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChiTiet_ChiTieuNoiDung_ViTri _DO)
        {
            try
            {
                var result_ViTri = _context.Database.ExecuteSqlRaw("EXEC ChiTiet_ChiTieuNoiDung_ViTri_insert {0},{1}",
                                                                                  _DO.ID_ViTriLaoDong, _DO.ID_DocHai);
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Thêm mới thất bại');</script>";
            }

            return RedirectToAction("Index", "ChiTiet_ChiTieuNoiDung_ViTri", new { id = _DO.ID_ViTriLaoDong });
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";

                return RedirectToAction("Index", "KSK_TuyenDung");
            }

            var res = await (from a in _context.ChiTiet_ChiTieuNoiDung_ViTri.Where(X => X.ID_CT_ViTriLaoDong == id)
                             join dh in _context.DanhSachDocHai on a.ID_DocHai equals dh.ID_DocHai
                             select new ChiTiet_ChiTieuNoiDung_ViTri
                             {
                                 ID_CT_ViTriLaoDong = a.ID_CT_ViTriLaoDong,
                                 ID_ViTriLaoDong = (int)a.ID_ViTriLaoDong,
                                 ID_DocHai = (int)a.ID_DocHai,
                                 TenDocHai = dh.TenDocHai

                             }).ToListAsync();

            ChiTiet_ChiTieuNoiDung_ViTri DO = new ChiTiet_ChiTieuNoiDung_ViTri();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID_CT_ViTriLaoDong = a.ID_CT_ViTriLaoDong;
                    DO.ID_ViTriLaoDong = (int)a.ID_ViTriLaoDong;
                    DO.ID_DocHai = (int)a.ID_DocHai;
                }
                List<DanhSachDocHai> th = _context.DanhSachDocHai.ToList();
                ViewBag.DHList = new SelectList(th, "ID_DocHai", "TenDocHai", DO.ID_DocHai);
            }
            else
            {
                return NotFound();
            }



            return PartialView(DO);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ChiTiet_ChiTieuNoiDung_ViTri _DO)
        {
            try
            {
                var result = _context.Database.ExecuteSqlRaw("EXEC ChiTiet_ChiTieuNoiDung_ViTri_update {0},{1},{2}", _DO.ID_CT_ViTriLaoDong, _DO.ID_ViTriLaoDong, _DO.ID_DocHai);

                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";
            }

            return RedirectToAction("Index", "ChiTiet_ChiTieuNoiDung_ViTri", new { id = _DO.ID_ViTriLaoDong });
        }
        public async Task<IActionResult> Delete(int id, int page)
        {
            var ID = _context.ChiTiet_ChiTieuNoiDung_ViTri.Where(x => x.ID_CT_ViTriLaoDong == id).FirstOrDefault();
            try
            {

                var result = _context.Database.ExecuteSqlRaw("EXEC ChiTiet_ChiTieuNoiDung_ViTri_delete {0}", id);

                TempData["msgSuccess"] = "<script>alert('Xóa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xóa dữ liệu thất bại');</script>";
            }

            return RedirectToAction("Index", "ChiTiet_ChiTieuNoiDung_ViTri", new { id = ID.ID_ViTriLaoDong });
        }

    }
}
