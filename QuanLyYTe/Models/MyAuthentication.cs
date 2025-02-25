namespace QuanLyYTe.Models
{
    using Microsoft.AspNetCore.Http;
    public class MyAuthentication
    {
        private readonly IHttpContextAccessor _contextAccessor;
        public MyAuthentication(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }
        //public static string Username
        //{
        //    get
        //    {
        //        try
        //        {
        //            object obj = httpContextAccessor.HttpContext.Current.User.Identity.Name;
        //            return (obj == null) ? String.Empty : (string)obj;
        //        }
        //        catch
        //        {
        //            return "";
        //        }
        //    }
        //}
    }
}
