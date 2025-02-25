using Microsoft.AspNetCore.Mvc;
using QuanLyYTe.Models;
using System.Net;
using System.Text;
using System.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using QuanLyYTe.Repositorys;
using System.Globalization;
using System.Data.Entity.Core.Objects;
using Microsoft.EntityFrameworkCore;
using static System.Net.WebRequestMethods;
using System.Text.Json.Nodes;

namespace QuanLyYTe.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly DataContext _context;

        public EmployeeController(DataContext _context)
        {
            this._context = _context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public String GetToken()
        {
            string url = "http://192.168.240.39/hoaphatdq_api/api/User/GetToken";
            string username = "HPDQ16961";
            string password = "Ldt@1001";
            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Method = "POST";
            httpRequest.ContentType = "application/json";
            var data = @"{
                          ""username"":""" + username + @""",
                          ""password"":""" + password + @"""
                        }";
            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                streamWriter.Write(data);
            }
            var token = "";
            try
            {
                WebResponse httpResponse = httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {

                    var result = streamReader.ReadToEnd();
                    JObject json = JObject.Parse(result);
                    var a = json["data"]["tokenLogin"].ToString();
                    token = a;
                }
            }
            catch (WebException webex)
            {
                WebResponse errResp = webex.Response;
                using (Stream respStream = errResp.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(respStream);
                    string text = reader.ReadToEnd();
                }
            }
            return token;
        }
        List<Employees_API.Employee> GetAPI()
        {
            /*var GetTo = GetToken();*/
            string link = "http://192.168.240.39/hpdqapi/api/HPDQ/GetEmployeeInfo";
            using (WebClient webClient = new WebClient())
            {
                /*var token = "Bearer " + GetTo;*/

                webClient.Encoding = Encoding.UTF8;
                /*webClient.Headers.Add("Authorization", token);*/
                var json = webClient.DownloadString(link);
                var list = JsonConvert.DeserializeObject<Employees_API>(json);
                return list.data;
            }

        }

        public int GetIDPhongBan(string TenPB)
        {
            var model = _context.PhongBan.Where(x => x.TenPhongBan == TenPB).FirstOrDefault();
            if (model == null)
                return 0;
            return model.ID_PhongBan;
        }

        public int GetIDPhanXuong(string TenPX)
        {
            var model = _context.PhanXuong.Where(x => x.TenPhanXuong == TenPX).FirstOrDefault();
            if (model == null)
                return 0;
            return model.ID_PhanXuong;
        }
        public int GetIDTo(string TenTo)
        {
            var model = _context.ToLamViec.Where(x => x.TenTo == TenTo).FirstOrDefault();
            if (model == null)
                return 0;
            return model.ID_To;
        }
        public int GetIDKip(string TenKip)
        {
            var model = _context.KipLamViec.Where(x => x.TenKip == TenKip).FirstOrDefault();
            if (model == null)
                return 0;
            return model.ID_Kip;
        }
        public int GetIDViTri(string TenViTri)
        {
            var model = _context.ViTriLamViec.Where(x => x.TenViTri == TenViTri).FirstOrDefault();
            if (model == null)
                return 0;
            return model.ID_ViTri;
        }

        public async Task<IActionResult> Sync()
        {
            try
            {
                string MaNV, sMaNV;
                int IDViTri, IDPhongBan, IDPhanXuong, IDTo, IDKip;
                int dtc = 0;
                string msg = "";
                List<Employees_API.Employee> listNV = GetAPI();
                var a = listNV.Where(x => x.manv == "HPDQ32828").SingleOrDefault();
                if (listNV.Count > 0)
                {
                    foreach (var item in listNV)
                    {
                        if (item.manv != null)
                        {
                            MaNV = item.manv;
                            sMaNV = MaNV.Substring(0, 4);
                            var rsnv = _context.NhanVien.Where(x => x.MaNV == MaNV).FirstOrDefault();
                            if (rsnv == null)
                            {
                                if (sMaNV == "HPDQ" && MaNV.Length == 9)
                                {
                                    ObjectParameter IDPhongBanout = new ObjectParameter("IDPhongBan", typeof(int));
                                    ObjectParameter IDPhanXuongout = new ObjectParameter("IDPhanXuong", typeof(int));
                                    ObjectParameter IDToout = new ObjectParameter("IDTo", typeof(int));
                                    ObjectParameter IDKipout = new ObjectParameter("IDKip", typeof(int));
                                    ObjectParameter IDViTriout = new ObjectParameter("IDViTri", typeof(int));

                                    IDPhongBan = GetIDPhongBan(item.phongban);
                                    if (IDPhongBan == 0)
                                    {
                                        var result = _context.Database.ExecuteSqlRaw("EXEC PhongBan_insert {0}", item.phongban);
                                        IDPhongBan = Convert.ToInt32(IDPhongBanout.Value);
                                    }

                                    IDPhanXuong = GetIDPhanXuong(item.phanxuong);
                                    if (IDPhanXuong == 0)
                                    {
                                        var result = _context.Database.ExecuteSqlRaw("EXEC PhanXuong_insert {0}", item.phanxuong);
                                        IDPhanXuong = Convert.ToInt32(IDPhanXuongout.Value);
                                    }

                                    IDTo = GetIDTo(item.tolamviec);
                                    if (IDTo == 0)
                                    {
                                        var result = _context.Database.ExecuteSqlRaw("EXEC ToLamViec_insert {0}", item.tolamviec);
                                        IDTo = Convert.ToInt32(IDToout.Value);
                                    }

                                    IDKip = GetIDKip(item.tenkip);
                                    if (IDKip == 0)
                                    {
                                        var result = _context.Database.ExecuteSqlRaw("EXEC KipLamViec_insert {0}", item.tenkip);
                                        IDKip = Convert.ToInt32(IDKipout.Value);
                                    }

                                    IDViTri = GetIDViTri(item.vitri);
                                    if (IDViTri == 0)
                                    {
                                        var result = _context.Database.ExecuteSqlRaw("EXEC ViTriLamViec_insert {0}", item.vitri);
                                        IDViTri = Convert.ToInt32(IDViTriout.Value);
                                    }
                                    var result_ = _context.Database.ExecuteSqlRaw("EXEC NhanVien_insert {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                                                            MaNV, item.hoten, item.cmnd, DateTime.ParseExact(item.ngaysinh, "dd/MM/yyyy", CultureInfo.InvariantCulture), item.diachi, DateTime.ParseExact(item.ngayvaolam, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                                                            IDPhongBan, IDPhanXuong, IDTo, IDKip, IDViTri, item.tinhtranglamviec);
                                    dtc++;
                                }
                            }
                            else
                            {
                                if (sMaNV == "HPDQ" && MaNV.Length == 9)
                                {
                                    ObjectParameter IDPhongBanout = new ObjectParameter("IDPhongBan", typeof(int));
                                    ObjectParameter IDPhanXuongout = new ObjectParameter("IDPhanXuong", typeof(int));
                                    ObjectParameter IDToout = new ObjectParameter("IDTo", typeof(int));
                                    ObjectParameter IDKipout = new ObjectParameter("IDKip", typeof(int));
                                    ObjectParameter IDViTriout = new ObjectParameter("IDViTri", typeof(int));

                                    IDPhongBan = GetIDPhongBan(item.phongban);
                                    if (IDPhongBan == 0)
                                    {
                                        var result = _context.Database.ExecuteSqlRaw("EXEC PhongBan_insert {0}", item.phongban);
                                        IDPhongBan = Convert.ToInt32(IDPhongBanout.Value);
                                    }

                                    IDPhanXuong = GetIDPhanXuong(item.phanxuong);
                                    if (IDPhanXuong == 0)
                                    {
                                        var result = _context.Database.ExecuteSqlRaw("EXEC PhanXuong_insert {0}", item.phanxuong);
                                        IDPhanXuong = Convert.ToInt32(IDPhanXuongout.Value);
                                    }

                                    IDTo = GetIDTo(item.tolamviec);
                                    if (IDTo == 0)
                                    {
                                        var result = _context.Database.ExecuteSqlRaw("EXEC ToLamViec_insert {0}", item.tolamviec);
                                        IDTo = Convert.ToInt32(IDToout.Value);
                                    }

                                    IDKip = GetIDKip(item.tenkip);
                                    if (IDKip == 0)
                                    {
                                        var result = _context.Database.ExecuteSqlRaw("EXEC KipLamViec_insert {0}", item.tenkip);
                                        IDKip = Convert.ToInt32(IDKipout.Value);
                                    }

                                    IDViTri = GetIDViTri(item.vitri);
                                    if (IDViTri == 0)
                                    {
                                        var result = _context.Database.ExecuteSqlRaw("EXEC ViTriLamViec_insert {0}", item.vitri);
                                        IDViTri = Convert.ToInt32(IDViTriout.Value);
                                    }
                                    if (rsnv.ID_PhongBan != IDPhongBan || rsnv.ID_PhanXuong != IDPhanXuong || rsnv.ID_To != IDTo || rsnv.ID_Kip != IDKip || rsnv.ID_ViTri != IDViTri || rsnv.ID_TinhTrangLamViec != item.tinhtranglamviec)
                                    {

                                        var result_ = _context.Database.ExecuteSqlRaw("EXEC NhanVien_update {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}", rsnv.ID_NV,
                                                          MaNV, item.hoten, item.cmnd, DateTime.ParseExact(item.ngaysinh, "dd/MM/yyyy", CultureInfo.InvariantCulture), item.diachi, DateTime.ParseExact(item.ngayvaolam, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                                                          IDPhongBan, IDPhanXuong, IDTo, IDKip, IDViTri, item.tinhtranglamviec);
                                    }
                                    dtc++;

                                }
                            }
                        }

                    }
                    if (dtc != 0)
                    {
                        msg = "Cập nhật thông tin được " + dtc + " nhân viên";
                    }
                    TempData["msgSuccess"] = "<script>alert('" + msg + "');</script>";
                }
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Có lỗi khi cập nhật: " + e + "');</script>";
            }

            return RedirectToAction("Index","nhanvien");
        }
    }
}
