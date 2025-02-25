using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyYTe.Models;
using QuanLyYTe.Repositorys;
using Microsoft.AspNetCore.Mvc.Rendering;
using ExcelDataReader;
using System.Data;


namespace QuanLyYTe.Controllers
{
    public class NhanVienController : Controller
    {
        private readonly DataContext _context;

        public NhanVienController(DataContext _context)
        {
            this._context = _context;
        }
        public async Task<IActionResult> Index(string search,int page = 1)
        {
            var res = await (from a in _context.NhanVien.Where(x=>x.ID_TinhTrangLamViec == 1)
                             join bp in _context.PhongBan on a.ID_PhongBan equals bp.ID_PhongBan
                             join px in _context.PhanXuong on a.ID_PhanXuong equals px.ID_PhanXuong into ulist1
                             from px in ulist1.DefaultIfEmpty()
                             join to in _context.ToLamViec on a.ID_To equals to.ID_To into ulist2
                             from to in ulist2.DefaultIfEmpty()
                             join k in _context.KipLamViec on a.ID_Kip equals k.ID_Kip into ulist3
                             from k in ulist3.DefaultIfEmpty()
                             join vt in _context.ViTriLamViec on a.ID_ViTri equals vt.ID_ViTri into ulist4
                             from vt in ulist4.DefaultIfEmpty()
                             select new NhanVien
                             {
                                 MaNV = a.MaNV,
                                 HoTen = a.HoTen,
                                 CMND = a.CMND,
                                 NgaySinh = (DateTime?)a.NgaySinh ?? default,
                                 DiaChi = a.DiaChi,
                                 NgayVaoLam = (DateTime?)a.NgayVaoLam ?? default,
                                 ID_PhongBan = (int?)a.ID_PhongBan ?? default,
                                 TenPhongBan = bp.TenPhongBan,
                                 ID_PhanXuong = (int?)a.ID_PhanXuong ?? default,
                                 TenPhanXuong = px.TenPhanXuong,
                                 ID_To = (int?)a.ID_To ?? default,
                                 TenTo = to.TenTo,
                                 ID_Kip = (int?)a.ID_Kip ?? default,
                                 TenKip = k.TenKip,
                                 ID_ViTri = (int?)a.ID_ViTri ?? default,
                                 TenViTri = vt.TenViTri,
                                 ID_TinhTrangLamViec = (int)a.ID_TinhTrangLamViec
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
    }
}
