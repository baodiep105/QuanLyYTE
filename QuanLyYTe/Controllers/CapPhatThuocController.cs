using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyYTe.Models;
using QuanLyYTe.Repositorys;
using Microsoft.AspNetCore.Mvc.Rendering;
using ExcelDataReader;
using System.Data;
using Microsoft.Data.SqlClient;
using DocumentFormat.OpenXml.Office2010.Excel;
using System.IO;
using ClosedXML.Excel;
using System.Runtime.InteropServices;
using System;
using static QuanLyYTe.Models.Employees_API;
using System.Linq;
using Oracle.ManagedDataAccess.Client;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.VariantTypes;

namespace QuanLyYTe.Controllers
{
    public class CapPhatThuocController : Controller
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CapPhatThuocController(DataContext _context, IWebHostEnvironment webHostEnvironment)
        {   
            this._context = _context;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index(string search,DateTime? st,DateTime? ed, int page = 1)
        {
            if(st==null || ed == null)
            {
                DateTime now = DateTime.Now;
                st = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
                ed = st.Value.AddDays(1).AddTicks(-1);  
            }
            
            var res = await (from a in _context.CapPhatThuoc.Where(x=>x.NgayCapThuoc >= st && x.NgayCapThuoc <= ed)
                             join nv in _context.NhanVien on a.ID_NV equals nv.ID_NV into ulist1
                             from nv in ulist1.DefaultIfEmpty()
                             join pb in _context.PhongBan on a.ID_PhongBan equals pb.ID_PhongBan into ulist2
                             from pb in ulist2.DefaultIfEmpty()
                             join b in _context.NhomBenh on a.ID_NhomBenh equals b.ID_NhomBenh into ulist3
                             from b in ulist3.DefaultIfEmpty()
                             select new CapPhatThuoc
                             {
                                 ID_CapThuoc = a.ID_CapThuoc,
                                 ID_NV = (int?)a.ID_NV ?? default,
                                 MaNV = nv.MaNV,
                                 HoTen = nv.HoTen,
                                 SoDienThoai = a.SoDienThoai,
                                 ID_PhongBan = (int?)a.ID_PhongBan ?? default,
                                 TenPhongBan = pb.TenPhongBan,
                                 NgayCapThuoc = (DateTime?)a.NgayCapThuoc ?? default,
                                 ThoiGianDen = a.ThoiGianDen,
                                 ThoiGianDi = a.ThoiGianDi,
                                 SoPhutLuuLai = a.SoPhutLuuLai,
                                 ID_NhomBenh = (int?)a.ID_NhomBenh ?? default,
                                 TenNhomBenh = b.TenNhomBenh,
                                 GhiChu = a.GhiChu
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
            ViewBag.search = search;
            ViewBag.st = st;
            ViewBag.ed = ed;
            return View(data.OrderByDescending(x=>x.NgayCapThuoc));

        }
        public FileResult TestDownloadPCF()
        {
            string path = "BM_CapPhatThuoc.xlsx";
            HttpContext.Response.ContentType = "application/xlsx";
            
            string filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "App_Data", path);

            if (!System.IO.File.Exists(filePath))
            {
                return null; // Xử lý lỗi nếu file không tồn tại
            }
            List<PhongBan> pb = _context.PhongBan.ToList();
            List<NhomBenh> mb = _context.NhomBenh.ToList();
            List<LoaiThuoc> lt = _context.LoaiThuoc.ToList();

            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheet("Sheet2");
                for (var i = 0; i < pb.Count; i++)
                {
                    worksheet.Cell(i + 2, 5).Value = pb[i].TenPhongBan;
                }
                for (var i = 0; i < mb.Count; i++)
                {
                    worksheet.Cell(i + 2, 7).Value = mb[i].TenNhomBenh;
                }
                for (var i = 0; i < lt.Count; i++)
                {
                    worksheet.Cell(i + 2, 9).Value = lt[i].TenThuoc;
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
                    return RedirectToAction("Index", "CapPhatThuoc");
                }

                // Create the Directory if it is not exist
                string webRootPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string dirPath = Path.Combine(webRootPath, "ReceivedReports");
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                // MAke sure that only Excel file is used 
                string dataFileName = Path.GetFileName(DateTime.Now.ToString("yyyyMMddHHmm"));

                string extension = Path.GetExtension(dataFileName);

                string[] allowedExtsnions = new string[] { ".xls", ".xlsx" };
                // Make a Copy of the Posted File from the Received HTTP Request
                string saveToPath = Path.Combine(dirPath, dataFileName);

                using (FileStream stream = new FileStream(saveToPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                // USe this to handle Encodeing differences in .NET Core
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
                    int ID_CapThuoc = 0;
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        System.Data.DataTable serviceDetails = ds.Tables[0];

                        for (int i = 5; i < serviceDetails.Rows.Count; i++)
                        {
                            string MNV = serviceDetails.Rows[i][1].ToString().Trim();
                            if(MNV != "")
                            {
                                var check_nv = _context.NhanVien.Where(x => x.MaNV == MNV).FirstOrDefault();
                                if (check_nv == null)
                                {
                                    TempData["msgError"] = "<script>alert('Import thất bại');</script>";
                                    return RedirectToAction("Index", "CapPhatThuoc");
                                }
                                string NgayCap = serviceDetails.Rows[i][3].ToString();
                                DateTime NgayCapThuoc = DateTime.ParseExact(NgayCap, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                                string PhongBan = serviceDetails.Rows[i][4].ToString();
                                var check_phongban = _context.PhongBan.Where(x => x.TenPhongBan == PhongBan).FirstOrDefault();
                                if (check_phongban == null)
                                {
                                    TempData["msgError"] = "<script>alert('Import thất bại');</script>";
                                    return RedirectToAction("Index", "CapPhatThuoc");
                                }
                                string SoDienThoai = serviceDetails.Rows[i][5].ToString();
                                string ThoiGianDen = serviceDetails.Rows[i][6].ToString();
                                string ThoiGianDi = serviceDetails.Rows[i][7].ToString();
                                string ThoiGianLuuLai = serviceDetails.Rows[i][8].ToString();
                                string Benh = serviceDetails.Rows[i][9].ToString();
                                string LoaiThuoc = serviceDetails.Rows[i][10].ToString();
                                var Check_LoaiThuoc = _context.LoaiThuoc.Where(x => x.TenThuoc == LoaiThuoc).FirstOrDefault();
                                if (Check_LoaiThuoc == null)
                                {
                                    TempData["msgError"] = "<script>alert('Import thất bại');</script>";
                                    return RedirectToAction("Index", "CapPhatThuoc");
                                }
                                string SoLuong = serviceDetails.Rows[i][11].ToString();

                                var check_nhombenh = _context.NhomBenh.Where(x => x.TenNhomBenh == Benh).FirstOrDefault();
                                if (check_nhombenh == null)
                                {
                                    TempData["msgError"] = "<script>alert('Import thất bại');</script>";
                                    return RedirectToAction("Index", "CapPhatThuoc");
                                }
                                string GhiChu = serviceDetails.Rows[i][12].ToString();
                                var Check_ = _context.CapPhatThuoc.Where(x=>x.ID_NV == check_nv.ID_NV && x.NgayCapThuoc == NgayCapThuoc).FirstOrDefault();
                                if (Check_ == null)
                                {
                                    var Output_ID_CapThuoc = new SqlParameter
                                    {
                                        ParameterName = "ID_CapThuoc",
                                        SqlDbType = System.Data.SqlDbType.Int,
                                        Direction = System.Data.ParameterDirection.Output,
                                    };
                                    var result_ID_CapThuoc = _context.Database.ExecuteSqlRaw("EXEC CapPhatThuoc_insert_all {0},{1},{2},{3},{4},{5},{6},{7},{8},@ID_CapThuoc OUTPUT",
                                           check_nv.ID_NV, check_phongban.ID_PhongBan, SoDienThoai, NgayCapThuoc, ThoiGianDen, ThoiGianDi, ThoiGianLuuLai, check_nhombenh.ID_NhomBenh, GhiChu, Output_ID_CapThuoc);
                                    ID_CapThuoc = (int)Output_ID_CapThuoc.Value;

                                    var result_ID_LoaiThuoc = _context.Database.ExecuteSqlRaw("EXEC ChiTiet_CapPhatThuoc_insert {0},{1},{2}",
                                           ID_CapThuoc, Check_LoaiThuoc.ID_LoaiThuoc, SoLuong);
                                }
                                else
                                {
                                    var ct = _context.ChiTiet_CapPhatThuoc.Where(x=>x.ID_CapThuoc == Check_.ID_CapThuoc).ToList();
                                    foreach (var delete in ct)
                                    {
                                        var result = _context.Database.ExecuteSqlRaw("EXEC ChiTiet_CapPhatThuoc_delete {0}", delete.ID_CT_CapThuoc);
                                    }

                                    var result_ID_CapThuoc = _context.Database.ExecuteSqlRaw("EXEC CapPhatThuoc_update {0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                                        Check_.ID_CapThuoc, check_nv.ID_NV, check_phongban.ID_PhongBan, SoDienThoai, NgayCapThuoc, ThoiGianDen, ThoiGianDi, ThoiGianLuuLai, check_nhombenh.ID_NhomBenh, GhiChu);


                                    ID_CapThuoc = (int)Check_.ID_CapThuoc;
                                    var result_ID_LoaiThuoc = _context.Database.ExecuteSqlRaw("EXEC ChiTiet_CapPhatThuoc_insert {0},{1},{2}",
                                           Check_.ID_CapThuoc, Check_LoaiThuoc.ID_LoaiThuoc, SoLuong);
                                }    

                            }    
                            else
                            {
                                string LoaiThuoc = serviceDetails.Rows[i][10].ToString();
                                var Check_LoaiThuoc = _context.LoaiThuoc.Where(x => x.TenThuoc == LoaiThuoc).FirstOrDefault();
                                if (Check_LoaiThuoc == null)
                                {
                                    TempData["msgError"] = "<script>alert('Import thất bại');</script>";
                                    return RedirectToAction("Index", "CapPhatThuoc");
                                }
                                string SoLuong = serviceDetails.Rows[i][11].ToString();
                                var result_ID_LoaiThuoc = _context.Database.ExecuteSqlRaw("EXEC ChiTiet_CapPhatThuoc_insert {0},{1},{2}",
                                    ID_CapThuoc, Check_LoaiThuoc.ID_LoaiThuoc, SoLuong);
                            }    

                        }
                    }
                }
                TempData["msgSuccess"] = "<script>alert('Import thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Import thất bại');</script>";
            }

            return RedirectToAction("Index", "CapPhatThuoc");
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var res = await (from a in _context.ChiTiet_CapPhatThuoc.Where(x => x.ID_CapThuoc == id)
                                 select new ChiTiet_CapPhatThuoc
                                 {
                                     ID_CT_CapThuoc = a.ID_CT_CapThuoc,
                                 }).ToListAsync();

                foreach (var item in res)
                {
                   _context.Database.ExecuteSqlRaw("EXEC ChiTiet_CapPhatThuoc_delete {0}", item.ID_CT_CapThuoc);
                }
                var result = _context.Database.ExecuteSqlRaw("EXEC CapPhatThuoc_delete {0}", id);

                return Ok(new { msg = "Xóa thành công" });
            }
            catch (Exception e)
            {
                return Ok(new { msg = "Xóa dữ liệu thất bại" });
            }
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CapPhatThuoc model)
        {
            try
            {
                var outputParam = new SqlParameter
                {
                    ParameterName = "ID_CapThuoc",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Output
                };

                var result = await _context.Database.ExecuteSqlRawAsync(
                    "EXEC CapPhatThuoc_insert_all {0},{1},{2},{3},{4},{5},{6},{7},{8}, @ID_CapThuoc OUTPUT",
                    model.ID_NV, model.ID_PhongBan, model.SoDienThoai, model.NgayCapThuoc,
                    model.ThoiGianDen, model.ThoiGianDi, model.SoPhutLuuLai, model.ID_NhomBenh,
                    model.GhiChu, outputParam);

                int idCapThuoc = (int)outputParam.Value;
                if (model.detail != null)
                {
                    foreach (var detail in model.detail)
                    {
                        await _context.Database.ExecuteSqlRawAsync(
                            "EXEC ChiTiet_CapPhatThuoc_insert {0}, {1}, {2}",
                            idCapThuoc, detail.ID_LoaiThuoc, detail.SoLuong);
                    }

                }

                return Json(new { success = true, message = "Cấp phát thuốc thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";

                return RedirectToAction("Index", "CapPhatThuoc");
            }

            var res = await (from a in _context.CapPhatThuoc.Where(x=>x.ID_CapThuoc == id)
                             join nv in _context.NhanVien on a.ID_NV equals nv.ID_NV into ulist1
                             from nv in ulist1.DefaultIfEmpty()
                             join pb in _context.PhongBan on a.ID_PhongBan equals pb.ID_PhongBan into ulist2
                             from pb in ulist2.DefaultIfEmpty()
                             join b in _context.NhomBenh on a.ID_NhomBenh equals b.ID_NhomBenh into ulist3
                             from b in ulist3.DefaultIfEmpty()
                             select new CapPhatThuoc
                             {
                                 ID_CapThuoc = a.ID_CapThuoc,
                                 ID_NV = (int?)a.ID_NV ?? default,
                                 MaNV = nv.MaNV,
                                 HoTen = nv.HoTen,
                                 SoDienThoai = a.SoDienThoai,
                                 ID_PhongBan = (int?)a.ID_PhongBan ?? default,
                                 TenPhongBan = pb.TenPhongBan,
                                 NgayCapThuoc = (DateTime?)a.NgayCapThuoc ?? default,
                                 ThoiGianDen = a.ThoiGianDen,
                                 ThoiGianDi = a.ThoiGianDi,
                                 SoPhutLuuLai = a.SoPhutLuuLai,
                                 ID_NhomBenh = (int?)a.ID_NhomBenh ?? default,
                                 TenNhomBenh = b.TenNhomBenh,
                                 GhiChu = a.GhiChu
                             }).ToListAsync();

            CapPhatThuoc DO = new CapPhatThuoc();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID_CapThuoc = a.ID_CapThuoc;
                    DO.ID_NV = a.ID_NV;
                    DO.SoDienThoai = a.SoDienThoai;
                    DO.ID_PhongBan = (int?)a.ID_PhongBan ?? default;
                    DO.NgayCapThuoc = (DateTime?)a.NgayCapThuoc ?? default;
                    DO.ThoiGianDen = a.ThoiGianDen;
                    DO.ThoiGianDi = a.ThoiGianDi;
                    DO.SoPhutLuuLai = a.SoPhutLuuLai;
                    DO.ID_NhomBenh = (int?)a.ID_NhomBenh ?? default;
                    DO.GhiChu = a.GhiChu;
                }

                var NhanVien = (from nv in _context.NhanVien
                                select new NhanVien
                                {
                                    ID_NV = (int)nv.ID_NV,
                                    HoTen = nv.MaNV + " : " + nv.HoTen
                                }).ToList();
                ViewBag.NVList = new SelectList(NhanVien, "ID_NV", "HoTen", DO.ID_NV);


                //List<NhanVien> nv = _context.NhanVien.ToList();
                //ViewBag.NVList = new SelectList(nv, "ID_NV", "MaNV", DO.ID_NV);

                List<PhongBan> pb = _context.PhongBan.ToList();
                ViewBag.PBList = new SelectList(pb, "ID_PhongBan", "TenPhongBan", DO.ID_PhongBan);

                List<NhomBenh> mb = _context.NhomBenh.ToList();
                ViewBag.MBList = new SelectList(mb, "ID_NhomBenh", "TenNhomBenh", DO.ID_NhomBenh);

                DateTime NgayCap = (DateTime)DO.NgayCapThuoc;
                ViewBag.NgayCapThuoc = NgayCap.ToString("yyyy-MM-dd");
            }
            else
            {
                return NotFound();
            }



            return PartialView(DO);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CapPhatThuoc _DO)
        {
            try
            {

                var result = _context.Database.ExecuteSqlRaw("EXEC CapPhatThuoc_update {0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                                                            _DO.ID_CapThuoc, _DO.ID_NV, _DO.ID_PhongBan, _DO.SoDienThoai, _DO.NgayCapThuoc, _DO.ThoiGianDen, _DO.ThoiGianDi, _DO.SoPhutLuuLai, _DO.ID_NhomBenh, _DO.GhiChu);

                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";
            }


            return RedirectToAction("Index", "CapPhatThuoc", new { id = _DO.ID_CapThuoc });
        }
        public async Task<IActionResult> duLieuQuetCong(string? search, DateTime? date, int page = 1)
        {
            date = date ?? DateTime.Now;
            DateTime st = date?.Date ?? DateTime.Now.Date;
            DateTime ed = st.AddDays(1).AddTicks(-1);
            List<TENTERCV> employees = new List<TENTERCV>();
            List<CapPhatThuoc> dataCapThuoc = new List<CapPhatThuoc>();

            try
            {
                // Truy vấn dữ liệu CapPhatThuoc từ database
                var capthuoc = await (from a in _context.CapPhatThuoc
                                      where a.NgayCapThuoc >= st && a.NgayCapThuoc <= ed
                                      join nv in _context.NhanVien on a.ID_NV equals nv.ID_NV into nvJoin
                                      from nv in nvJoin.DefaultIfEmpty()
                                      select new CapPhatThuoc
                                      {
                                          MaNV = nv.MaNV,
                                      }).ToListAsync();

                // Truy vấn dữ liệu từ bảng Tenter trong database ORCcontext
                using (var context = new ORCcontext())
                {
                   employees = await context.Tenter
                        .FromSqlRaw(@"SELECT L_UID, L_TID, C_NAME, C_DATE, C_TIME
                      FROM Tenter 
                      WHERE L_TID = 199 AND C_DATE ='" + date.Value.ToString("yyyyMMdd") + "'AND L_UID <>-1")
                        .Select(x => new TENTERCV
                        {
                            L_UID = x.L_UID,
                            L_TID = x.L_TID,
                            C_NAME = x.C_NAME,
                            C_DATE = x.C_DATE,
                            C_TIME = x.C_TIME
                        })
                        .ToListAsync();
                }
                TENTERCV? tgden;
                TENTERCV? tgdi;
                // Lặp qua từng employee để xử lý thêm dữ liệu
                foreach (var e in employees)
                {
                    var checkTENTER = _context.Tenter.Any(x => x.C_TIME == e.C_TIME && x.L_UID == e.L_UID && x.C_DATE == e.C_DATE);
                    if (!checkTENTER)
                    {       
                        await _context.Database.ExecuteSqlRawAsync($"tenter_insert {e.L_TID}, {e.L_UID}, '{e.C_NAME}', '{e.C_DATE}', '{e.C_TIME}'");
                    }

                    var maNVString = e.L_UID.ToString().PadLeft(5, '0');
                      if (capthuoc.Any(x => x.MaNV != null && x.MaNV.Contains(maNVString) ) || dataCapThuoc.Any(x => x.MaNV != null && x.MaNV.Contains(maNVString)))
                                            {
                        continue; // Nếu MaNV đã có trong danh sách, bỏ qua
                    }

                    var nhanvien = await (from nv in _context.NhanVien
                                          join pb in _context.PhongBan on nv.ID_PhongBan equals pb.ID_PhongBan
                                          where nv.MaNV == $"HPDQ{maNVString}"
                                          select new CapPhatThuoc
                                          {
                                              MaNV = nv.MaNV,
                                              HoTen = nv.HoTen,
                                              ID_NV = nv.ID_NV,
                                              ID_PhongBan = pb.ID_PhongBan,
                                              TenPhongBan = pb.TenPhongBan,
                                              GhiChu = ""
                                          }).SingleOrDefaultAsync();


                    tgden = employees?.Where(x => x.L_UID == e.L_UID).FirstOrDefault();
                    tgdi = employees?.Where(x => x.L_UID == e.L_UID).LastOrDefault();

                    int? phut = (tgden == null || tgdi == null || (tgden.C_DATE.Equals(tgdi.C_DATE) && tgden.C_TIME.Equals(tgdi.C_TIME))
                        ? null
                        : (int?)(DateTime.ParseExact($"{tgdi.C_DATE} {tgdi.C_TIME}", "yyyyMMdd HHmmss", CultureInfo.InvariantCulture)
                                  - DateTime.ParseExact($"{tgden.C_DATE} {tgden.C_TIME}", "yyyyMMdd HHmmss", CultureInfo.InvariantCulture)).TotalMinutes);

                    dataCapThuoc.Add(new CapPhatThuoc()
                    {
                        MaNV = nhanvien?.MaNV,
                        HoTen = nhanvien?.HoTen,
                        TenPhongBan = nhanvien?.TenPhongBan,
                        ID_PhongBan = nhanvien?.ID_PhongBan,
                        NgayCapThuoc = date,
                        ThoiGianDen = tgden == null ? "" : $"{tgden?.C_TIME.Substring(0, 2)}h{tgden?.C_TIME.Substring(2, 2)}",
                        ThoiGianDi = (tgdi == null || (tgden != null && tgden.C_DATE.Equals(tgdi.C_DATE)) && tgden.C_TIME.Substring(0, 4).Equals(tgdi.C_TIME.Substring(0, 4))) ? "" : $"{tgdi.C_TIME.Substring(0, 2)}h{tgdi.C_TIME.Substring(2, 2)}",
                        SoPhutLuuLai = (phut != 0 && phut != null) ? phut.ToString() : "",
                        ID_NV = nhanvien?.ID_NV
                    });
                }

                // Lọc theo search nếu có
                dataCapThuoc = (!string.IsNullOrEmpty(search)) ? dataCapThuoc.Where(x => x.MaNV.ToLower().Contains(search.Trim().ToLower()) || x.HoTen.ToLower().Contains(search.ToLower().Trim())).ToList() : dataCapThuoc;

                // Phân trang
                const int pageSize = 20;
                if (page < 1)
                {
                    page = 1;
                }

                int resCount = dataCapThuoc.Count;
                var pager = new Pager(resCount, page, pageSize);
                int recSkip = (page - 1) * pageSize;
                var data = dataCapThuoc.Skip(recSkip).Take(pager.PageSize).ToList();

                // Lấy thông tin NhomBenh và LoaiThuoc để đưa vào ViewBag
                List<NhomBenh> mb = _context.NhomBenh.ToList();
                ViewBag.MBList = new SelectList(mb, "ID_NhomBenh", "TenNhomBenh");

                List<LoaiThuoc> lt = _context.LoaiThuoc.ToList();
                ViewBag.LTList = new SelectList(lt, "ID_LoaiThuoc", "TenThuoc");

                // Cung cấp pager và thông tin tìm kiếm
                this.ViewBag.Pager = pager;
                ViewBag.search = search;
                ViewBag.st = st;
                ViewBag.ed = ed;
                
                // Trả về View với dữ liệu
                return View(data);
            }
            catch (Exception e)
            {
                return BadRequest(new { message = "Chỉnh sửa thất bại", error = e.Message });
            }
        }



        /*    private static DateTime ParseDate(string cDate)
            {
                if (DateTime.TryParseExact(cDate, "yyyyMMdd HHmmss", null, System.Globalization.DateTimeStyles.None, out DateTime result))
                {
                    return result;
                }
                else
                {
                    // Xử lý lỗi nếu C_DATE không hợp lệ, có thể trả về giá trị mặc định
                    return default(DateTime);
                }
            }*/
    }
}
