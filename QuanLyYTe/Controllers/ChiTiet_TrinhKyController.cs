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
    public class ChiTiet_TrinhKyController : Controller
    {
        private readonly DataContext _context;

        public ChiTiet_TrinhKyController(DataContext _context)
        {
            this._context = _context;
        }
        public async Task<IActionResult> Index(int id, int page = 1)
        
        {
            var check = _context.TrinhKy.Where(x=>x.ID_TK == id).FirstOrDefault();

            var res = await (from a in _context.KSK_BenhNgheNghiep.Where(x => x.ID_PhongBan == check.ID_PhongBan)
                             join nv in _context.NhanVien on a.ID_NV equals nv.ID_NV
                             join bp in _context.PhongBan on a.ID_PhongBan equals bp.ID_PhongBan
                             join k in _context.KipLamViec on nv.ID_Kip equals k.ID_Kip into ulist3
                             from k in ulist3.DefaultIfEmpty()
                             join vt in _context.ViTriLamViec on nv.ID_ViTri equals vt.ID_ViTri into ulist4
                             from vt in ulist4.DefaultIfEmpty()
                             join vtld in _context.ViTriLaoDong on a.ID_ViTriLaoDong equals vtld.ID_ViTriLaoDong into ulist5
                             from vtld in ulist5.DefaultIfEmpty()
                             select new KSK_BenhNgheNghiep
                             {
                                 ID_KSK_BNN = a.ID_KSK_BNN,
                                 ID_NV = (int)a.ID_NV,
                                 MaNV = nv.MaNV,
                                 HoTen = nv.HoTen,
                                 NgaySinh = (DateTime?)nv.NgaySinh ?? default,
                                 ID_PhongBan = (int?)a.ID_PhongBan??default,
                                 TenPhongBan = bp.TenPhongBan,
                                 TenKip = k.TenKip,
                                 TenViTri = vt.TenViTri,
                                 NgayKham = (DateTime?)a.NgayKham ?? default,
                                 NgayLenDanhSach = (DateTime?)a.NgayLenDanhSach ?? default,
                                 ID_ViTriLaoDong = (int)a.ID_ViTriLaoDong,
                                 TenViTriLaoDong = vtld.TenViTriLaoDong,
                                 GhiChu = a.GhiChu,
                                 ID_PheDuyet = (int?)a.ID_PheDuyet ?? default
                             }).ToListAsync();
            res = res.Where(x => x.NgayLenDanhSach == check.NgayTrinhKy).ToList();
            ViewBag.ID_TK = id;
            ViewBag.ID_PB = check.ID_PhongBan;
            var ct_nd = _context.CT_KSK_BenhNgheNghiep.ToList();
            ViewData["CT_KSK_BenhNgheNghiep"] = ct_nd;
            var ct_vt = _context.ViTriLaoDong.ToList();
            ViewData["ViTriLaoDong"] = ct_vt;
            var ct_tk = _context.TrinhKy.ToList();
            ViewData["TrinhKy"] = ct_tk;
            var ct_nv = _context.NhanVien.ToList();
            ViewData["NhanVien"] = ct_nv;
            var ct_bp = _context.PhongBan.ToList();
            ViewData["PhongBan"] = ct_bp;
            var ct_vtlv = _context.ViTriLamViec.ToList();
            ViewData["ViTriLamViec"] = ct_vtlv;
            var ct_ck = _context.TaiKhoan.ToList();
            ViewData["TaiKhoan"] = ct_ck;
            const int pageSize = 3000;
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
        private List<KSK_BenhNgheNghiep> GetDemarcation(int? ID_TK)
        {
            var check = _context.TrinhKy.Where(x => x.ID_TK == ID_TK).FirstOrDefault();
            var res = (from a in _context.KSK_BenhNgheNghiep.Where(x=>x.ID_PhongBan == check.ID_PhongBan && x.NgayLenDanhSach == check.NgayTrinhKy)
                            join nv in _context.NhanVien on a.ID_NV equals nv.ID_NV
                            join bp in _context.PhongBan on nv.ID_PhongBan equals bp.ID_PhongBan
                            join k in _context.KipLamViec on nv.ID_Kip equals k.ID_Kip into ulist3
                            from k in ulist3.DefaultIfEmpty()
                            join vt in _context.ViTriLamViec on nv.ID_ViTri equals vt.ID_ViTri into ulist4
                            from vt in ulist4.DefaultIfEmpty()
                            join vtld in _context.ViTriLaoDong on a.ID_ViTriLaoDong equals vtld.ID_ViTriLaoDong into ulist5
                            from vtld in ulist5.DefaultIfEmpty()
                            select new KSK_BenhNgheNghiep
                            {
                                ID_KSK_BNN = a.ID_KSK_BNN,
                                ID_NV = (int)a.ID_NV,
                                MaNV = nv.MaNV,
                                HoTen = nv.HoTen,
                                NgaySinh = (DateTime?)nv.NgaySinh ?? default,
                                NgayNhanViec =(DateTime?)nv.NgayVaoLam,
                                TenPhongBan = bp.TenPhongBan,
                                TenKip = k.TenKip,
                                TenViTri = vt.TenViTri,
                                NgayKham = (DateTime?)a.NgayKham ?? default,
                                NgayLenDanhSach = (DateTime?)a.NgayLenDanhSach ?? default,
                                ID_ViTriLaoDong = (int)a.ID_ViTriLaoDong,
                                TenViTriLaoDong = vtld.TenViTriLaoDong,
                                GhiChu = a.GhiChu,
                                ID_PheDuyet = (int?)a.ID_PheDuyet ?? default

                            }).ToList();
            return res;
        }
        public async Task<IActionResult> ExportToExcel(int? ID_TK)
        {
            string Ten_CBNV = "";
            try
            {
                string fileNamemau = AppDomain.CurrentDomain.DynamicDirectory + @"App_Data\BM_KSK_BNN.xlsx";
                string fileNamemaunew = AppDomain.CurrentDomain.DynamicDirectory + @"App_Data\BM_KSK_BNN_Temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNamemau);
                IXLWorksheet Worksheet = Workbook.Worksheet("BNN");
                List<KSK_BenhNgheNghiep> Data = GetDemarcation(ID_TK);
                int row = 6, stt = 0, icol = 1;

                List<string> ChiTieu = new List<string>();
                List<string> NoiDung = new List<string>();
                foreach (var item in Data)
                {
                    var check_ = _context.CT_KSK_BenhNgheNghiep.Where(x => x.ID_KSK_BNN == item.ID_KSK_BNN).ToList();
                    foreach (var ad in check_)
                    {
                        ChiTieu.Add(ad.TenChiTieu);
                        NoiDung.Add(ad.TenNoiDung);
                    }
                }
                List<string> Distinct_ChiTieu = ChiTieu.Distinct().ToList();
                int count_chitieu = Distinct_ChiTieu.Count();
                List<string> Distinct_NoiDung = NoiDung.Distinct().ToList();
                int count_noidung = Distinct_NoiDung.Count();

                if (Data.Count() > 0)
                {
                    Worksheet.Range(Worksheet.Cell(5, 10), Worksheet.Cell(5, (10+ count_chitieu) - 1)).Merge();
                    Worksheet.Range(Worksheet.Cell(5, 10), Worksheet.Cell(5, (10 + count_chitieu) - 1)).Value = "Chỉ tiêu quan trắc môi trường cần khám BNN";
                    Worksheet.Range(Worksheet.Cell(5, 10), Worksheet.Cell(5, (10 + count_chitieu) - 1)).Style.Font.SetFontSize(13);
                    Worksheet.Range(Worksheet.Cell(5, 10), Worksheet.Cell(5, (10 + count_chitieu) - 1)).Style.Font.Bold = true;
                    int icol_ct = 10;
                    foreach(var ct in Distinct_ChiTieu)
                    {
                        Worksheet.Cell(6, icol_ct).Value = ct;
                        Worksheet.Cell(6, icol_ct).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(6, icol_ct).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(6, icol_ct).Style.Alignment.WrapText = true;
                        icol_ct ++;
                    }    
                    Worksheet.Range(Worksheet.Cell(5, (11 + count_chitieu) - 1), Worksheet.Cell(5, ((11 + count_chitieu) + count_noidung) - 2)).Merge();
                    Worksheet.Range(Worksheet.Cell(5, (11 + count_chitieu) - 1), Worksheet.Cell(5, ((11 + count_chitieu) + count_noidung) - 2)).Value = "Nội dung khám phát hiện bệnh nghề nghiệp";
                    Worksheet.Range(Worksheet.Cell(5, (11 + count_chitieu) - 1), Worksheet.Cell(5, ((11 + count_chitieu) + count_noidung) - 2)).Style.Font.SetFontSize(13);
                    Worksheet.Range(Worksheet.Cell(5, (11 + count_chitieu) - 1), Worksheet.Cell(5, ((11 + count_chitieu) + count_noidung) - 2)).Style.Font.Bold = true;

                    int icol_nd = ((11 + count_chitieu) - 1);
                    foreach (var nd in Distinct_NoiDung)
                    {
                        Worksheet.Cell(6, icol_nd).Value = nd;
                        Worksheet.Cell(6, icol_nd).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(6, icol_nd).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(6, icol_nd).Style.Alignment.WrapText = true;
                        icol_nd ++;
                    }


                    Worksheet.Range(Worksheet.Cell(3, 1), Worksheet.Cell(3, ((11 + count_chitieu) + count_noidung) - 2)).Merge();
                    Worksheet.Range(Worksheet.Cell(3, 1), Worksheet.Cell(3, ((11 + count_chitieu) + count_noidung) - 2)).Value = "DANH SÁCH CBNV KHÁM BỆNH NGHỀ NGHIỆP";
                    Worksheet.Range(Worksheet.Cell(3, 1), Worksheet.Cell(3, ((11 + count_chitieu) + count_noidung) - 2)).Style.Font.SetFontSize(18);
                    Worksheet.Range(Worksheet.Cell(3, 1), Worksheet.Cell(3, ((11 + count_chitieu) + count_noidung) - 2)).Style.Font.Bold = true;

                    Worksheet.Range(Worksheet.Cell(4, 1), Worksheet.Cell(4, ((11 + count_chitieu) + count_noidung) - 2)).Merge();
                    Worksheet.Range(Worksheet.Cell(4, 1), Worksheet.Cell(4, ((11 + count_chitieu) + count_noidung) - 2)).Value = "";
                    Worksheet.Range(Worksheet.Cell(4, 1), Worksheet.Cell(4, ((11 + count_chitieu) + count_noidung) - 2)).Style.Font.SetFontSize(18);
                    Worksheet.Range(Worksheet.Cell(4, 1), Worksheet.Cell(4, ((11 + count_chitieu) + count_noidung) - 2)).Style.Font.Bold = true;


                    foreach (var item in Data)
                    {

                        row++; stt++; icol = 1;

                        Worksheet.Cell(row, icol).Value = stt;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                        icol++;

                        Worksheet.Cell(row, icol).Value = item.TenViTriLaoDong;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        icol++;
                        Worksheet.Cell(row, icol).Value = item.MaNV;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                        Ten_CBNV = item.MaNV;
                        icol++;
                        Worksheet.Cell(row, icol).Value = item.HoTen;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                        icol++;
                        Worksheet.Cell(row, icol).Value = item.NgaySinh;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                        Worksheet.Cell(row, icol).Style.DateFormat.Format = "dd-MM-yyyy";


                        icol++;
                        Worksheet.Cell(row, icol).Value = item.TenViTri;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                        icol++;
                        Worksheet.Cell(row, icol).Value = item.TenKip;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        icol++;
                        Worksheet.Cell(row, icol).Value = item.NgayNhanViec;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                        Worksheet.Cell(row, icol).Style.DateFormat.Format = "dd-MM-yyyy";


                        foreach (var Chitieu in Distinct_ChiTieu)
                        {
                            var check_ct = _context.CT_KSK_BenhNgheNghiep.Where(x => x.ID_KSK_BNN == item.ID_KSK_BNN && x.TenChiTieu == Chitieu).Distinct().FirstOrDefault();
                            if( check_ct != null)
                            {
                                icol++;
                                Worksheet.Cell(row, icol + 1).Value = "X";
                            }
                            else
                            {
                                icol++;
                                Worksheet.Cell(row, icol + 1).Value = " ";
                            }
                         
                            Worksheet.Cell(row, icol + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol + 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol + 1).Style.Alignment.WrapText = true;

                        }
                        
                        foreach(var Noidung in Distinct_NoiDung)
                        {
                            var check_nd = _context.CT_KSK_BenhNgheNghiep.Where(x => x.ID_KSK_BNN == item.ID_KSK_BNN && x.TenNoiDung == Noidung).FirstOrDefault();
                            if(check_nd != null)
                            {
                                icol++;
                                Worksheet.Cell(row, icol + 1).Value = "X";
                            }   
                            else
                            {

                                icol++;
                                Worksheet.Cell(row, icol + 1).Value = " ";
                            }
                            Worksheet.Cell(row, icol + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol + 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol + 1).Style.Alignment.WrapText = true;
                        }    



                    }

                    Worksheet.Range("A7:P" + (row)).Style.Font.SetFontName("Times New Roman");
                    Worksheet.Range("A7:P" + (row)).Style.Font.SetFontSize(13);

                    int iclo_in = (((11 + count_chitieu) + count_noidung) - 2);
                    Worksheet.Range(Worksheet.Cell(5, 1), Worksheet.Cell(row, iclo_in)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    Worksheet.Range(Worksheet.Cell(5, 1), Worksheet.Cell(row, iclo_in)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;


                    Workbook.SaveAs(fileNamemaunew);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNamemaunew);
                    string fileName = "Danh sách KSK_BNN - " + DateTime.Now.Date.ToString("dd/MM/yyyy") + ".xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {


                    Workbook.SaveAs(fileNamemaunew);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNamemaunew);
                    string fileName = "Danh sách KSK_BNN - " + DateTime.Now.Date.ToString("dd/MM/yyyy") + ".xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Có lỗi khi truy xuất dữ liệu. Vui lòng kiểm tra mã nhân viên: " + Ten_CBNV + "');</script>";
                return RedirectToAction("Index", "ChiTiet_TrinhKy", new { id = ID_TK});
            }
        }
    }
}
