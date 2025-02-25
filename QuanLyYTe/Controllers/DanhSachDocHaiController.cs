using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyYTe.Models;
using QuanLyYTe.Repositorys;
using Microsoft.AspNetCore.Mvc.Rendering;
using ExcelDataReader;
using System.Data;
using Microsoft.Data.SqlClient;
using DocumentFormat.OpenXml.VariantTypes;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace QuanLyYTe.Controllers
{
    public class DanhSachDocHaiController : Controller
    {
        private readonly DataContext _context;

        public DanhSachDocHaiController(DataContext _context)
        {
            this._context = _context;
        }
        public async Task<IActionResult> Index(string search, int page = 1)
        {
            var res = await (from a in _context.DanhSachDocHai
                             select new DanhSachDocHai
                             {
                                 ID_DocHai = a.ID_DocHai,
                                 TenDocHai = a.TenDocHai,
                             }).ToListAsync();
            if (search != null)
            {
                res = res.Where(x=>x.TenDocHai.ToLower().Contains(search.ToLower())).ToList();
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

        public async Task<IActionResult> Create()
        {

            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DanhSachDocHai _DO)
        {
            try
            {

                var result_dochai = _context.Database.ExecuteSqlRaw("EXEC DanhSachDocHai_insert {0}", _DO.TenDocHai);
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Thêm mới thất bại');</script>";
            }

            return RedirectToAction("Index", "DanhSachDocHai");
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";

                return RedirectToAction("Index", "DanhSachDocHai");
            }

            var res = await (from a in _context.DanhSachDocHai.Where(x=>x.ID_DocHai == id)
                             select new DanhSachDocHai
                             {
                                 ID_DocHai = a.ID_DocHai,
                                 TenDocHai = a.TenDocHai
                             }).ToListAsync();

            DanhSachDocHai DO = new DanhSachDocHai();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID_DocHai = a.ID_DocHai;
                    DO.TenDocHai = a.TenDocHai;
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
        public async Task<IActionResult> Edit(int id, int page, DanhSachDocHai _DO)
        {
            try
            {

                var result = _context.Database.ExecuteSqlRaw("EXEC DanhSachDocHai_update {0},{1}", id, _DO.TenDocHai);
                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";
            }
        

            return RedirectToAction("Index", "DanhSachDocHai", new { search = _DO.TenDocHai });
        }

         public async Task<IActionResult> Delete(int id, int page)
        {
            try
            {

                var result = _context.Database.ExecuteSqlRaw("EXEC DanhSachDocHai_delete {0}", id);

                TempData["msgSuccess"] = "<script>alert('Xóa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xóa dữ liệu thất bại');</script>";
            }
     

            return RedirectToAction("Index", "DanhSachDocHai", new { page = page });
        }

        public FileResult TestDownloadPCF()
        {
            string filePath = "Form files/BM_ChiTieuNoiDung_DocHai.xlsx";
            HttpContext.Response.ContentType = "application/xlsx";
            FileContentResult result = new FileContentResult
            (System.IO.File.ReadAllBytes(filePath), "application/xlsx")
            {
                FileDownloadName = "BM_ChiTieuNoiDung_DocHai.xlsx"
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
                    return RedirectToAction("Index", "DanhSachDocHai");
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
                        string TenDocHai = "";
                        int ID_DocHai = 0;
                        System.Data.DataTable serviceDetails = ds.Tables[0];
                        for (int i = 5; i < serviceDetails.Rows.Count; i++)
                        {
                            string check = serviceDetails.Rows[i][0].ToString();
                            if (check != "")
                            {
                                TenDocHai = serviceDetails.Rows[i][0].ToString();
                                string ChiTieu = serviceDetails.Rows[i][1].ToString();
                                string NoiDung = serviceDetails.Rows[i][2].ToString();

                                var check_dh = _context.DanhSachDocHai.Where(x => x.TenDocHai == TenDocHai).FirstOrDefault();
                                if (check_dh == null)
                                {
                                    var Output_ID_DocHai = new SqlParameter
                                    {
                                        ParameterName = "ID_DocHai",
                                        SqlDbType = System.Data.SqlDbType.Int,
                                        Direction = System.Data.ParameterDirection.Output,
                                    };
                                    var result_Vitri = _context.Database.ExecuteSqlRaw("EXEC DanhSachDocHai_insert {0},@ID_DocHai OUTPUT", TenDocHai, Output_ID_DocHai);
                                    ID_DocHai = (int)Output_ID_DocHai.Value;
                                    var result_ChiTieuNoiDung = _context.Database.ExecuteSqlRaw("EXEC ChiTieuNoiDung_insert {0},{1},{2}",
                                                                                                            ID_DocHai, ChiTieu, NoiDung);
                                }
                                else
                                {
                                    var result_Vitri = _context.Database.ExecuteSqlRaw("EXEC DanhSachDocHai_update {0},{1}", check_dh.ID_DocHai, TenDocHai);

                                    var Delete = _context.ChiTieuNoiDung.Where(x=>x.ID_DocHai == check_dh.ID_DocHai).ToList();
                                    foreach(var item in  Delete)
                                    {
                                        var result = _context.Database.ExecuteSqlRaw("EXEC ChiTieuNoiDung_delete {0}", item.ID_CTND);
                                    }    
                                    var result_ChiTieuNoiDung = _context.Database.ExecuteSqlRaw("EXEC ChiTieuNoiDung_insert {0},{1},{2}", check_dh.ID_DocHai, ChiTieu, NoiDung);

                                }
                            }    
                            else
                            {
                                string ChiTieu = serviceDetails.Rows[i][1].ToString();
                                string NoiDung = serviceDetails.Rows[i][2].ToString();

                                var check_dh = _context.DanhSachDocHai.Where(x => x.TenDocHai == TenDocHai).FirstOrDefault();
                                if (check_dh == null)
                                {
                                    var Output_ID_DocHai = new SqlParameter
                                    {
                                        ParameterName = "ID_DocHai",
                                        SqlDbType = System.Data.SqlDbType.Int,
                                        Direction = System.Data.ParameterDirection.Output,
                                    };
                                    var result_Vitri = _context.Database.ExecuteSqlRaw("EXEC DanhSachDocHai_insert {0},@ID_DocHai OUTPUT", TenDocHai, Output_ID_DocHai);
                                    var result_ChiTieuNoiDung = _context.Database.ExecuteSqlRaw("EXEC ChiTieuNoiDung_insert {0},{1},{2}",
                                                                                                            ID_DocHai, ChiTieu, NoiDung);
                                }
                                else
                                {
                                    var result_Vitri = _context.Database.ExecuteSqlRaw("EXEC DanhSachDocHai_update {0},{1}", check_dh.ID_DocHai, TenDocHai);

                                    var result_ChiTieuNoiDung = _context.Database.ExecuteSqlRaw("EXEC ChiTieuNoiDung_insert {0},{1},{2}", check_dh.ID_DocHai, ChiTieu, NoiDung);

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

            return RedirectToAction("Index", "DanhSachDocHai");
        }
    }
}
