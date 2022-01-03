using FCWeb.Models;
using System;
using System.Collections.Generic;
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
            try
            {
                if(TeamName==null||TeamName=="")
                {
                    throw new Exception();
                }
                DateTime Time = DateTime.Now;
                CreateTeams createTeam = new CreateTeams
                {
                    TeamName = TeamName,
                    TeamOpenType = TeamOpenType,
                    TeamIntroduce = TeamIntroduce,
                    City = City,
                    CreateTime = Time,
                    Sponsors ="无"
                };
                db.CreateTeam.Add(createTeam);
                db.SaveChanges();
                var script = String.Format("<script>alert('创建成功');location.href='{0}'</script>", Url.Action("Index", "Home/CreateTeam"));
                return Content(script, "text/html");
            }
            catch (Exception)
            {
                var script = String.Format("<script>alert('数据输入不完整');location.href='{0}'</script>", Url.Action("Index", "Home/CreateTeam"));
                return Content(script, "text/html");
            }
        }
        public ActionResult JoinTeam()
        {
            return View(db.CreateTeam.ToList());
        }
        public ActionResult TeamDetails()
        {
            return View();
        }
    }
}