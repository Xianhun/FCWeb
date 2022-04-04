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
            DateTime Now = DateTime.Now;
            TimeSpan Difference = new TimeSpan();
            List<TeamMembers> teamMembers = db.TeamMember.Where(s => s.Status == "删除中" && s.LeaveTimes < Now).ToList();
            TeamMembers D_members = new TeamMembers();
            if (teamMembers.Count != 0)
            {
                for (int i = 0; i < teamMembers.Count; i++)
                {
                    Difference= Now-teamMembers[i].LeaveTimes;
                    if(Difference.TotalSeconds>=3600)
                    {
                        int id = teamMembers[i].ID;
                        D_members=db.TeamMember.Find(id);
                        db.TeamMember.Remove(D_members);
                    }
                }
                db.SaveChanges();
            }
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
                    string Account = Session["User"].ToString();
                    string cptain = db.User.Where(s => s.Account == Account).Select(s => s.UserName).FirstOrDefault();
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
                        Age = Age,
                        Appearance = 0,
                        Cost = 0,
                        Goal=0,
                        Assists=0,
                        Permission = "球队队长",
                        Permissionid = "1",
                        PermissionStatus = "启用",
                        Attendance = "0",
                        B_Appointment = "0",
                        LeaveRate = "0",
                        LastAttendance = "无",
                        DateTimes = DateTime.Now,
                        LeaveTimes = DateTime.Now
                    };
                    db.TeamMember.Add(teamMembers);
                    db.SaveChanges();
                    var users = db.User.Where(s => s.Account == Account).FirstOrDefault();
                    users.Access = 1;
                    Session["Access"] = users.Access;
                    db.Entry(users).State = EntityState.Modified;
                    db.SaveChanges();
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
                string TeamOpenType = db.CreateTeam.Where(s => s.ID == id).Select(s => s.TeamOpenType).FirstOrDefault();
                if (teamName==null)
                {
                    throw new Exception("球队不存在");
                }
                var teamCaptain = db.CreateTeam.Where(s => s.ID == id).ToList();
                var numberCount = db.User.Where(s => s.TeamName == teamName).Count();
                var matchCount = db.Schedule.Where(s => s.TeamName == teamName && s.Status == "已结束").Count();
                ViewBag.Count = numberCount;
                ViewBag.M_Count = matchCount;
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
                            UserName = UserName,
                            Position = "队员",
                            Location = Location,
                            Age = Age,
                            Sex = Sex,
                            Cost = 0,
                            Goal = 0,
                            Assists = 0,
                            Appearance = 0,
                            Permission = null,
                            Permissionid = null,
                            PermissionStatus = "启用",
                            Attendance = "0",
                            B_Appointment = "0",
                            LeaveRate = "0",
                            LastAttendance = "无",
                            DateTimes=DateTime.Now,
                            LeaveTimes = DateTime.Now
                        };
                        db.TeamMember.Add(teamMembers);
                        db.SaveChanges();
                        Session["TeamName"] = TeamName;
                        var users = db.User.Where(s => s.Account == Account).FirstOrDefault();
                        users.Access = 1;
                        Session["Access"] = users.Access;
                        db.Entry(users).State = EntityState.Modified;
                        db.SaveChanges();
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
                        db.SaveChanges();
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