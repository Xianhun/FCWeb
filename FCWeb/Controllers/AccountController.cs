using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FCWeb.Models;

namespace FCWeb.Controllers
{
    public class AccountController : Controller
    {
        FCDbContext db = new FCDbContext();
        /// <summary>
        /// 登录注册页
        /// </summary>
        /// <returns></returns>
        public ActionResult Login()
        {
            return View();
        }
        /// <summary>
        /// 登录验证ajax
        /// </summary>
        /// <param name="User_Name">账号</param>
        /// <param name="Password">密码</param>
        /// <returns></returns>
        public JsonResult Login_Validation(string User_Name, string Password)
        {
            List<string> username = new List<string>();
            string res;
            username = db.Login.Select(s => s.UserName).ToList();
            if (username.Exists(p => p == User_Name))
            {
                string pas = db.Login.Where(s => s.UserName == User_Name).Select(s => s.Password).FirstOrDefault();
                if (pas == Password)
                {
                    res = "登录成功";
                    return Json(res, JsonRequestBehavior.AllowGet);
                }
            }
            res = "登录失败";
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 注册验证ajax
        /// </summary>
        /// <param name="User_Name">账号</param>
        /// <param name="Password">密码</param>
        /// <returns></returns>
        public JsonResult registered_Validation(string User_Name, string Password)
        {
            List<string> username = new List<string>();
            Logins logs = new Logins();
            string res;
            username = db.Login.Select(s => s.UserName).ToList();
            if (!username.Exists(p => p == User_Name)&&User_Name!=null)
            {
                logs.UserName = User_Name;
                logs.Password = Password;
                db.Login.Add(logs);
                db.SaveChanges();
                res = "注册成功";
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            res = "注册失败";
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        
    }
}