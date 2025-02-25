using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyYTe.Models;
using QuanLyYTe.Repositorys;
using Microsoft.AspNetCore.Mvc.Rendering;
using ExcelDataReader;
using System.Data;
using Microsoft.Data.SqlClient;
using ClosedXML.Excel;
using System.Security.Claims;

namespace QuanLyYTe.Controllers
{
    public class ViTriLaoDongController : Controller
    {
        private readonly DataContext _context;

        public ViTriLaoDongController(DataContext _context)
        {
            this._context = _context;
        }
        public async Task<IActionResult> Index(string search,int page = 1)
        {

            var MNV = User.FindFirstValue(ClaimTypes.Name);
            var check = _context.TaiKhoan.Where(x => x.TenDangNhap == MNV).FirstOrDefault();
            var res = await (from a in _context.ViTriLaoDong
                             join bp in _context.PhongBan on a.ID_PhongBan equals bp.ID_PhongBan
                             select new ViTriLaoDong
                             {
                                 ID_ViTriLaoDong = a.ID_ViTriLaoDong,
                                 TenViTriLaoDong = a.TenViTriLaoDong,
                                 ID_PhongBan = (int)a.ID_PhongBan,
                                 TenPhongBan = bp.TenPhongBan
                             }).ToListAsync();
            if(check.ID_Quyen != 1 && check.ID_Quyen != 2)
            {
                res = res.Where(x => x.ID_PhongBan == check.ID_PhongBan).ToList();
            }    
            if (search != null)
            {
                res = res.Where(x=>x.TenViTriLaoDong.Contains(search)).ToList();
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
        public async Task<IActionResult> Create()
        {
            List<PhongBan> pb = _context.PhongBan.ToList();
            ViewBag.PBList = new SelectList(pb, "ID_PhongBan", "TenPhongBan");


            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ViTriLaoDong _DO)
        {
            try
            {
                var result_ViTri = _context.Database.ExecuteSqlRaw("EXEC ViTriLaoDong_insert {0},{1}",
                                 _DO.TenViTriLaoDong, _DO.ID_PhongBan);
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Thêm mới thất bại');</script>";
            }

            return RedirectToAction("Index", "ViTriLaoDong");
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";

                return RedirectToAction("Index", "KSK_TuyenDung");
            }

            var res = await (from a in _context.ViTriLaoDong.Where(x=>x.ID_ViTriLaoDong == id)
                             select new ViTriLaoDong
                             {
                                 ID_ViTriLaoDong = a.ID_ViTriLaoDong,
                                 TenViTriLaoDong = a.TenViTriLaoDong,
                                 ID_PhongBan = (int)a.ID_PhongBan
                             }).ToListAsync();

            ViTriLaoDong DO = new ViTriLaoDong();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID_ViTriLaoDong = a.ID_ViTriLaoDong;
                    DO.TenViTriLaoDong = a.TenViTriLaoDong;
                    DO.ID_PhongBan = (int)a.ID_PhongBan;
                }

                List<PhongBan> pb = _context.PhongBan.ToList();
                ViewBag.PBList = new SelectList(pb, "ID_PhongBan", "TenPhongBan", DO.ID_PhongBan);
            }
            else
            {
                return NotFound();
            }



            return PartialView(DO);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ViTriLaoDong _DO)
        {

            try
            {

                var result = _context.Database.ExecuteSqlRaw("EXEC ViTriLaoDong_update {0},{1},{2}", _DO.ID_ViTriLaoDong, _DO.TenViTriLaoDong, _DO.ID_PhongBan);

                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";
            }
           
            return RedirectToAction("Index", "ViTriLaoDong", new { search = _DO.TenViTriLaoDong });
        }

        public async Task<IActionResult> Delete(int id, int page)
        {
            try
            {

                var result = _context.Database.ExecuteSqlRaw("EXEC ViTriLaoDong_delete {0}", id);

                TempData["msgSuccess"] = "<script>alert('Xóa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xóa dữ liệu thất bại');</script>";
            }


            return RedirectToAction("Index", "ViTriLaoDong", new { page = page });
        }

