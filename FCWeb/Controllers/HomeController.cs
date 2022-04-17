using FCWeb.BLL;
using FCWeb.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FCWeb.Controllers
{
    public class HomeController : Controller
    {
        
        FCDbContext db = new FCDbContext();
        public ActionResult Index()
        {
            HomeBLL.DeleteChange();
            return View();
        }

        public ActionResult CreateTeam()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CreateTeam(string TeamName, string TeamOpenType, string TeamIntroduce, string City)
        {
            if (Session["User"] != null)
            {
                try
                {
                    string Account = Session["User"].ToString();
                    string UserName = db.User.Where(s => s.Account == Account).Select(s => s.UserName).FirstOrDefault();
                    string Sex = db.User.Where(s => s.Account == Account).Select(s => s.Sex).FirstOrDefault();
                    string Location = db.User.Where(s => s.Account == Account).Select(s => s.Location).FirstOrDefault();
                    int Age = db.User.Where(s => s.Account == Account).Select(s => s.Age).FirstOrDefault();
                    string Permission = "球队队长";
                    string Permissionid = "1";
                    string Position = "队长";
                    HomeBLL.TeamVerify(TeamName);
                    HomeBLL.CreateT(UserName, TeamName, TeamOpenType, TeamIntroduce, City, Account);
                    HomeBLL.TeamChange(TeamName, Account);
                    HomeBLL.MemberCreate(TeamName, Account, UserName, Location, Age, Sex,Permission, Permissionid, Position);
                    int access = HomeBLL.UserAccess(Account);
                    Session["TeamName"] = TeamName;
                    Session["Access"] = access;
                    var script = String.Format("<script>alert('创建成功');location.href='{0}'</script>", Url.Action("Index", "Home/CreateTeam"));
                    return Content(script, "text/html");
                }
                catch (Exception ex)
                {
                    var script = String.Format("<script>alert('" + ex.Message.ToString() + "');location.href='{0}'</script>", Url.Action("Index", "Home/CreateTeam"));
                    return Content(script, "text/html");
                }
            }
            else
            {
                var script = String.Format("<script>alert('请登录');location.href='{0}'</script>", Url.Action("Index", "Account/Login"));
                return Content(script, "text/html");
            }
        }

        public ActionResult JoinTeam()
        {
            return View(db.CreateTeam.ToList());
        }
        public ActionResult TeamDetails(int id)
        {
            try
            {
                var teamName = db.CreateTeam.Where(s => s.ID == id).Select(s => s.TeamName).FirstOrDefault();
                List<CreateTeams>  teamCaptain = db.CreateTeam.Where(s => s.ID == id).ToList();
                int numberCount = db.User.Where(s => s.TeamName == teamName).Count();
                int matchCount = db.Schedule.Where(s => s.TeamName == teamName && s.Status == "已结算").Count();
                HomeBLL.HomeTeamDetail(teamName);
                ViewBag.Count = numberCount;
                ViewBag.M_Count = matchCount;
                return View(teamCaptain);
            }
            catch (Exception ex)
            {
                var script = String.Format("<script>alert('" + ex.Message.ToString() + "');location.href='{0}'</script>", Url.Action("Index", "Home/JoinTeam"));
                return Content(script, "text/html");
            }
        }

        [HttpPost]
        public ActionResult TeamDetails(int id, EventArgs e)
        {
            if(Session["User"]!=null)
            {
                try
                {
                    string Account = Session["User"].ToString();
                    string UserName= db.User.Where(s=>s.Account==Account).Select(s=>s.UserName).FirstOrDefault();
                    string TeamName = db.CreateTeam.Where(s => s.ID == id).Select(s => s.TeamName).FirstOrDefault();
                    string TeamOpenType = db.CreateTeam.Where(s => s.ID == id).Select(s => s.TeamOpenType).FirstOrDefault();
                    string Sex = db.User.Where(s => s.Account == Account).Select(s => s.Sex).FirstOrDefault();
                    string Location = db.User.Where(s => s.Account == Account).Select(s => s.Location).FirstOrDefault();
                    int Age = db.User.Where(s => s.Account == Account).Select(s => s.Age).FirstOrDefault();
                    string Permission = null;
                    string Permissionid = null;
                    string Position = "队员";
                    HomeBLL.TeamChange(TeamName, Account);
                    if (TeamOpenType == "任何人可以加入")
                    {
                        HomeBLL.MemberCreate(Account, UserName, TeamName, Sex, Age, Location,Permission, Permissionid, Position);
                        int access = HomeBLL.UserAccess(Account);
                        Session["TeamName"] = TeamName;
                        Session["Access"] = access;
                        string script = String.Format("<script>alert('成功加入球队');location.href='{0}'</script>", Url.Action("Index", "Home/JoinTeam"));
                        return Content(script, "text/html");
                    }
                    else if (TeamOpenType == "需要申请加入")
                    {
                        HomeBLL.ApplicationForm(UserName, TeamName);
                        var script = String.Format("<script>alert('申请成功，请耐心等候验证');location.href='{0}'</script>", Url.Action("Index", "Home/JoinTeam"));
                        return Content(script, "text/html");
                    }
                    else
                    {
                        throw new Exception("球队已设置为不能加入");
                    }
                }
                catch(Exception ex)
                {
                    var script = String.Format("<script>alert('"+ex.Message.ToString()+"');location.href='{0}'</script>", Url.Action("Index", "Home/JoinTeam"));
                    return Content(script, "text/html");
                }
            }
            else
            {
                var script = String.Format("<script>alert('请登录');location.href='{0}'</script>", Url.Action("Index", "Account/Login"));
                return Content(script, "text/html");
            }
        }
    }
}