using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyYTe.Models;
using QuanLyYTe.Repositorys;
using Microsoft.AspNetCore.Mvc.Rendering;
using ExcelDataReader;
using System.Data;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Hosting;

namespace QuanLyYTe.Controllers;
public class KSK_TuyenDungController : Controller
{
    private readonly DataContext _context;

    private readonly IWebHostEnvironment _webHostEnvironment;
    public KSK_TuyenDungController(DataContext _context, IWebHostEnvironment webHostEnvironment)
    {
        this._context = _context;
        _webHostEnvironment = webHostEnvironment;
    }
    public async Task<IActionResult> Index(DateTime? st,DateTime?end,int page = 1)
    {
        if (st == null || end == null) { 
            DateTime Now = DateTime.Now;
            st = new DateTime(Now.Year, Now.Month, 1);
            end =st.Value.AddMonths(1).AddDays(-1);
        }
        var res = await (from a in _context.KSK_DauVao
                         join kq in _context.KetQuaDauVao on a.ID_KetQuaDV equals kq.ID_KetQuaDV
                         join gt in _context.GioiTinh on a.ID_GioiTinh equals gt.ID_GioiTinh
                         join ld in _context.LyDoKhongDat on a.ID_LyDo equals ld.ID_LyDo into ulist1
                         from ld in ulist1.DefaultIfEmpty()
                         where a.NgayKham >=st && a.NgayKham<=end
                         select new KSK_DauVao
                         {
                             ID_KSK_DV = a.ID_KSK_DV,
                             HoVaTen = a.HoVaTen,
                             NgaySinh = a.NgaySinh,
                             CCCD = a.CCCD,
                             ID_GioiTinh = (int)a.ID_GioiTinh,
                             TenGioiTinh = gt.TenGioiTinh,
                             TDHV = a.TDHV,
                             TDCM = a.TDCM,
                             NgheNghiep = a.NgheNghiep,
                             HoKhau = a.HoKhau,
                             ID_KetQuaDV = (int)a.ID_KetQuaDV,
                             TenKetQua = kq.TenKetQua,
                             ID_LyDo = (int?)a.ID_LyDo ?? default,
                             TenLyDo = ld.TenLyDo ?? default,
                             NgayKham = a.NgayKham,
                             GhiChu = a.GhiChu
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
        ViewBag.st = st;
        ViewBag.end = end;
        return View(data);
  

    }
    public async Task<IActionResult> Deatail(string search, int page = 1)
    {

        var res = await (from a in _context.KSK_DauVao.Where(x=>x.CCCD.Contains(search))
                         join kq in _context.KetQuaDauVao on a.ID_KetQuaDV equals kq.ID_KetQuaDV
                         join gt in _context.GioiTinh on a.ID_GioiTinh equals gt.ID_GioiTinh
                         join ld in _context.LyDoKhongDat on a.ID_LyDo equals ld.ID_LyDo into ulist1
                         from ld in ulist1.DefaultIfEmpty()
                         select new KSK_DauVao
                         {
                             ID_KSK_DV = a.ID_KSK_DV,
                             HoVaTen = a.HoVaTen,
                             NgaySinh = a.NgaySinh,
                             CCCD = a.CCCD,
                             ID_GioiTinh = (int)a.ID_GioiTinh,
                             TenGioiTinh = gt.TenGioiTinh,
                             TDHV = a.TDHV,
                             TDCM = a.TDCM,
                             NgheNghiep = a.NgheNghiep,
                             HoKhau = a.HoKhau,
                             ID_KetQuaDV = (int)a.ID_KetQuaDV,
                             TenKetQua = kq.TenKetQua,
                             ID_LyDo = (int?)a.ID_LyDo ?? default,
                             TenLyDo = ld.TenLyDo ?? default,
                             NgayKham = a.NgayKham,
                             GhiChu = a.GhiChu
                         }).ToListAsync();
        ViewBag.ID_NV = search;
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




    public async Task<IActionResult> Create()
    {
        List<GioiTinh> gt = _context.GioiTinh.ToList();
        ViewBag.GTList = new SelectList(gt, "ID_GioiTinh", "TenGioiTinh");

        List<KetQuaDauVao> kq = _context.KetQuaDauVao.ToList();
        ViewBag.KQList = new SelectList(kq, "ID_KetQuaDV", "TenKetQua");

        List<LyDoKhongDat> ld = _context.LyDoKhongDat.ToList();
        ViewBag.LDList = new SelectList(ld, "ID_LyDo", "TenLyDo");


        return PartialView();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(KSK_DauVao _DO)
    {
        try
        {

            if (_DO.ID_LyDo == 0)
            {
                var result = _context.Database.ExecuteSqlRaw("EXEC KSK_DauVao_insert {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                                                        _DO.HoVaTen, _DO.NgaySinh, _DO.CCCD, _DO.ID_GioiTinh, _DO.TDHV, _DO.TDCM, _DO.NgheNghiep,
                                                        _DO.HoKhau, _DO.ID_KetQuaDV, null, _DO.NgayKham, _DO.GhiChu);
            }
            else
            {
                var result = _context.Database.ExecuteSqlRaw("EXEC KSK_DauVao_insert {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                                                     _DO.HoVaTen, _DO.NgaySinh, _DO.CCCD, _DO.ID_GioiTinh, _DO.TDHV, _DO.TDCM, _DO.NgheNghiep,
                                                     _DO.HoKhau, _DO.ID_KetQuaDV, _DO.ID_LyDo, _DO.NgayKham, _DO.GhiChu);
            }
            TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
        }
        catch (Exception e)
        {
            TempData["msgError"] = "<script>alert('Thêm mới thất bại');</script>";
        }

        return RedirectToAction("Index", "KSK_TuyenDung");
    }


    public async Task<IActionResult> Edit(int? id, int? page)
    {
        if (id == null)
        {
            TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";

            return RedirectToAction("Index", "KSK_TuyenDung");
        }

        var res = await (from a in _context.KSK_DauVao.Where(x=>x.ID_KSK_DV == id)
                         select new KSK_DauVao
                         {
                             ID_KSK_DV = a.ID_KSK_DV,
                             HoVaTen = a.HoVaTen,
                             NgaySinh = a.NgaySinh,
                             CCCD = a.CCCD,
                             ID_GioiTinh = (int)a.ID_GioiTinh,
                             TDHV = a.TDHV,
                             TDCM = a.TDCM,
                             NgheNghiep = a.NgheNghiep,
                             HoKhau = a.HoKhau,
                             ID_KetQuaDV = (int)a.ID_KetQuaDV,
                             ID_LyDo = (int?)a.ID_LyDo ?? default,
                             NgayKham = a.NgayKham,
                             GhiChu = a.GhiChu,
                             Page = (int)page
                         }).ToListAsync();

        KSK_DauVao DO = new KSK_DauVao();
        if (res.Count > 0)
        {
            foreach (var a in res)
            {
                DO.ID_KSK_DV = a.ID_KSK_DV;
                DO.HoVaTen = a.HoVaTen;
                DO.NgaySinh = a.NgaySinh;
                DO.CCCD = a.CCCD;
                DO.ID_GioiTinh = (int)a.ID_GioiTinh;
                DO.TDHV = a.TDHV;
                DO.TDCM = a.TDCM;
                DO.NgheNghiep = a.NgheNghiep;
                DO.HoKhau = a.HoKhau;
                DO.ID_KetQuaDV = (int)a.ID_KetQuaDV;
                DO.ID_LyDo = (int?)a.ID_LyDo ?? default;
                DO.NgayKham = a.NgayKham;
                DO.GhiChu = a.GhiChu;
                DO.Page = (int)page;
            }

            List<GioiTinh> gt = _context.GioiTinh.ToList();
            ViewBag.GTList = new SelectList(gt, "ID_GioiTinh", "TenGioiTinh", DO.ID_GioiTinh);

            List<KetQuaDauVao> kq = _context.KetQuaDauVao.ToList();
            ViewBag.KQList = new SelectList(kq, "ID_KetQuaDV", "TenKetQua",DO.ID_KetQuaDV);

            List<LyDoKhongDat> ld = _context.LyDoKhongDat.ToList();
            ViewBag.LDList = new SelectList(ld, "ID_LyDo", "TenLyDo", DO.ID_LyDo);

            DateTime NS = (DateTime)DO.NgaySinh;
            DateTime NK = (DateTime)DO.NgayKham;

            ViewBag.NgaySinh = NS.ToString("yyyy-MM-dd");
            ViewBag.NgayKham = NK.ToString("yyyy-MM-dd");
        }
        else
        {
            return NotFound();
        }



        return PartialView(DO);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, KSK_DauVao _DO)
    {
        try
        {

            var result = _context.Database.ExecuteSqlRaw("EXEC KSK_DauVao_update {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}", id,
                                                        _DO.HoVaTen, _DO.NgaySinh, _DO.CCCD, _DO.ID_GioiTinh, _DO.TDHV, _DO.TDCM, _DO.NgheNghiep,
                                                        _DO.HoKhau, _DO.ID_KetQuaDV, _DO.ID_LyDo, _DO.NgayKham, _DO.GhiChu);

            TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
        }
        catch (Exception e)
        {
            TempData["msgError"] = "<script>alert('Chính sửa thất bại');</script>";
        }

     

        return RedirectToAction("Index", "KSK_TuyenDung" ,new { page = _DO.Page });
    }

    public async Task<IActionResult> Delete(int id, int? page)
    {
        try
        {

            var result = _context.Database.ExecuteSqlRaw("EXEC KSK_DauVao_delete {0}", id);

            TempData["msgSuccess"] = "<script>alert('Xóa thành công');</script>";
        }
        catch (Exception e)
        {
            TempData["msgError"] = "<script>alert('Xóa dữ liệu thất bại');</script>";
        }
     

        return RedirectToAction("Index", "KSK_TuyenDung", new { page = page});
    }
    public FileResult TestDownloadPCF()
    { 
        string path = "Form files/BM_KSK_TuyenDung.xlsx";
        HttpContext.Response.ContentType = "application/xlsx";
        string filePath = Path.Combine(_webHostEnvironment.ContentRootPath, path);

        if (!System.IO.File.Exists(filePath))
        {
            return null; // Xử lý lỗi nếu file không tồn tại
        }
        List<LyDoKhongDat> ld = _context.LyDoKhongDat.ToList();

        using (var workbook = new XLWorkbook(filePath))
        {
            var worksheet = workbook.Worksheet(2);
            for (var i = 0; i < ld.Count; i++)
            {
                worksheet.Cell(i + 2, 11).Value = ld[i].TenLyDo;
            }
            // Lưu lại file Excel
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", path);
            }
        }
    }
    public async Task<IActionResult> ImportExcel()
    {
        return PartialView();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportExcel(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return RedirectToAction("Index", "KSK_TuyenDung");
            }
            string webRootPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string dirPath = Path.Combine(webRootPath, "ReceivedReports");
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            string dataFileName = Path.GetFileName(DateTime.Now.ToString("yyyyMMddHHmm"));

            string extension = Path.GetExtension(dataFileName);

            string[] allowedExtsnions = new string[] { ".xls", ".xlsx" };

            string saveToPath = Path.Combine(dirPath, dataFileName);

            using (FileStream stream = new FileStream(saveToPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
         
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            // read the excel file
            IExcelDataReader reader = null;
            using (var stream = new FileStream(saveToPath, FileMode.Open))
            {
                if (extension == ".xlsx")
                    reader = ExcelReaderFactory.CreateBinaryReader(stream);
                else
                    reader = ExcelReaderFactory.CreateOpenXmlReader(stream);

                DataSet ds = new DataSet();
                ds = reader.AsDataSet();
                reader.Close();
                if (ds != null && ds.Tables.Count > 0)
                {
                    System.Data.DataTable serviceDetails = ds.Tables[0];
                    for (int i = 5; i < serviceDetails.Rows.Count; i++)
                    {
                        string HoVaTen = serviceDetails.Rows[i][1].ToString();
                        string NgaySinh = serviceDetails.Rows[i][2].ToString();
                        DateTime NS = DateTime.ParseExact(NgaySinh, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                        string GioiTinh = serviceDetails.Rows[i][3].ToString().Trim();
                        string CCCD = serviceDetails.Rows[i][4].ToString().Trim();

                        var check_gioitinh = _context.GioiTinh.Where(x => x.TenGioiTinh == GioiTinh).FirstOrDefault();
                        if(check_gioitinh == null)
                        {
                            TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra tên giới tính. Nhân viên: " + HoVaTen + "');</script>";
                            return RedirectToAction("Index", "KSK_TuyenDung");
                        }
                        string NgheNghiep = serviceDetails.Rows[i][5].ToString().Trim();
                        string HoKhau = serviceDetails.Rows[i][6].ToString().Trim();
                        string Dat = serviceDetails.Rows[i][7].ToString().Trim();
                        string KhongDat = serviceDetails.Rows[i][8].ToString().Trim();
                        string XemXet = serviceDetails.Rows[i][9].ToString().Trim();
                        string LyDoKhongDat = serviceDetails.Rows[i][10].ToString().Trim();
                        var check_lydo = _context.LyDoKhongDat.Where(x => x.TenLyDo == LyDoKhongDat).FirstOrDefault();
                        if(check_lydo == null && KhongDat != "" || check_lydo == null && XemXet != "")
                        {
                            TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lý do không đạt. Nhân viên: " + HoVaTen + "');</script>";
                            return RedirectToAction("Index", "KSK_TuyenDung");
                        }
                        string NgayKham = serviceDetails.Rows[i][11].ToString();
                        DateTime NK = DateTime.ParseExact(NgayKham, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        string GhiChu = serviceDetails.Rows[i][12].ToString();

                        var check_ = _context.KSK_DauVao.Where(x => x.HoVaTen.Contains(HoVaTen) && x.NgaySinh == NS && x.CCCD == CCCD).FirstOrDefault();
                        if( check_ == null)
                        {
                            if (Dat != "" && KhongDat == "" && XemXet == "")
                            {

                                var result = _context.Database.ExecuteSqlRaw("EXEC KSK_DauVao_insert {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                                                            HoVaTen, NS, CCCD, check_gioitinh.ID_GioiTinh, null, null, NgheNghiep,
                                                            HoKhau, 1, null, NK, GhiChu);
                            }
                            else if (Dat == "" && KhongDat != "" && XemXet == "")
                            {
                                 
                                    var result = _context.Database.ExecuteSqlRaw("EXEC KSK_DauVao_insert {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                                                            HoVaTen, NS, CCCD, check_gioitinh.ID_GioiTinh, null, null, NgheNghiep,
                                                            HoKhau, 2, check_lydo.ID_LyDo, NK, GhiChu);
                                
                            }
                            else if (Dat == "" && KhongDat == "" && XemXet != "")
                            {
                                
                                    var result = _context.Database.ExecuteSqlRaw("EXEC KSK_DauVao_insert {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                                                           HoVaTen, NS, CCCD, check_gioitinh.ID_GioiTinh, null, null, NgheNghiep,
                                                           HoKhau, 3, check_lydo.ID_LyDo, NK, GhiChu);
                                
                               
                            }
                        }   
                        else
                        {
                            if (Dat != "" && KhongDat == "" && XemXet == "")
                            {

                                var result = _context.Database.ExecuteSqlRaw("EXEC KSK_DauVao_update {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}",
                                                                            check_.ID_KSK_DV, HoVaTen, NS, CCCD, check_gioitinh.ID_GioiTinh, null, null, NgheNghiep,
                                                                            HoKhau, 1, null, NK, GhiChu);
                            }
                            else if (Dat == "" && KhongDat != "" && XemXet == "")
                            {
                                
                                    var result = _context.Database.ExecuteSqlRaw("EXEC KSK_DauVao_update {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}",
                                                                                 check_.ID_KSK_DV, HoVaTen, NS, CCCD, check_gioitinh.ID_GioiTinh, null, null, NgheNghiep,
                                                                                  HoKhau, 2, check_lydo.ID_LyDo, NK, GhiChu);
                                
                            }
                            else if (Dat == "" && KhongDat == "" && XemXet != "")
                            {
                                 
                                    var result = _context.Database.ExecuteSqlRaw("EXEC KSK_DauVao_update {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}",
                                                                              check_.ID_KSK_DV, HoVaTen, NS, CCCD, check_gioitinh.ID_GioiTinh, null, null, NgheNghiep,
                                                                           HoKhau, 3, check_lydo.ID_LyDo, NK, GhiChu);
                                 
                            }

                        }    
          
                    }    
                }    
            }
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
        }
        catch (Exception e)
        {
            TempData["msgError"] = "<script>alert('Thêm mới thất bại');</script>";
        }

        return RedirectToAction("Index", "KSK_TuyenDung");
    }
}