        public FileResult TestDownloadPCF()
        {
            string filePath = "Form files/BM_ViTriLaoDong.xlsx";
            HttpContext.Response.ContentType = "application/xlsx";
            FileContentResult result = new FileContentResult
            (System.IO.File.ReadAllBytes(filePath), "application/xlsx")
            {
                FileDownloadName = "BM_ViTriLaoDong.xlsx"
            };
            return result;
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
                    return RedirectToAction("Index", "ViTriLaoDong");
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
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        System.Data.DataTable serviceDetails = ds.Tables[0];
                        int ViTriLaoDong_ID = 0;
                        for (int i = 5; i < serviceDetails.Rows.Count; i++)
                        {
                            string PhongBan = serviceDetails.Rows[i][0].ToString().Trim();

                            var check_phongban = _context.PhongBan.Where(x => x.TenPhongBan == PhongBan).FirstOrDefault();
                            if (check_phongban == null)
                            {
                                TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại BP/NM: " + PhongBan + "');</script>";
                                return RedirectToAction("Index", "ViTriLaoDong");
                            }
                            string Vitri = serviceDetails.Rows[i][1].ToString().Trim();
                            var check_ = _context.ViTriLaoDong.Where(x => x.TenViTriLaoDong == Vitri && x.ID_PhongBan == check_phongban.ID_PhongBan).FirstOrDefault();
                            if(check_ == null)
                            {
                                var Output_ID_ViTriLaoDong = new SqlParameter
                                {
                                    ParameterName = "ID_ViTriLaoDong",
                                    SqlDbType = System.Data.SqlDbType.Int,
                                    Direction = System.Data.ParameterDirection.Output,
                                };
                                var result_ViTri = _context.Database.ExecuteSqlRaw("EXEC ViTriLaoDong_insert_all {0},{1},@ID_ViTriLaoDong OUTPUT",
                                    Vitri, check_phongban.ID_PhongBan, Output_ID_ViTriLaoDong);
                                int ID_ViTriLaoDong = (int)Output_ID_ViTriLaoDong.Value;
                                ViTriLaoDong_ID = ID_ViTriLaoDong;

                                string LoaiDocHai = serviceDetails.Rows[i][2].ToString().Trim();
                                var ds_dochai = new List<string>();
                                string[] arr_list = LoaiDocHai.Split(',');
                                foreach (var item in arr_list)
                                {
                                    var check_yeutodochai = _context.DanhSachDocHai.Where(x => x.TenDocHai == item.Trim()).FirstOrDefault();
                                    if (check_yeutodochai == null)
                                    {
                                        TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại yếu tố đọc hại: " + item + "');</script>";
                                        return RedirectToAction("Index", "ViTriLaoDong");
                                    }
                                    var result_DocHai = _context.Database.ExecuteSqlRaw("EXEC ChiTiet_ChiTieuNoiDung_ViTri_insert {0},{1}",
                                   ID_ViTriLaoDong, check_yeutodochai.ID_DocHai);
                                }
                            }
                            else
                            {
                                var result_ViTri = _context.Database.ExecuteSqlRaw("EXEC ViTriLaoDong_update {0},{1},{2}",
                                check_.ID_ViTriLaoDong, Vitri, check_phongban.ID_PhongBan);

                                var Delete = _context.ChiTiet_ChiTieuNoiDung_ViTri.Where(x => x.ID_ViTriLaoDong == check_.ID_ViTriLaoDong).ToList();
                                foreach(var item in  Delete)
                                {

                                    var result = _context.Database.ExecuteSqlRaw("EXEC ChiTiet_ChiTieuNoiDung_ViTri_delete {0}", item.ID_CT_ViTriLaoDong);
                                }

                                string LoaiDocHai = serviceDetails.Rows[i][2].ToString().Trim();
                                var ds_dochai = new List<string>();
                                string[] arr_list = LoaiDocHai.Split(',');
                                foreach (var item in arr_list)
                                {
                                    var check_yeutodochai = _context.DanhSachDocHai.Where(x => x.TenDocHai == item.Trim()).FirstOrDefault();
                                    if (check_yeutodochai == null)
                                    {
                                        TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại yếu tố đọc hại: " + item + "');</script>";
                                        return RedirectToAction("Index", "ViTriLaoDong");
                                    }
                                    var result_DocHai = _context.Database.ExecuteSqlRaw("EXEC ChiTiet_ChiTieuNoiDung_ViTri_insert {0},{1}",
                                   check_.ID_ViTriLaoDong, check_yeutodochai.ID_DocHai);
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

            return RedirectToAction("Index", "ViTriLaoDong");
        }

        public async Task<IActionResult> Index_(string search, int page = 1)
        {


            var res = await (from a in _context.ViTriLamViec
                             select new ViTriLamViec
                             {
                                 ID_ViTri = a.ID_ViTri,
                                 TenViTri = a.TenViTri ?? default,
                                LoaiViTri = (int?)a.LoaiViTri ?? default
                             }).ToListAsync();

            if (search != null)
            {
                res = res.Where(x => x.TenViTri.Contains(search)).ToList();
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
        public async Task<IActionResult> ExportToExcel(DateTime? begind, DateTime? endd, int? IDPhongBan)
        {
            try
            {
                string fileNamemau = AppDomain.CurrentDomain.DynamicDirectory + @"App_Data\BM_ViTri.xlsx";
                string fileNamemaunew = AppDomain.CurrentDomain.DynamicDirectory + @"App_Data\BM_ViTri_Temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNamemau);
                IXLWorksheet Worksheet = Workbook.Worksheet("VT");
                var Data = _context.ViTriLamViec.ToList();
                int row = 5, stt = 0, icol = 1;
                if (Data.Count > 0)
                {
                    foreach (var item in Data)
                    {

                        row++; stt++; icol = 1;

                        Worksheet.Cell(row, icol).Value = stt;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                        icol++;

                        Worksheet.Cell(row, icol).Value = item.TenViTri;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                    }

                    Worksheet.Range("A6:B" + (row)).Style.Font.SetFontName("Times New Roman");
                    Worksheet.Range("A6:B" + (row)).Style.Font.SetFontSize(13);
                    Worksheet.Range("A6:B" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    Worksheet.Range("A6:B" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;


                    Workbook.SaveAs(fileNamemaunew);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNamemaunew);
                    string fileName = "Danh sách vị trí nhân sự - " + DateTime.Now.Date.ToString("dd/MM/yyyy") + ".xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {


                    Workbook.SaveAs(fileNamemaunew);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNamemaunew);
                    string fileName = "Danh sách vị trí nhân sự - " + DateTime.Now.Date.ToString("dd/MM/yyyy") + ".xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index_", "ViTriLaoDong");
            }
        }


        public async Task<IActionResult> ImportExcel_LX()
        {
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportExcel_LX(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return RedirectToAction("Index_", "ViTriLaoDong");
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
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        System.Data.DataTable serviceDetails = ds.Tables[0];
                        for (int i = 5; i < serviceDetails.Rows.Count; i++)
                        {
                            string TenViTri = serviceDetails.Rows[i][1].ToString().Trim();
                            var check_vt = _context.ViTriLamViec.Where(x => x.TenViTri == TenViTri).FirstOrDefault();
                            if (check_vt == null)
                            {
                                TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại vị trí: " + TenViTri + "');</script>";

                                return RedirectToAction("Index_LX", "ViTriLaoDong");
                            }
                            var result = _context.Database.ExecuteSqlRaw("EXEC ViTriLamViec_update_laixe {0},{1}",
                                                           check_vt.ID_ViTri, 1);
                        }
                    }
                }
                TempData["msgSuccess"] = "<script>alert('Import dữ liệu thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Import dữ liệu thất bại');</script>";
            }

            return RedirectToAction("Index_LX", "ViTriLaoDong");
        }



        public async Task<IActionResult> ImportExcel_TV()
        {
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportExcel_TV(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return RedirectToAction("Index_", "ViTriLaoDong");
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
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        System.Data.DataTable serviceDetails = ds.Tables[0];
                        for (int i = 5; i < serviceDetails.Rows.Count; i++)
                        {
                            string TenViTri = serviceDetails.Rows[i][1].ToString().Trim();
                            var check_vt = _context.ViTriLamViec.Where(x => x.TenViTri == TenViTri).FirstOrDefault();
                            if (check_vt == null)
                            {
                                TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại vị trí: " + TenViTri + "');</script>";

                                return RedirectToAction("Index_LX", "ViTriLaoDong");
                            }
                            var result = _context.Database.ExecuteSqlRaw("EXEC ViTriLamViec_update_laixe {0},{1}",
                                                           check_vt.ID_ViTri, 2);
                        }
                    }
                }
                TempData["msgSuccess"] = "<script>alert('Import dữ liệu thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Import dữ liệu thất bại');</script>";
            }

            return RedirectToAction("Index_", "ViTriLaoDong");
        }
    }
}
