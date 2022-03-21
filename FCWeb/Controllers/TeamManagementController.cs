﻿using FCWeb.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FCWeb.Controllers
{
    public class TeamManagementController : Controller
    {
        FCDbContext db = new FCDbContext();
        public ActionResult Home()
        {
            var Account = Session["User"].ToString();
            var UserName = db.User.Where(s => s.Account == Account).Select(s => s.UserName).FirstOrDefault();
            ViewBag.US = UserName;
            return View();
        }
        /// <summary>
        /// 管理首页
        /// </summary>
        /// <returns></returns>
        public ActionResult Main()
        {
            try
            {
                if (Session["TeamName"].ToString() == null)
                {
                    throw new Exception("您还未加入球队");
                }
                else if (Session["User"].ToString() == null)
                {
                    throw new Exception("请登录");
                }
                else
                {
                    string User = Session["User"].ToString();
                    string TeamName = Session["TeamName"].ToString();
                    DateTime NearTime = DateTime.MinValue;//最近赛程的比赛日期
                    DateTime Now = DateTime.Now;
                    List<DateTime> dateTimes = db.Schedule.Where(s => s.SdulsTime > Now && s.TeamName == TeamName).Select(s => s.SdulsTime).ToList();
                    List<Schedules> schedules = db.Schedule.Where(s => s.SdulsTime < Now && s.TeamName == TeamName).ToList();
                    if (schedules != null)
                    {
                        Dictionary<int, List<string>> bm = new Dictionary<int, List<string>>();//<赛程id，List<报名球员用户名>>报名
                        List<int> scid = db.Schedule.Select(s => s.ID).ToList();
                        for (int i = 0; i < scid.Count; i++)
                        {
                            int SID = scid[i];
                            List<int> bmqyid = db.SignUps.Where(s => s.SchedulesID == SID && s.SignUpStatus == "报名中").Select(s => s.PlayerID).ToList();
                            List<string> bmqy = new List<string>();
                            for (int j = 0; j < bmqyid.Count; j++)
                            {
                                int qyid = bmqyid[j];
                                bmqy.Add(db.TeamMember.Where(s => s.ID == qyid).Select(s => s.UserName).FirstOrDefault());
                            }
                            bm.Add(SID, bmqy);
                        }
                        ViewBag.bm = bm;
                        for (int i = 0; i < schedules.Count; i++)
                        {
                            var stu = schedules[i];
                            stu.Status = "已结束";
                            db.Entry(stu).State = EntityState.Modified;
                        }
                        db.SaveChanges();
                    }
                    bool result = false;
                    if (dateTimes != null)
                    {
                        foreach (DateTime x in dateTimes)
                        {
                            if (result)
                            {
                                if (x < NearTime)
                                {
                                    NearTime = x;
                                }
                            }
                            else
                            {
                                NearTime = x;
                                result = true;
                            }
                        }
                    }
                    var Near_Sedul = db.Schedule.Where(s => s.SdulsTime == NearTime).ToList();
                    ViewBag.Check = result;//判断最近是否有赛程
                    return View(Near_Sedul);
                }
            }
            catch(Exception ex)
            {
                var script = String.Format("<script>alert('" + ex.Message.ToString() + "');location.href='{0}'</script>", Url.Action("Index", "Home/Index"));
                return Content(script, "text/html");
            }
        }
        /// <summary>
        /// 添加比赛
        /// </summary>
        /// <returns></returns>
        public ActionResult MatchCreate()
        {
            return View();
        }
        [HttpPost]
        public ActionResult MatchCreate(string MatchType,DateTime Date,DateTime Time,string Place,string Rival, decimal SduFees, decimal PersonFees, int LimitNum,string Note)
        {
            try
            {
                if(Session["TeamName"].ToString()==null)
                {
                    var script = String.Format("<script>alert('请重新登录');location.href='{0}'</script>", Url.Action("Index", "Account/Login"));
                    return Content(script, "text/html");
                }
                string TeamName = Session["TeamName"].ToString();
                string date = Date.ToShortDateString().ToString();//获取日期
                string time = Time.ToShortTimeString().ToString();//获取时间
                DateTime datetime = Convert.ToDateTime(date + ' ' + time);//获取日期时间
                if (datetime > DateTime.Now)
                {
                    Schedules createMatch = new Schedules
                    {
                        TeamName = TeamName,
                        MatchType = MatchType,
                        SdulsTime = datetime,
                        Place = Place,
                        Rival = Rival,
                        SduFees = SduFees,
                        PersonFees = PersonFees,
                        LimitNum = LimitNum,
                        Note = Note,
                        Status = "报名中"
                    };
                    db.Schedule.Add(createMatch);
                    db.SaveChanges();
                    var script = String.Format("<script>alert('添加成功');location.href='{0}'</script>", Url.Action("Index", "TeamManagement/MatchSchedule"));
                    return Content(script, "text/html");
                }
                else
                {
                    var script = String.Format("<script>alert('新增赛程日期必须大于现在的时间');location.href='{0}'</script>", Url.Action("Index", "TeamManagement/MatchCreate"));
                    return Content(script, "text/html");
                }
            }
            catch(Exception)
            {
                var script = String.Format("<script>alert('请输入正确数据');location.href='{0}'</script>", Url.Action("Index", "TeamManagement/MatchCreate"));
                return Content(script, "text/html");
            }
        }
        public ActionResult MatchSchedule()
        {
            string TeamName = Session["TeamName"].ToString();
            List<Schedules> schedules = db.Schedule.Where(s=>s.TeamName==TeamName).OrderByDescending(s=>s.ID).ToList();
            if(schedules!=null)
            {
                return View(schedules);
            }
            return View();
        }
        public ActionResult MatchInformation(int id)
        {
            try
            {
                string TeamName = Session["TeamName"].ToString();
                List<Schedules> schedules = db.Schedule.Where(s => s.TeamName == TeamName && s.ID == id).ToList();
                if (schedules != null)
                {
                    Dictionary<int, List<string>> bm = new Dictionary<int, List<string>>();//<赛程id，List<报名球员用户名>>报名
                    List<int> scid = db.Schedule.Select(s => s.ID).ToList();
                    for (int i = 0; i < scid.Count; i++)
                    {
                        int SID = scid[i];
                        List<int> bmqyid = db.SignUps.Where(s => s.SchedulesID == SID && s.SignUpStatus == "报名中").Select(s => s.PlayerID).ToList();
                        List<string> bmqy = new List<string>();
                        for (int j = 0; j < bmqyid.Count; j++)
                        {
                            int qyid = bmqyid[j];
                            bmqy.Add(db.TeamMember.Where(s => s.ID == qyid).Select(s => s.UserName).FirstOrDefault());
                        }
                        bm.Add(SID, bmqy);
                    }
                    ViewBag.bm = bm;
                    return View(schedules);
                }
                else
                {
                    throw new Exception("赛程不存在");
                }
            }
            catch(Exception ex){
                var script = String.Format("<script>alert('" + ex.Message.ToString() + "');location.href='{0}'</script>", Url.Action("Index", "TeamManagement/MatchSchedule"));
                return Content(script, "text/html");
            }
        }

        [HttpPost]
        public ActionResult MatchInformation(int id, EventArgs e)
        {
            try
            {
                string UserID = Session["User"].ToString();
                string TeamName = Session["TeamName"].ToString();
                int PlayerID = db.TeamMember.Where(s => s.Account == UserID && s.TeamName == TeamName).Select(s => s.ID).FirstOrDefault();
                //报名人数
                int Participate = db.Schedule.Where(s => s.ID == id).Select(s => s.Participate).FirstOrDefault();
                Schedules pt = db.Schedule.Where(s => s.ID == id).FirstOrDefault();
                //人数上限
                int LimitNum = db.Schedule.Where(s => s.ID == id).Select(s => s.LimitNum).FirstOrDefault();
                //用于判断是否重复报名
                List<SignUp> PID = db.SignUps.Where(s => s.PlayerID == PlayerID && s.SchedulesID == id).ToList();
                if (Participate < LimitNum)
                {
                    if(PID.Count == 0)
                    {
                        SignUp Play_Sign_Up = new SignUp
                        {
                            SchedulesID = id,
                            PlayerID = PlayerID,
                            SignUpStatus = "报名中"
                        };
                        db.SignUps.Add(Play_Sign_Up);
                        db.SaveChanges();
                        //重新报名人数统计
                        pt.Participate = db.SignUps.Where(s => s.SignUpStatus == "报名中" && s.SchedulesID == id).Count();
                        db.Entry(pt).State = EntityState.Modified;
                        var cost = db.TeamMember.Where(s => s.Account == UserID).Select(s => s.Cost).FirstOrDefault();
                        var person_cost = db.Schedule.Where(s => s.ID == id).Select(s => s.PersonFees).FirstOrDefault();
                        var costup = cost + person_cost;
                        TeamMembers teamMembers = new TeamMembers();
                        teamMembers = db.TeamMember.Where(s => s.Account == UserID && s.TeamName == TeamName).FirstOrDefault();
                        teamMembers.Cost = costup;
                        db.Entry(teamMembers).State = EntityState.Modified;
                        db.SaveChanges();

                        var script = String.Format("<script>alert('报名成功');location.href='{0}'</script>", Url.Action("Index", "TeamManagement/MatchSchedule"));
                        return Content(script, "text/html");
                    }
                    else
                    {
                        throw new Exception("请勿重复报名");
                    }
                }
                else
                {
                    throw new Exception("报名已超过人数上限");
                }
            }
            catch (Exception ex)
            {
                var script = String.Format("<script>alert('" + ex.Message.ToString() + "');location.href='{0}'</script>", Url.Action("Index", "TeamManagement/MatchSchedule"));
                return Content(script, "text/html");
            }
        }
        public ActionResult TeamMember()
        {
            try
            {
                string TeamName = Session["TeamName"].ToString();
                int total_match = db.Schedule.Where(s => s.TeamName == TeamName&&s.Status=="已结束").Count();
                List<TeamMembers> teammember = db.TeamMember.Where(s => s.TeamName == TeamName).ToList();
                ViewBag.TM = total_match;
                return View(teammember);
            }
            catch (Exception ex)
            {
                var script = String.Format("<script>alert('" + ex.Message.ToString() + "');location.href='{0}'</script>", Url.Action("Index", "Home/Index"));
                return Content(script, "text/html");
            }
        }

        public ActionResult MemberDetailed(int id)
        {
            string TeamName = Session["TeamName"].ToString();
            List<TeamMembers> teammember = db.TeamMember.Where(s => s.TeamName == TeamName&&s.ID==id).ToList();
            return View(teammember);
        }
        public ActionResult Application()
        {
            string TeamName = Session["TeamName"].ToString();
            List<ApplicationForm> applicationForms = db.ApplicationForms.Where(s => s.TeamName == TeamName && s.ApplicationStatus=="申请中").ToList();
            List<Users> userinFormation = new List<Users>();
            for (int i=0;i<applicationForms.Count;i++)
            {
                string UserName = applicationForms[i].UserName;
                var Account= db.User.Where(s => s.UserName == UserName).Select(s => s.Account).FirstOrDefault();
                userinFormation.Add(db.User.Where(s => s.Account == Account).FirstOrDefault());
            }
            return View(userinFormation);
        }
        public ActionResult ApplicationDetailed(int id)
        {
            string TeamName = Session["TeamName"].ToString();
            List<Users> userinFormation=db.User.Where(s => s.ID == id).ToList();
            ViewBag.ID = id;
            return View(userinFormation);
        }
        [HttpPost]
        public ActionResult ApplicationDetailed(int id, EventArgs e)
        {
            string TeamName = Session["TeamName"].ToString();
            string Account = db.User.Where(s => s.ID == id).Select(s => s.Account).FirstOrDefault();
            string UserName = db.User.Where(s => s.ID == id).Select(s => s.UserName).FirstOrDefault();
            string Sex = db.User.Where(s => s.ID == id).Select(s => s.Sex).FirstOrDefault();
            string Location = db.User.Where(s => s.ID == id).Select(s => s.Location).FirstOrDefault();
            TeamMembers teamMembers = new TeamMembers
            {
                TeamName = TeamName,
                Account = Account,
                UserName = UserName,
                Position = "队员",
                Location = Location,
                Sex = Sex,
                Cost = 0,
                Appearance = 0,
                Attendance = "0",
                B_Appointment = "0",
                LeaveRate = "0",
                LastAttendance = "无",
                DateTimes = DateTime.Now
            };
            db.TeamMember.Add(teamMembers);
            var FormID = db.ApplicationForms.Where(s => s.UserName == UserName).Select(s => s.ID).FirstOrDefault();
            ApplicationForm Form = db.ApplicationForms.Find(FormID);
            db.ApplicationForms.Remove(Form);
            db.SaveChanges();
            var script = String.Format("<script>alert('申请通过');location.href='{0}'</script>", Url.Action("Index", "TeamManagement/Application"));
            return Content(script, "text/html");
        }
        [HttpPost]
        public ActionResult Delete(int id)
        {
            ApplicationForm Form = new ApplicationForm();
            var UserName = db.User.Where(s => s.ID == id).Select(s => s.UserName).FirstOrDefault();
            Form = db.ApplicationForms.Where(s => s.UserName == UserName).FirstOrDefault();
            Form.ApplicationStatus = "已拒绝";
            db.Entry(Form).State = EntityState.Modified;
            db.SaveChanges();
            var script = String.Format("<script>alert('已拒绝');location.href='{0}'</script>", Url.Action("Index", "TeamManagement/Application"));
            return Content(script, "text/html");
        }
        
        public ActionResult Createplayers()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Createplayers(string Name,string Location,int Age,string Sex)
        {
            string TeamName = Session["TeamName"].ToString();
            TeamMembers teamMembers = new TeamMembers
            {
                TeamName = TeamName,
                Account =null,
                UserName = Name,
                Position = "临时",
                Location = Location,
                Sex = Sex,
                Cost = 0,
                Appearance = 0,
                Attendance = "0",
                B_Appointment = "0",
                LeaveRate = "0",
                LastAttendance = "无",
                DateTimes = DateTime.Now
            };
            db.TeamMember.Add(teamMembers);
            db.SaveChanges();
            var script = String.Format("<script>alert('添加成功！');location.href='{0}'</script>", Url.Action("Index", "TeamManagement/Application"));
            return Content(script, "text/html");
        }
        public ActionResult TeamInformation()
        {
            string TeamName = Session["TeamName"].ToString();
            var TeamInformation = db.CreateTeam.Where(s => s.TeamName == TeamName).ToList();
            var numberCount = db.User.Where(s => s.TeamName == TeamName).Count();
            var matchCount = db.Schedule.Where(s => s.TeamName == TeamName&&s.Status=="已结束").Count();
            ViewBag.Count = numberCount;
            ViewBag.M_Count = matchCount;
            return View(TeamInformation);
        }

        public ActionResult ModifyInformation()
        {
            string TeamName = Session["TeamName"].ToString();
            var team_Id = db.CreateTeam.Where(s => s.TeamName == TeamName).Select(s => s.ID).FirstOrDefault();
            CreateTeams TeamInformation = db.CreateTeam.Find(team_Id);
            var numberCount = db.User.Where(s => s.TeamName == TeamName).Count();
            var matchCount = db.Schedule.Where(s => s.TeamName == TeamName && s.Status == "已结束").Count();
            ViewBag.Count = numberCount;
            ViewBag.M_Count = matchCount;
            return View(TeamInformation);
        }
        [HttpPost]
        public ActionResult ModifyInformation(string Open,string City,string Sponsors,string TeamIntroduce, string Rule)
        {
            string TeamName = Session["TeamName"].ToString();
            CreateTeams TeamInformation = db.CreateTeam.Where(s => s.TeamName == TeamName).FirstOrDefault();
            TeamInformation.TeamOpenType = Open;
            TeamInformation.City = City;
            TeamInformation.Sponsors = Sponsors;
            TeamInformation.TeamIntroduce = TeamIntroduce;
            TeamInformation.Rule = Rule;
            db.Entry(TeamInformation).State = EntityState.Modified;
            db.SaveChanges();
            var script = String.Format("<script>alert('修改成功！');location.href='{0}'</script>", Url.Action("Index", "TeamManagement/TeamInformation"));
            return Content(script, "text/html");
        }
    }
}