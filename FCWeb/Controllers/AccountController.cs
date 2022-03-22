using System;
using System.Collections.Generic;
using System.Data.Entity;
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
        /// <param name="UserID">账号</param>
        /// <param name="Password">密码</param>
        /// <returns></returns>
        public JsonResult Login_Validation(string UserID, string Password)
        {
            List<string> username = new List<string>();
            string res;
            username = db.User.Select(s => s.Account).ToList();
            if (username.Exists(p => p == UserID))
            {
                string pas = db.User.Where(s => s.Account == UserID).Select(s => s.Password).FirstOrDefault();
                if (pas == Password)
                {
                    res = "登录成功";
                    Session["User"] = UserID;
                    var UserName = db.User.Where(s => s.Account == UserID).Select(s => s.UserName).FirstOrDefault();
                    Session["UserName"] = UserName;
                    string TeamName = db.User.Where(s => s.Account == UserID).Select(s => s.TeamName).FirstOrDefault();
                    var users = db.User.Where(s => s.Account == UserID).FirstOrDefault();
                    users.Status = "在线";
                    db.Entry(users).State = EntityState.Modified;
                    db.SaveChanges();
                    Session["TeamName"] = TeamName;
                    return Json(res, JsonRequestBehavior.AllowGet);
                }
            }
            res = "登录失败";
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 注册验证ajax
        /// </summary>
        /// <param name="UserID">账号</param>
        /// <param name="Password">密码</param>
        /// <returns></returns>
        public JsonResult Registered_Validation(string UserID, string Password)
        {
            List<string> User_ID = new List<string>();
            Users logs = new Users();
            string res;
            User_ID = db.User.Select(s => s.Account).ToList();
            if (!User_ID.Exists(p => p == UserID) && UserID != null)
            {
                logs.Account = UserID;
                logs.Password = Password;
                logs.Access = 1;
                logs.Sex = "男";//注册默认为男性
                db.User.Add(logs);
                db.SaveChanges();
                res = "注册成功";
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            res = "注册失败";
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Login_out()
        {
            Session["User"] = null;
            return Json(JsonRequestBehavior.AllowGet);
        }
        public ActionResult PersonalCenter()
        {
            string Account = Session["User"].ToString();
            int A_id = db.User.Where(s => s.Account == Account).Select(s => s.ID).FirstOrDefault();
            Users Personal = db.User.Find(A_id);
            Personal.UserName = db.User.Where(s=>s.Account==Account).Select(s => s.UserName).FirstOrDefault();
            Personal.Location =db.User.Where(s => s.Account == Account).Select(s => s.Location).FirstOrDefault();
            Personal.Sex = db.User.Where(s => s.Account == Account).Select(s => s.Sex).FirstOrDefault();
            Personal.Age = db.User.Where(s => s.Account == Account).Select(s => s.Age).FirstOrDefault();
            ViewBag.Sex_value = new List<SelectListItem>() {
                new SelectListItem(){Value="男",Text="男"},
                new SelectListItem(){Value="女",Text="女"}
            };
            return View(Personal);
        }
        public JsonResult Center_Validation(string UserName, string Sex, int Age, string Location)
        {
            try {
                string Account = Session["User"].ToString();
                string TeamName = db.User.Where(s => s.Account == Account).Select(s => s.TeamName).FirstOrDefault();
                int A_id = db.User.Where(s => s.Account == Account).Select(s => s.ID).FirstOrDefault();
                Users Personal = db.User.Find(A_id);
                Personal.UserName = UserName;
                Personal.Location = Location;
                Personal.Sex = Sex;
                Personal.Age = Age;
                db.Entry(Personal).State = EntityState.Modified;
                db.SaveChanges();
                string res = "保存成功";
                if(TeamName!=null)
                {
                    int T_id = db.TeamMember.Where(s => s.Account == Account&&s.TeamName==TeamName).Select(s => s.ID).FirstOrDefault();
                    TeamMembers teamMember=db.TeamMember.Find(T_id);
                    teamMember.UserName = UserName;
                    teamMember.Sex = Sex;
                    teamMember.Age = Age;
                    teamMember.Location = Location;
                    db.Entry(teamMember).State = EntityState.Modified;
                    if(teamMember.Position=="队长")
                    {
                        CreateTeams createTeams = db.CreateTeam.Where(s => s.TeamName == TeamName).FirstOrDefault();
                        createTeams.TeamCptain = UserName;
                        db.Entry(createTeams).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                }
                Session["UserName"] = UserName;
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch(Exception){
                return Json("保存失败", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult Password_Validation(string oldPassword, string newPassword, string confirmPassword)
        {
            try
            {
                string Account = Session["User"].ToString();
                int A_id = db.User.Where(s => s.Account == Account).Select(s => s.ID).FirstOrDefault();
                Users Personal = db.User.Find(A_id);
                if(Personal.Password== oldPassword)
                {
                    if(newPassword== confirmPassword)
                    {
                        Personal.Password = newPassword;
                    }
                    else
                    {
                        throw new Exception("两次密码不一致");
                    }
                }
                else
                {
                    throw new Exception("密码输入错误");
                }
                db.Entry(Personal).State = EntityState.Modified;
                db.SaveChanges();
                string res = "修改成功";
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json("修改失败", JsonRequestBehavior.AllowGet);
            }
        }
    }
}