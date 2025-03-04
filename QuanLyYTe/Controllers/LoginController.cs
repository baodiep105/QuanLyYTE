using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using QuanLyYTe.Models;
using QuanLyYTe.Repositorys;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;
using System.Security.Claims;
using QuanLyYTe.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace QuanLyYTe.Controllers
{
    public class LoginController : Controller
    {
        private readonly DataContext _context;

        public LoginController(DataContext _context)
        {
            this._context = _context;
        }
        public async Task<IActionResult> Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> LoginUser(TaiKhoan u)
        {
            if (u.TenDangNhap != "" && u.MatKhau != "" && u.TenDangNhap != null && u.MatKhau != null)
            {
                string mk = Common.Encryptor.MD5Hash(u.MatKhau);
                TaiKhoan? user = _context.TaiKhoan?.Where(x => x.TenDangNhap == u.TenDangNhap && x.MatKhau == mk && x.IsLock == 1)?.FirstOrDefault();
                if (user != null)
                {
                    var identity = new ClaimsIdentity(new[] {
                            new Claim(ClaimTypes.Name, user?.TenDangNhap)
                        }, CookieAuthenticationDefaults.AuthenticationScheme);

                    var principal = new ClaimsPrincipal(identity);

                    var login = HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    var ct_tk = _context.TaiKhoan.ToList();
                    ViewData["TaiKhoan"] = ct_tk;

                    return RedirectToAction("Index","Home");

                }
                else
                {
                    TempData["msglg"] = "<script>alert('Tài khoản hoặc mật khẩu không đúng, liên hệ B.CNTT nếu bạn quên mật khẩu')</script>";
                    return RedirectToAction("", "Login");
                }
            }
            else
            {
                TempData["msglg"] = "<script>alert('Vui lòng nhập tài khoản và mật khẩu')</script>";
                return RedirectToAction("", "Login");
            }
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Login");

        }
        public ActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ChangePassword(TaiKhoan u)
        {
            var TenDangNhap = User.FindFirstValue(ClaimTypes.Name);
            string mk = Encryptor.MD5Hash(u.MatKhauCu);
            var user = _context.TaiKhoan.Where(x => x.MatKhau == mk && x.TenDangNhap == TenDangNhap).FirstOrDefault();
            if (user != null)
            {
                if (u.MatKhau != u.NhapLaiMatKhau)
                {
                    TempData["msgSuccess"] = "<script>alert('Nhập lại mật khẩu mới không đúng');</script>";
                }
                else
                {
                    mk = Encryptor.MD5Hash(u.MatKhau);
                    var result = _context.Database.ExecuteSqlRaw("EXEC TaiKhoan_update_tk {0},{1}", user.ID_TK, mk);

                    TempData["msgSuccess"] = "<script>alert('Thay đổi mật khẩu thành công');</script>";

                }
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["msgSuccess"] = "<script>alert('Mật khẩu cũ không đúng, vui lòng nhập lại');</script>";
                return RedirectToAction("ChangePassword", "Login");
            }

        }
    }
}
