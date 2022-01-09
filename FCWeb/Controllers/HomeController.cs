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
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult CreateTeam()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CreateTeam(string TeamName,string TeamOpenType, string TeamIntroduce,string City)
        {
            if(Session["User"]!=null)
            {
                try
                {
                    if (TeamName == null || TeamName == "")
                    {
                        throw new Exception("数据输入不完整!");
                    }
                    var cptain = Session["User"].ToString();
                    DateTime Time = DateTime.Now;
                    CreateTeams createTeam = new CreateTeams
                    {
                        TeamName = TeamName,
                        TeamOpenType = TeamOpenType,
                        TeamIntroduce = TeamIntroduce,
                        City = City,
                        CreateTime = Time,
                        Sponsors = "无",
                        TeamcCptain = cptain
                    };
                    db.CreateTeam.Add(createTeam);
                    string UserName = Session["User"].ToString();
                    var result = db.Login.Where(s => s.UserName == UserName).FirstOrDefault();
                    if(result.TeamName!=null)
                    {
                        throw new Exception("您已加入球队！");
                    }
                    result.TeamName = TeamName;
                    db.Entry(result).State = EntityState.Modified;
                    db.SaveChanges();
                    var script = String.Format("<script>alert('创建成功');location.href='{0}'</script>", Url.Action("Index", "Home/CreateTeam"));
                    return Content(script, "text/html");
                }
                catch (Exception ex)
                {
                    var script = String.Format("<script>alert('"+ex.Message.ToString()+"');location.href='{0}'</script>", Url.Action("Index", "Home/CreateTeam"));
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
                if(teamName==null)
                {
                    throw new Exception("球队不存在");
                }
                var teamCaptain = db.CreateTeam.Where(s => s.ID == id).ToList();
                var numberCount = db.Login.Where(s => s.TeamName == teamName).Count();
                ViewBag.Count = numberCount;
                return View(teamCaptain);
            }
            catch(Exception ex)
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
                    int TeamID = db.CreateTeam.Find(id).ID;
                    string UserName = Session["User"].ToString();
                    string TeamName = db.CreateTeam.Where(s => s.ID == TeamID).Select(s => s.TeamName).FirstOrDefault();
                    var result = db.Login.Where(s => s.UserName == UserName).FirstOrDefault();
                    if (result.TeamName != null)
                    {
                        throw new Exception("您已加入球队！");
                    }
                    else
                    {
                        result.TeamName = TeamName;
                        db.Entry(result).State = EntityState.Modified;
                        db.SaveChanges();
                        var script = String.Format("<script>alert('申请成功');location.href='{0}'</script>", Url.Action("Index", "Home/JoinTeam"));
                        return Content(script, "text/html");
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