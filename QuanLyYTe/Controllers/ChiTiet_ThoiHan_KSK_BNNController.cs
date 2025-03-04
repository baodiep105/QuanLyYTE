using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyYTe.Models;
using QuanLyYTe.Repositorys;
using Microsoft.AspNetCore.Mvc.Rendering;
using ExcelDataReader;
using System.Data;
using Microsoft.Data.SqlClient;
using ClosedXML.Excel;

namespace QuanLyYTe.Controllers
{
    public class ChiTiet_ThoiHan_KSK_BNNController : Controller
    {
        private readonly DataContext _context;

        public ChiTiet_ThoiHan_KSK_BNNController(DataContext _context)
        {
            this._context = _context;
        }
        public async Task<IActionResult> Index(int id,int page = 1)
        {
            var res = await (from a in _context.CT_KSK_BenhNgheNghiep.Where(x=>x.ID_KSK_BNN == id)
                             select new CT_KSK_BenhNgheNghiep
                             {  
                                 ID_CT_KSKBNN = a.ID_CT_KSKBNN,
                                 ID_KSK_BNN = a.ID_KSK_BNN,
                                 TenChiTieu=a.TenChiTieu,
                                 KetQua = a.KetQua,
                                 TenNoiDung = a.TenNoiDung
                             }).ToListAsync();
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
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";

                return RedirectToAction("Index", "ThoiHan_KSK_BNN");
            }
            var res = await (from a in _context.CT_KSK_BenhNgheNghiep.Where(x => x.ID_CT_KSKBNN == id)
                             select new CT_KSK_BenhNgheNghiep
                             {
                                 ID_CT_KSKBNN = a.ID_CT_KSKBNN,
                                 ID_KSK_BNN = a.ID_KSK_BNN,
                                 TenChiTieu = a.TenChiTieu,
                                 TenNoiDung = a.TenNoiDung,
                                 KetQua = a.KetQua
                             }).ToListAsync();

            CT_KSK_BenhNgheNghiep DO = new CT_KSK_BenhNgheNghiep();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID_CT_KSKBNN = a.ID_CT_KSKBNN;
                    DO.ID_KSK_BNN = a.ID_KSK_BNN;
                    DO.TenChiTieu = a.TenChiTieu;
                    DO.TenNoiDung = a.TenNoiDung;
                    DO.KetQua = a.KetQua;
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
        public async Task<IActionResult> Edit(int id, CT_KSK_BenhNgheNghiep _DO)
        {
            try
            {
                var result = _context.Database.ExecuteSqlRaw("EXEC CT_KSK_BenhNgheNghiep_update {0},{1},{2},{3}",
                                                            _DO.ID_CT_KSKBNN, _DO.TenChiTieu, _DO.TenNoiDung, _DO.KetQua);

                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";
            }

            var check = _context.CT_KSK_BenhNgheNghiep.Where(x => x.ID_CT_KSKBNN == _DO.ID_CT_KSKBNN).FirstOrDefault();

            return RedirectToAction("Index", "ChiTiet_ThoiHan_KSK_BNN", new { id = check.ID_KSK_BNN });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var check = _context.CT_KSK_BenhNgheNghiep.Where(x => x.ID_CT_KSKBNN == id).FirstOrDefault();
            try
            {
                var result_delete = _context.Database.ExecuteSqlRaw("EXEC CT_KSK_BenhNgheNghiep_delete {0}", id);

                TempData["msgSuccess"] = "<script>alert('Xóa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xóa dữ liệu thất bại');</script>";
            }

         

            return RedirectToAction("Index", "ChiTiet_ThoiHan_KSK_BNN", new { id = check.ID_KSK_BNN});
        }
    }
}
