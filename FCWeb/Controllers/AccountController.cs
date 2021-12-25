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
            if (!username.Exists(p => p == User_Name))
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

        public ActionResult SendEmial(string x)
        {
            int customerID = 1;
            string validataCode = System.Guid.NewGuid().ToString();
            try
            {
                System.Net.Mail.MailAddress from = new System.Net.Mail.MailAddress(x, "wode"); //填写电子邮件地址，和显示名称
                System.Net.Mail.MailAddress to = new System.Net.Mail.MailAddress(x, "nide"); //填写邮件的收件人地址和名称
                //设置好发送地址，和接收地址，接收地址可以是多个
                System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
                mail.From = from;
                mail.To.Add(to);
                mail.Subject = "主题内容";

                System.Text.StringBuilder strBody = new System.Text.StringBuilder();
                strBody.Append("点击下面链接激活账号，48小时生效，否则重新注册账号，链接只能使用一次，请尽快激活！</br>");
                strBody.Append("<a href='http://localhost:3210/Order/ActivePage?customerID=" + customerID + "&validataCode =" + validataCode + "'>点击这里</a></br>");

                mail.Body = strBody.ToString();
                mail.IsBodyHtml = true;//设置显示htmls
                //设置好发送邮件服务地址
                System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient();
                client.Host = "smtp.163.com";
                //填写服务器地址相关的用户名和密码信息
                client.Credentials = new System.Net.NetworkCredential("xxxxxxxx@163.com", "xxxxxx");
                //发送邮件
                client.Send(mail);
            }
            catch { }

            return new EmptyResult();
        }
    }
}