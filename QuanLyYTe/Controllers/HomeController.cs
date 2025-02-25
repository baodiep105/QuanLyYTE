using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using QuanLyYTe.Models;
using QuanLyYTe.Repositorys;
using System.Diagnostics;
namespace QuanLyYTe.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataContext _context;

        public HomeController(DataContext _context)
        {
            this._context = _context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public List<object> Get_Data()
        {
            DateTime Now = DateTime.Now;
            DateTime begind = new DateTime(Now.Year, Now.Month, 1);
            DateTime end = begind.AddMonths(1).AddDays(-1);


            List<Object> data = new List<Object>();
            List<string> Name = new List<string>();
            List<int> Count = new List<int>();
            var List = _context.PhongBan.ToList();

            foreach (var item in List)
            {
                var res = (from a in _context.CapPhatThuoc
                           select new CapPhatThuoc
                           {
                               ID_CapThuoc = a.ID_CapThuoc,
                               ID_PhongBan = (int)a.ID_PhongBan
                           }).Where(x => x.ID_PhongBan == item.ID_PhongBan).Count();
                if(res > 0)
                {
                    Count.Add(Convert.ToInt32(res));
                    Name.Add(item.TenPhongBan);
                }    
            
            }
            data.Add(Name);
            data.Add(Count);
            return data;
        }

        public List<object> Get_Result()
        {
            DateTime Now = DateTime.Now;
            DateTime begind = new DateTime(Now.Year, Now.Month, 1);
            DateTime end = begind.AddMonths(1).AddDays(-1);


            List<Object> data = new List<Object>();
            List<string> Name = new List<string>();
            List<int> Count = new List<int>();
            var List = _context.KetQuaDauVao.ToList();

            foreach (var item in List)
            {
                var res = (from a in _context.KSK_DauVao.Where(x => x.NgayKham >= begind && x.NgayKham <= end)
                           select new KSK_DauVao
                           {
                               ID_KSK_DV = a.ID_KSK_DV,
                               ID_KetQuaDV = (int)a.ID_KetQuaDV
                           }).Where(x => x.ID_KetQuaDV == item.ID_KetQuaDV).Count();
                if (res > 0)
                {
                    Count.Add(Convert.ToInt32(res));
                    Name.Add(item.TenKetQua);
                }

            }
            data.Add(Name);
            data.Add(Count);
            return data;
        }

        public List<object> Get_Reason()
        {
            DateTime Now = DateTime.Now;
            DateTime begind = new DateTime(Now.Year, Now.Month, 1);
            DateTime end = begind.AddMonths(1).AddDays(-1);


            List<Object> data = new List<Object>();
            List<string> Name = new List<string>();
            List<int> Count = new List<int>();
            var List = _context.LyDoKhongDat.Where(x=>x.LoaiLyDo == 0).ToList();

            foreach (var item in List)
            {
                var res = (from a in _context.KSK_DauVao.Where(x=>x.NgayKham >= begind && x.NgayKham <= end)
                           select new KSK_DauVao
                           {
                               ID_KSK_DV = a.ID_KSK_DV,
                               ID_LyDo = (int)a.ID_LyDo
                           }).Where(x => x.ID_LyDo == item.ID_LyDo).Count();
                if (res > 0)
                {
                    Count.Add(Convert.ToInt32(res));
                    Name.Add(item.TenLyDo);
                }

            }
            data.Add(Name);
            data.Add(Count);
            return data;
        }
        public List<object> Blood_group()
        {
   
            List<Object> data = new List<Object>();
            List<string> Name = new List<string>();
            List<int> Count = new List<int>();
            var List = _context.NhomMau.ToList();

            foreach (var item in List)
            {
                var res = (from a in _context.SoTheoDoi_KSK
                           select new SoTheoDoi_KSK
                           {
                               ID_STD = a.ID_STD,
                               ID_NhomMau = (int?)a.ID_NhomMau ?? default,
                           }).Where(x => x.ID_NhomMau == item.ID_NhomMau).Count();
                if (res > 0)
                {
                    Count.Add(Convert.ToInt32(res));
                    Name.Add(item.TenNhomMau);
                }

            }
            data.Add(Name);
            data.Add(Count);
            return data;
        }
    }
}