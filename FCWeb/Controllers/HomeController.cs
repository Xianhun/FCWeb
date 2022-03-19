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
                        TeamCptain = cptain
                    };
                    db.CreateTeam.Add(createTeam);
                    string Account = Session["User"].ToString();
                    string Location = db.User.Where(s => s.Account == Account).Select(s => s.Location).FirstOrDefault();
                    int Age = db.User.Where(s => s.Account == Account).Select(s => s.Age).FirstOrDefault();
                    var result = db.User.Where(s => s.Account == Account).FirstOrDefault();
                    if(result.TeamName!=null)
                    {
                        throw new Exception("您已加入球队！");
                    }
                    result.TeamName = TeamName;
                    db.Entry(result).State = EntityState.Modified;
                    db.SaveChanges();
                    Session["TeamName"] = TeamName;
                    TeamMembers teamMembers = new TeamMembers
                    {
                        TeamName = TeamName,
                        Account = Account,
                        UserName = cptain,
                        Position = "队长",
                        Location = Location,
                        Age= Age,
                        Appearance = 0,
                        Cost = 0,
                        Attendance = "0",
                        B_Appointment = "0",
                        LeaveRate = "0",
                        LastAttendance = "无",
                        DateTimes = DateTime.Now
                    };
                    db.TeamMember.Add(teamMembers);
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
                string TeamOpenType = db.CreateTeam.Where(s => s.ID == id).Select(s => s.TeamOpenType).FirstOrDefault();
                if (teamName==null)
                {
                    throw new Exception("球队不存在");
                }
                var teamCaptain = db.CreateTeam.Where(s => s.ID == id).ToList();
                var numberCount = db.User.Where(s => s.TeamName == teamName).Count();
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
                    string Account = Session["User"].ToString();
                    string UserName= db.User.Where(s=>s.Account==Account).Select(s=>s.UserName).FirstOrDefault();
                    string TeamName = db.CreateTeam.Where(s => s.ID == id).Select(s => s.TeamName).FirstOrDefault();
                    string TeamOpenType = db.CreateTeam.Where(s => s.ID == id).Select(s => s.TeamOpenType).FirstOrDefault();
                    string Sex = db.User.Where(s => s.Account == Account).Select(s => s.Sex).FirstOrDefault();
                    string Location = db.User.Where(s => s.Account == Account).Select(s => s.Location).FirstOrDefault();
                    int Age = db.User.Where(s => s.Account == Account).Select(s => s.Age).FirstOrDefault();
                    var result = db.User.Where(s => s.Account == Account).FirstOrDefault();
                    if (result.TeamName != null)
                    {
                        throw new Exception("您已加入球队！");
                    }
                    else if (TeamOpenType == "任何人可以加入")
                    {
                        result.TeamName = TeamName;
                        db.Entry(result).State = EntityState.Modified;
                        TeamMembers teamMembers = new TeamMembers
                        {
                            TeamName = TeamName,
                            Account = Account,
                            UserName = UserName,//UserName,
                            Position = "队员",
                            Location = Location,//个人信息中心补充
                            Age = Age,
                            Sex = Sex,
                            Cost = 0,
                            Appearance = 0,
                            Attendance = "0",
                            B_Appointment = "0",
                            LeaveRate = "0",
                            LastAttendance = "无",
                            DateTimes=DateTime.Now
                        };
                        db.TeamMember.Add(teamMembers);
                        db.SaveChanges();
                        Session["TeamName"] = TeamName;
                        string script = String.Format("<script>alert('成功加入球队');location.href='{0}'</script>", Url.Action("Index", "Home/JoinTeam"));
                        return Content(script, "text/html");
                    }
                    else if (TeamOpenType == "需要申请加入")
                    {
                        ApplicationForm applicationForm = new ApplicationForm
                        {
                            TeamName = TeamName,
                            UserName = UserName,
                            ApplicationStatus = "申请中"
                        };
                        db.ApplicationForms.Add(applicationForm);
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