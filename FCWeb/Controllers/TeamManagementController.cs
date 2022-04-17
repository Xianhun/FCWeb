using FCWeb.Models;
using OAWeb.ECharts;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
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
            var UserID = db.TeamMember.Where(s => s.Account == Account).Select(s => s.ID).FirstOrDefault();
            string per_id = db.TeamMember.Where(s => s.Account == Account).Select(s => s.Permissionid).FirstOrDefault();
            if (per_id == "1")
            {
                ViewBag.Per = "管理权限";
            }
            else
            {
                ViewBag.Per = "普通队员";
            }
            ViewBag.US = UserName;
            ViewBag.ID = UserID;
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
                    TimeSpan Difference = new TimeSpan();
                    List<TeamMembers> teamMembers = db.TeamMember.Where(s => s.Status == "删除中" && s.LeaveTimes < Now).ToList();
                    TeamMembers D_members = new TeamMembers();
                    if (teamMembers.Count != 0)
                    {
                        for (int i = 0; i < teamMembers.Count; i++)
                        {
                            Difference = Now - teamMembers[i].LeaveTimes;
                            if (Difference.TotalSeconds >= 3600)
                            {
                                int id = teamMembers[i].ID;
                                D_members = db.TeamMember.Find(id);
                                db.TeamMember.Remove(D_members);
                            }
                        }
                        db.SaveChanges();
                    }
                    List<DateTime> dateTimes = db.Schedule.Where(s => s.SdulsTime > Now && s.TeamName == TeamName).Select(s => s.SdulsTime).ToList();
                    List<Schedules> schedules = db.Schedule.Where(s => s.SdulsTime < Now && s.TeamName == TeamName && s.Status!="已结算").ToList();
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
                    var Near_Sedul = db.Schedule.Where(s => s.SdulsTime == NearTime && s.TeamName == TeamName).ToList();
                    ViewBag.Check = result;//判断最近是否有赛程
                    return View(Near_Sedul);
                }
            }
            catch (Exception ex)
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
            if (Access_permissions(2) == "管理权限")
            {
                return View();
            }
            else
            {
                var script = String.Format("<script>alert('你未被管理员赋予此权限');history.back(-1);</script>");
                return Content(script, "text/html");
            }
        }
        [HttpPost]
        public ActionResult MatchCreate(string MatchType, DateTime Date, DateTime Time, string Place, string Rival, decimal SduFees, decimal PersonFees, int LimitNum, string Note)
        {
            try
            {
                if (Session["TeamName"].ToString() == null)
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
            catch (Exception)
            {
                var script = String.Format("<script>alert('请输入正确数据');location.href='{0}'</script>", Url.Action("Index", "TeamManagement/MatchCreate"));
                return Content(script, "text/html");
            }
        }
        public ActionResult MatchSchedule()
        {
            string TeamName = Session["TeamName"].ToString();
            List<Schedules> schedules = db.Schedule.Where(s => s.TeamName == TeamName).OrderByDescending(s => s.ID).ToList();
            if (schedules != null)
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
            catch (Exception ex)
            {
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
                decimal Cost = db.Schedule.Where(s => s.ID == id).Select(s => s.PersonFees).FirstOrDefault();
                //用于判断是否重复报名
                List<SignUp> PID = db.SignUps.Where(s => s.PlayerID == PlayerID && s.SchedulesID == id).ToList();
                if (Participate < LimitNum)
                {
                    if (PID.Count == 0)
                    {
                        SignUp Play_Sign_Up = new SignUp
                        {
                            SchedulesID = id,
                            PlayerID = PlayerID,
                            Cost = Cost,
                            Goal = 0,
                            Assists = 0,
                            SignUpStatus = "报名中"
                        };
                        db.SignUps.Add(Play_Sign_Up);
                        db.SaveChanges();
                        //重新报名人数统计
                        SignUpCount(id, pt);
                        //重新统计花费
                        CostTotal(PlayerID);
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
        /// <summary>
        /// 球员列表
        /// </summary>
        /// <returns></returns>
        public ActionResult TeamMember()
        {
            try
            {
                string TeamName = Session["TeamName"].ToString();
                int total_match = db.Schedule.Where(s => s.TeamName == TeamName && s.Status == "已结算").Count();
                List<TeamMembers> teammember = db.TeamMember.Where(s => s.TeamName == TeamName).ToList();
                var PlayeridCount = teammember.Select(s => s.ID).ToList();
                for (int i = 0; i < PlayeridCount.Count; i++)
                {
                    int Playerid = PlayeridCount[i];
                    //重新统计花费
                    CostTotal(Playerid);
                    //重新统计进球数
                    GoalTotal(Playerid);
                    //重新统计助攻数
                    AssistsTotal(Playerid);
                    //重新统计出勤率
                    AttendanceTotal(Playerid);
                    //重新统计请假率
                    LeaveRateTotal(Playerid);
                    //重新统计鸽子率
                    AbsentTotal(Playerid);
                    //重新出场次数
                    Appearances(Playerid);

                }
                ViewBag.TM = total_match;
                return View(teammember);
            }
            catch (Exception ex)
            {
                var script = String.Format("<script>alert('" + ex.Message.ToString() + "');location.href='{0}'</script>", Url.Action("Index", "Home/Index"));
                return Content(script, "text/html");
            }
        }
        /// <summary>
        /// 球员详细信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult MemberDetailed(int id)
        {
            string TeamName = Session["TeamName"].ToString();
            List<TeamMembers> teammember = db.TeamMember.Where(s => s.TeamName == TeamName && s.ID == id).ToList();
            return View(teammember);
        }
        /// <summary>
        /// 申请列表
        /// </summary>
        /// <returns></returns>
        public ActionResult Application()
        {
            string TeamName = Session["TeamName"].ToString();
            List<ApplicationForm> applicationForms = db.ApplicationForms.Where(s => s.TeamName == TeamName && s.ApplicationStatus == "申请中").ToList();
            List<Users> userinFormation = new List<Users>();
            for (int i = 0; i < applicationForms.Count; i++)
            {
                string UserName = applicationForms[i].UserName;
                var Account = db.User.Where(s => s.UserName == UserName).Select(s => s.Account).FirstOrDefault();
                userinFormation.Add(db.User.Where(s => s.Account == Account).FirstOrDefault());
            }
            if (Access_permissions(5) == "管理权限")
            {
                return View(userinFormation);
            }
            else
            {
                var script = String.Format("<script>alert('你未被管理员赋予此权限');history.back(-1);</script>");
                return Content(script, "text/html");
            }
        }
        public ActionResult ApplicationDetailed(int id)
        {
            string TeamName = Session["TeamName"].ToString();
            List<Users> userinFormation = db.User.Where(s => s.ID == id).ToList();
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
                Goal = 0,
                Assists = 0,
                Appearance = 0,
                Permission = null,
                Permissionid = null,
                Attendance = "0",
                B_Appointment = "0",
                LeaveRate = "0",
                LastAttendance = "无",
                Status = null,
                DateTimes = DateTime.Now,
                LeaveTimes = DateTime.Now

            };
            db.TeamMember.Add(teamMembers);
            var FormID = db.ApplicationForms.Where(s => s.UserName == UserName).Select(s => s.ID).FirstOrDefault();
            ApplicationForm Form = db.ApplicationForms.Find(FormID);
            db.ApplicationForms.Remove(Form);
            db.SaveChanges();
            var users = db.User.Where(s => s.Account == Account).FirstOrDefault();
            users.Access = -1;
            db.Entry(users).State = EntityState.Modified;
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
            if (Access_permissions(4) == "管理权限")
            {
                return View();
            }
            else
            {
                var script = String.Format("<script>alert('你未被管理员赋予此权限');history.back(-1);</script>");
                return Content(script, "text/html");
            }
        }
        [HttpPost]
        public ActionResult Createplayers(string Name, string Location, int Age, string Sex)
        {
            string TeamName = Session["TeamName"].ToString();
            TeamMembers teamMembers = new TeamMembers
            {
                TeamName = TeamName,
                Account = null,
                UserName = Name,
                Position = "临时",
                Location = Location,
                Age = Age,
                Sex = Sex,
                Cost = 0,
                Goal = 0,
                Assists = 0,
                Appearance = 0,
                Permission = null,
                Permissionid = null,
                PermissionStatus = "禁用",
                Attendance = "0",
                B_Appointment = "0",
                LeaveRate = "0",
                LastAttendance = "无",
                Status = null,
                DateTimes = DateTime.Now,
                LeaveTimes = DateTime.Now

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
            //出场统计
            var matchCount = db.Schedule.Where(s => s.TeamName == TeamName && s.Status == "已结算").Count();
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
            var matchCount = db.Schedule.Where(s => s.TeamName == TeamName && s.Status == "已结算").Count();
            ViewBag.Count = numberCount;
            ViewBag.M_Count = matchCount;
            if (Access_permissions(7) == "管理权限")
            {
                return View(TeamInformation);
            }
            else
            {
                var script = String.Format("<script>alert('你未被管理员赋予此权限');history.back(-1);</script>");
                return Content(script, "text/html");
            }
        }
        [HttpPost]
        public ActionResult ModifyInformation(string Open, string City, string Sponsors, string TeamIntroduce, string Rule)
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

        public ActionResult MemberDelete()
        {
            try
            {
                string TeamName = Session["TeamName"].ToString();
                List<TeamMembers> teammember = db.TeamMember.Where(s => s.TeamName == TeamName).ToList();
                if (Access_permissions(6) == "管理权限")
                {
                    return View(teammember);
                }
                else
                {
                    var script = String.Format("<script>alert('你未被管理员赋予此权限');history.back(-1);</script>");
                    return Content(script, "text/html");
                }
            }
            catch (Exception ex)
            {
                var script = String.Format("<script>alert('" + ex.Message.ToString() + "');location.href='{0}'</script>", Url.Action("Index", "Home/Index"));
                return Content(script, "text/html");
            }
        }
        public ActionResult DeleteMember(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                var TeamName = Session["TeamName"].ToString();
                TeamMembers members = db.TeamMember.Find(id);
                members.Status = "删除中";
                members.LeaveTimes = DateTime.Now;
                db.Entry(members).State = EntityState.Modified;
                db.SaveChanges();
                var script = String.Format("<script>alert('一个小时内自动删除，期间可撤销删除');location.href='{0}'</script>", Url.Action("Index", "TeamManagement/MemberDelete"));
                return Content(script, "text/html");
            }
            catch (Exception ee)
            {
                var script = String.Format("<script>alert('删除失败" + ee.Message.ToString() + "');location.href='{0}'</script>", Url.Action("Index", "TeamManagement/MemberDelete"));
                return Content(script, "text/html");
            }
        }
        public ActionResult Undo(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                var TeamName = Session["TeamName"].ToString();
                TeamMembers members = db.TeamMember.Find(id);
                members.Status = null;
                db.Entry(members).State = EntityState.Modified;
                db.SaveChanges();
                var script = String.Format("<script>alert('撤销成功');location.href='{0}'</script>", Url.Action("Index", "TeamManagement/MemberDelete"));
                return Content(script, "text/html");
            }
            catch (Exception ee)
            {
                var script = String.Format("<script>alert('撤销失败" + ee.Message.ToString() + "');location.href='{0}'</script>", Url.Action("Index", "TeamManagement/MemberDelete"));
                return Content(script, "text/html");
            }
        }

        public ActionResult PlayerAdd()
        {
            string TeamName = Session["TeamName"].ToString();
            List<Schedules> schedules = db.Schedule.Where(s => s.TeamName == TeamName).OrderByDescending(s => s.ID).ToList();
            if (Access_permissions(3) == "管理权限")
            {
                if (schedules != null)
                {
                    return View(schedules);
                }
                else
                {
                    return View();
                }
            }
            else
            {
                var script = String.Format("<script>alert('你未被管理员赋予此权限');history.back(-1);</script>");
                return Content(script, "text/html");
            }
        }

        public ActionResult BatchCreate(int id)
        {
            string TeamName = Session["TeamName"].ToString();
            List<string> teamMember = db.TeamMember.Where(s => s.TeamName == TeamName).Select(s => s.UserName).ToList();
            List<Schedules> schedules = db.Schedule.Where(s => s.TeamName == TeamName && s.ID == id).ToList();
            Dictionary<int, List<string>> bm = new Dictionary<int, List<string>>();//<赛程id，List<报名球员用户名>>报名
            List<int> scid = db.Schedule.Select(s => s.ID).ToList();
            bool t = true;
            for (int i = 0; i < scid.Count; i++)
            {
                int SID = scid[i];
                List<int> bmqyid = db.SignUps.Where(s => s.SchedulesID == SID && s.SignUpStatus == "报名中").Select(s => s.PlayerID).ToList();
                List<string> bmqy = new List<string>();
                for (int j = 0; j < teamMember.Count; j++)
                {
                    for (int k = 0; k < bmqyid.Count; k++)
                    {
                        int qyid = bmqyid[k];
                        string qyNmae = db.TeamMember.Where(s => s.ID == qyid).Select(s => s.UserName).FirstOrDefault();
                        if (teamMember[j] == qyNmae)
                        {
                            t = false;
                        }
                    }
                    if (t)
                    {
                        bmqy.Add(teamMember[j]);
                    }
                    t = true;
                }
                bm.Add(SID, bmqy);
            }
            ViewBag.ID = id;
            ViewBag.bm = bm;
            return View(schedules);
        }
        public JsonResult BatchAdd(string sel_memberInfo, int sche_id)
        {
            var signup = new SignUp();
            Schedules pt = db.Schedule.Where(s => s.ID == sche_id).FirstOrDefault();
            string[] memberinfo = sel_memberInfo.Split(',');
            string res = "";
            for (int i = 0; i < memberinfo.Length - 1; i++)
            {
                string[] member = memberinfo[i].Split('+');
                string m = member[0];
                int p_id = db.TeamMember.Where(s => s.UserName == m).Select(s => s.ID).FirstOrDefault();
                decimal Cost = db.Schedule.Where(s => s.ID == sche_id).Select(s => s.PersonFees).FirstOrDefault();
                signup.SchedulesID = sche_id;
                signup.PlayerID = p_id;
                signup.Cost = Cost;
                signup.Goal = 0;
                signup.Assists = 0;
                signup.SignUpStatus = "报名中";
                db.SignUps.Add(signup);
                db.SaveChanges();
            }
            SignUpCount(sche_id, pt);
            res += "添加成功";
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Permission()
        {
            string TeamName = Session["TeamName"].ToString();
            List<TeamMembers> teammembers = db.TeamMember.Where(s => s.TeamName == TeamName).ToList();
            return View(teammembers);
        }

        public ActionResult RoleAdd(int id)
        {
            string TeamName = Session["TeamName"].ToString();
            var teammbers = db.TeamMember.Where(s => s.TeamName == TeamName && s.ID == id).ToList();
            ViewBag.ID = teammbers.Select(s => s.Permissionid).FirstOrDefault();
            return View(teammbers);
        }

        public JsonResult PermissionAdd(string sel_permissionInfo, int member_id)
        {
            TeamMembers player = db.TeamMember.Where(s => s.ID == member_id).FirstOrDefault();
            string[] permissioninfo = sel_permissionInfo.Split(',');
            string res = "";
            string res_mission = "";
            for (int i = 0; i < permissioninfo.Length - 1; i++)
            {
                string[] permissions = permissioninfo[i].Split('+');
                string p = permissions[0];
                int permissionid = Convert.ToInt32(p);
                string qx = db.Permission.Where(s => s.ID == permissionid).Select(s => s.PermissionName).FirstOrDefault();
                res_mission += qx + ",";
            }
            player.Permissionid = sel_permissionInfo.TrimEnd(',');
            player.Permission = res_mission.TrimEnd(',');
            player.PermissionStatus = "启用";
            db.Entry(player).State = EntityState.Modified;
            db.SaveChanges();
            res += "启用成功";
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DisableAdd(int member_id)
        {
            TeamMembers player = db.TeamMember.Where(s => s.ID == member_id).FirstOrDefault();
            string res = "";
            player.PermissionStatus = "禁用";
            db.Entry(player).State = EntityState.Modified;
            db.SaveChanges();
            res += "禁用成功";
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public string Access_permissions(int id)
        {
            string TeamName = Session["TeamName"].ToString();
            string Account = Session["User"].ToString();
            var sel_permissionInfo = db.TeamMember.Where(s => s.TeamName == TeamName && s.Account == Account && s.PermissionStatus == "启用").Select(s => s.Permissionid).FirstOrDefault();
            if (sel_permissionInfo != null)
            {
                string[] permissioninfo = sel_permissionInfo.Split(',');
                for (int i = 0; i < permissioninfo.Length; i++)
                {
                    string[] permissions = permissioninfo[i].Split('+');
                    string p = permissions[0];
                    int permissionid = Convert.ToInt32(p);
                    if (id == permissionid || permissionid == 1)
                    {
                        return "管理权限";
                    }
                }
            }
            return "普通权限";
        }

        public ActionResult Statistics(int id)
        {
            if (Access_permissions(8) == "管理权限")
            {
                Dictionary<int, List<string>> bm = new Dictionary<int, List<string>>();//<赛程id，List<报名球员用户名>>报名
                List<SignUp> sp = db.SignUps.Where(s => s.SchedulesID == id && s.SignUpStatus == "报名中").ToList();
                List<string> qy_Name = new List<string>();
                List<string> Location = new List<string>();
                List<int> bmqyid = db.SignUps.Where(s => s.SchedulesID == id && s.SignUpStatus == "报名中").Select(s => s.PlayerID).ToList();
                List<int> scid = db.Schedule.Select(s => s.ID).ToList();
                string TeamName = Session["TeamName"].ToString();
                List<string> teamMember = db.TeamMember.Where(s => s.TeamName == TeamName).Select(s => s.UserName).ToList();
                for (int i = 0; i < scid.Count; i++)
                {
                    int SID = scid[i];
                    List<int> bmqyids = db.SignUps.Where(s => s.SchedulesID == SID && s.SignUpStatus == "报名中").Select(s => s.PlayerID).ToList();
                    List<string> bmqy = new List<string>();
                    for (int j = 0; j < bmqyids.Count; j++)
                    {
                        int qyid = bmqyids[j];
                        bmqy.Add(db.TeamMember.Where(s => s.ID == qyid).Select(s => s.UserName).FirstOrDefault());

                    }
                    bm.Add(SID, bmqy);
                }
                for (int k = 0; k < bmqyid.Count; k++)
                {
                    int qyid = bmqyid[k];
                    qy_Name.Add(db.TeamMember.Where(s => s.ID == qyid).Select(s => s.UserName).FirstOrDefault());
                    Location.Add(db.TeamMember.Where(s => s.ID == qyid).Select(s => s.Location).FirstOrDefault());
                }
                //未报名球员统计
                List<TeamMembers> wbmqy = UnSignUp(id, TeamName, teamMember);
                ViewBag.wbmqy = wbmqy;
                ViewBag.bm = bm;
                ViewBag.sp = sp;
                ViewBag.qy_Name = qy_Name;
                ViewBag.Location = Location;
                var sche = db.Schedule.Where(s => s.ID == id).ToList();
                return View(sche);
            }
            else
            {
                var script = String.Format("<script>alert('你未被管理员赋予此权限');history.back(-1);</script>");
                return Content(script, "text/html");
            }

        }

        public JsonResult Settlement(int S_ID)
        {
            var Status = db.Schedule.Find(S_ID);
            Status.Status = "已结算";
            db.Entry(Status).State = EntityState.Modified;
            db.SaveChanges();
            string res = "结算成功";
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        private List<TeamMembers> UnSignUp(int id, string TeamName, List<string> teamMember)
        {
            bool t = true;
            List<int> wbmqyid = db.SignUps.Where(s => s.SchedulesID == id).Select(s => s.PlayerID).ToList();
            List<TeamMembers> wbmqy = new List<TeamMembers>();
            for (int j = 0; j < teamMember.Count; j++)
            {
                string t_Member = teamMember[j];
                for (int k = 0; k < wbmqyid.Count; k++)
                {
                    int qyid = wbmqyid[k];
                    string qyNmae = db.TeamMember.Where(s => s.ID == qyid).Select(s => s.UserName).FirstOrDefault();
                    if (t_Member == qyNmae)
                    {
                        t = false;
                    }
                }
                if (t)
                {
                    wbmqy.Add(db.TeamMember.Where(s => s.UserName == t_Member && s.TeamName == TeamName).FirstOrDefault()); ;
                }
                t = true;
            }

            return wbmqy;
        }

        public JsonResult CostChanges(int S_ID, decimal Cost, int P_ID)
        {
            var costchange = db.SignUps.Where(s => s.PlayerID == P_ID && s.SchedulesID == S_ID).FirstOrDefault();
            costchange.Cost = Cost;
            db.Entry(costchange).State = EntityState.Modified;
            db.SaveChanges();
            //重新统计总花费
            CostTotal(P_ID);
            string res = "修改成功";
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GoalChanges(int S_ID, int Goal, int P_ID)
        {
            var goalchange = db.SignUps.Where(s => s.PlayerID == P_ID && s.SchedulesID == S_ID).FirstOrDefault();
            goalchange.Goal = Goal;
            db.Entry(goalchange).State = EntityState.Modified;
            db.SaveChanges();
            //重新统计进球数
            GoalTotal(P_ID);
            string res = "修改成功";
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AssistsChanges(int S_ID, int Assists, int P_ID)
        {
            var assistschange = db.SignUps.Where(s => s.PlayerID == P_ID && s.SchedulesID == S_ID).FirstOrDefault();
            assistschange.Assists = Assists;
            db.Entry(assistschange).State = EntityState.Modified;
            db.SaveChanges();
            //重新统计助攻数
            AssistsTotal(P_ID);
            string res = "修改成功";
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AddToSchedule(int id, int PlayerID)
        {
            try
            {
                string TeamName = db.TeamMember.Where(s => s.ID == PlayerID).Select(s => s.TeamName).FirstOrDefault();
                //报名人数
                int Participate = db.Schedule.Where(s => s.ID == id).Select(s => s.Participate).FirstOrDefault();
                Schedules pt = db.Schedule.Where(s => s.ID == id).FirstOrDefault();
                //人数上限
                int LimitNum = db.Schedule.Where(s => s.ID == id).Select(s => s.LimitNum).FirstOrDefault();
                decimal Cost = db.Schedule.Where(s => s.ID == id).Select(s => s.PersonFees).FirstOrDefault();
                //用于判断是否重复报名
                List<SignUp> PID = db.SignUps.Where(s => s.PlayerID == PlayerID && s.SchedulesID == id).ToList();
                if (Participate < LimitNum)
                {
                    if (PID.Count == 0)
                    {
                        SignUp Play_Sign_Up = new SignUp
                        {
                            SchedulesID = id,
                            PlayerID = PlayerID,
                            Cost = Cost,
                            Goal = 0,
                            Assists = 0,
                            SignUpStatus = "报名中"
                        };
                        db.SignUps.Add(Play_Sign_Up);
                        db.SaveChanges();
                        //重新报名人数统计
                        SignUpCount(id, pt);
                        var cost = db.SignUps.Where(s => s.SchedulesID == id && s.PlayerID == PlayerID && s.SignUpStatus == "报名中").Select(s => s.Cost).Sum();
                        TeamMembers teamMembers = new TeamMembers();
                        teamMembers = db.TeamMember.Where(s => s.ID == PlayerID && s.TeamName == TeamName).FirstOrDefault();
                        teamMembers.Cost = cost;
                        db.Entry(teamMembers).State = EntityState.Modified;
                        db.SaveChanges();

                        var script = String.Format("<script>alert('添加成功');location.href='{0}'</script>", Url.Action("Index", "TeamManagement/Statistics/" + id));
                        return Content(script, "text/html");
                    }
                    else
                    {
                        throw new Exception("重复报名");
                    }
                }
                else
                {
                    throw new Exception("报名已超过人数上限");
                }
            }
            catch (Exception ex)
            {
                var script = String.Format("<script>alert('" + ex.Message.ToString() + "');location.href='{0}'</script>", Url.Action("Index", "TeamManagement/Statistics/" + id));
                return Content(script, "text/html");
            }
        }
        public ActionResult LeaveAbsence(int id)
        {
            Dictionary<int, List<string>> qj = new Dictionary<int, List<string>>();//<赛程id，List<请假球员用户名>>报名
            List<SignUp> sp = db.SignUps.Where(s => s.SchedulesID == id && s.SignUpStatus == "请假").ToList();
            List<string> qy_Name = new List<string>();
            List<string> Location = new List<string>();
            List<int> qjqyid = db.SignUps.Where(s => s.SchedulesID == id && s.SignUpStatus == "请假").Select(s => s.PlayerID).ToList();
            List<int> scid = db.Schedule.Select(s => s.ID).ToList();
            string TeamName = Session["TeamName"].ToString();
            List<string> teamMember = db.TeamMember.Where(s => s.TeamName == TeamName).Select(s => s.UserName).ToList();
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
                qj.Add(SID, bmqy);
            }
            for (int k = 0; k < qjqyid.Count; k++)
            {
                int qyid = qjqyid[k];
                qy_Name.Add(db.TeamMember.Where(s => s.ID == qyid).Select(s => s.UserName).FirstOrDefault());
                Location.Add(db.TeamMember.Where(s => s.ID == qyid).Select(s => s.Location).FirstOrDefault());
            }
            //未报名球员统计
            List<TeamMembers> wbmqy = UnSignUp(id, TeamName, teamMember);
            ViewBag.wbmqy = wbmqy;
            ViewBag.bm = qj;
            ViewBag.sp = sp;
            ViewBag.qy_Name = qy_Name;
            ViewBag.Location = Location;
            var sche = db.Schedule.Where(s => s.ID == id).ToList();
            return View(sche);
        }
        public ActionResult Absence(int id)
        {
            Dictionary<int, List<string>> qj = new Dictionary<int, List<string>>();//<赛程id，List<请假球员用户名>>报名
            List<SignUp> sp = db.SignUps.Where(s => s.SchedulesID == id && s.SignUpStatus == "鸽子").ToList();
            List<int> gzqyid = db.SignUps.Where(s => s.SchedulesID == id && s.SignUpStatus == "鸽子").Select(s => s.PlayerID).ToList();
            List<string> qy_Name = new List<string>();
            List<string> Location = new List<string>();
            List<int> scid = db.Schedule.Select(s => s.ID).ToList();
            string TeamName = Session["TeamName"].ToString();
            List<string> teamMember = db.TeamMember.Where(s => s.TeamName == TeamName).Select(s => s.UserName).ToList();
            bool t = true;
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
                qj.Add(SID, bmqy);
            }
            for (int k = 0; k < gzqyid.Count; k++)
            {
                int qyid = gzqyid[k];
                qy_Name.Add(db.TeamMember.Where(s => s.ID == qyid).Select(s => s.UserName).FirstOrDefault());
                Location.Add(db.TeamMember.Where(s => s.ID == qyid).Select(s => s.Location).FirstOrDefault());
            }
            //未报名球员统计
            List<TeamMembers> wbmqy = UnSignUp(id, TeamName, teamMember);
            ViewBag.wbmqy = wbmqy;
            ViewBag.bm = qj;
            ViewBag.sp = sp;
            ViewBag.qy_Name = qy_Name;
            ViewBag.Location = Location;
            var sche = db.Schedule.Where(s => s.ID == id).ToList();
            return View(sche);
        }
        public JsonResult Record(int sid, int pid, string status)
        {
            Schedules pt = db.Schedule.Where(s => s.ID == sid).FirstOrDefault();
            string res = "";
            if (status == "qj")
            {
                var Record = db.SignUps.Where(s => s.SchedulesID == sid && s.PlayerID == pid).FirstOrDefault();
                Record.SignUpStatus = "请假";
                db.Entry(Record).State = EntityState.Modified;
                db.SaveChanges();
                res = "记录成功";
            }
            else if (status == "gz")
            {
                var Record = db.SignUps.Where(s => s.SchedulesID == sid && s.PlayerID == pid).FirstOrDefault();
                Record.SignUpStatus = "鸽子";
                db.Entry(Record).State = EntityState.Modified;
                db.SaveChanges();
                res = "记录成功";
            }
            else if (status == "cx")
            {
                var Record = db.SignUps.Where(s => s.SchedulesID == sid && s.PlayerID == pid).Select(s => s.ID).FirstOrDefault();
                var t = db.SignUps.Find(Record);
                db.SignUps.Remove(t);
                db.SaveChanges();
                res = "撤销成功";
            }
            else if (status == "cq")
            {
                var Record = db.SignUps.Where(s => s.SchedulesID == sid && s.PlayerID == pid).FirstOrDefault();
                Record.SignUpStatus = "报名中";
                db.Entry(Record).State = EntityState.Modified;
                db.SaveChanges();
                res = "记录成功";
            }
            else
            {
                res = "记录失败";
            }
            //报名球员统计
            SignUpCount(sid, pt);
            //费用重新统计
            CostTotal(pid);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 重新报名人数统计
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="pt"></param>
        private void SignUpCount(int sid, Schedules pt)
        {
            pt.Participate = db.SignUps.Where(s => s.SignUpStatus == "报名中" && s.SchedulesID == sid).Count();
            db.Entry(pt).State = EntityState.Modified;
            db.SaveChanges();
        }
        /// <summary>
        /// 统计球员总花费
        /// </summary>
        /// <param name="pid"></param>
        private void CostTotal(int pid)
        {
            double SignCount = db.SignUps.Join(db.Schedule, a => a.SchedulesID, b => b.ID, (a, b) => new { a, b }).Where(s => s.a.PlayerID == pid && s.a.SignUpStatus == "报名中" && s.b.Status == "已结算").Count();
            TeamMembers teamMembers = new TeamMembers();
            teamMembers = db.TeamMember.Where(s => s.ID == pid).FirstOrDefault();
            if (SignCount != 0)
            {
                var costs = db.SignUps.Join(db.Schedule, a => a.SchedulesID, b => b.ID, (a, b) => new { a, b }).Where(s => s.a.PlayerID == pid && s.a.SignUpStatus == "报名中" && s.b.Status == "已结算").Select(s => s.a.Cost).Sum();
                teamMembers.Cost = costs;
            }
            else
            {
                teamMembers.Cost = 0;
            }
            db.Entry(teamMembers).State = EntityState.Modified;
            db.SaveChanges();
        }
        /// <summary>
        /// 统计球员总进球
        /// </summary>
        /// <param name="pid"></param>
        private void GoalTotal(int pid)
        {
            double SignCount = db.SignUps.Join(db.Schedule, a => a.SchedulesID, b => b.ID, (a, b) => new { a, b }).Where(s => s.a.PlayerID == pid && s.a.SignUpStatus == "报名中" && s.b.Status == "已结算").Count();
            TeamMembers teamMembers = new TeamMembers();
            teamMembers = db.TeamMember.Where(s => s.ID == pid).FirstOrDefault();
            if (SignCount != 0)
            {
                var goal = db.SignUps.Join(db.Schedule, a => a.SchedulesID, b => b.ID, (a, b) => new { a, b }).Where(s => s.a.PlayerID == pid && s.a.SignUpStatus == "报名中" && s.b.Status == "已结算").Select(s => s.a.Goal).Sum();
                teamMembers.Goal = goal;
            }
            else
            {
                teamMembers.Goal = 0;
            }
            db.Entry(teamMembers).State = EntityState.Modified;
            db.SaveChanges();
        }
        /// <summary>
        /// 统计球员总助攻
        /// </summary>
        /// <param name="pid"></param>
        private void AssistsTotal(int pid)
        {
            double SignCount = db.SignUps.Join(db.Schedule, a => a.SchedulesID, b => b.ID, (a, b) => new { a, b }).Where(s => s.a.PlayerID == pid && s.a.SignUpStatus == "报名中" && s.b.Status == "已结算").Count();
            TeamMembers teamMembers = new TeamMembers();
            teamMembers = db.TeamMember.Where(s => s.ID == pid).FirstOrDefault();
            if (SignCount != 0)
            {
                var assists = db.SignUps.Join(db.Schedule, a => a.SchedulesID, b => b.ID, (a, b) => new { a, b }).Where(s => s.a.PlayerID == pid && s.a.SignUpStatus == "报名中" && s.b.Status == "已结算").Select(s => s.a.Assists).Sum();
                teamMembers.Assists = assists;
            }
            else
            {
                teamMembers.Assists = 0;
            }
            db.Entry(teamMembers).State = EntityState.Modified;
            db.SaveChanges();
        }

        /// <summary>
        /// 统计球员出勤率
        /// </summary>
        /// <param name="pid"></param>
        private void AttendanceTotal(int pid)
        {
            string TeamName = db.TeamMember.Where(s => s.ID == pid).Select(s => s.TeamName).FirstOrDefault();
            double matchCount = db.Schedule.Where(s => s.TeamName == TeamName && s.Status == "已结算").Count();
            double SignCount = db.SignUps.Join(db.Schedule, a => a.SchedulesID, b => b.ID, (a, b) => new { a, b }).Where(s => s.a.PlayerID == pid && s.a.SignUpStatus == "报名中" && s.b.Status == "已结算").Count();
            TeamMembers teamMembers = new TeamMembers();
            teamMembers = db.TeamMember.Where(s => s.ID == pid).FirstOrDefault();
            if (SignCount != 0 && matchCount != 0)
            {
                var AttendanceTotal = Math.Round(SignCount / matchCount * 100, 1).ToString(); ;
                teamMembers.Attendance = AttendanceTotal;
            }
            else
            {
                teamMembers.Attendance = "0";
            }
            db.Entry(teamMembers).State = EntityState.Modified;
            db.SaveChanges();
        }

        /// <summary>
        /// 统计球员请假率
        /// </summary>
        /// <param name="pid"></param>
        private void LeaveRateTotal(int pid)
        {
            string TeamName = db.TeamMember.Where(s => s.ID == pid).Select(s => s.TeamName).FirstOrDefault();
            double SignCount = db.SignUps.Join(db.Schedule, a => a.SchedulesID, b => b.ID, (a, b) => new { a, b }).Where(s => s.a.PlayerID == pid && s.b.Status == "已结算").Count();
            double LeaveRate = db.SignUps.Join(db.Schedule, a => a.SchedulesID, b => b.ID, (a, b) => new { a, b }).Where(s => s.a.PlayerID == pid && s.a.SignUpStatus == "请假" && s.b.Status == "已结算").Count();
            TeamMembers teamMembers = new TeamMembers();
            teamMembers = db.TeamMember.Where(s => s.ID == pid).FirstOrDefault();
            if (SignCount != 0 && LeaveRate != 0)
            {
                var AttendanceTotal = Math.Round(LeaveRate / SignCount * 100, 1).ToString(); ;
                teamMembers.LeaveRate = AttendanceTotal;
            }
            else
            {
                teamMembers.LeaveRate = "0";
            }
            db.Entry(teamMembers).State = EntityState.Modified;
            db.SaveChanges();
        }

        /// <summary>
        /// 统计球员鸽子率
        /// </summary>
        /// <param name="pid"></param>
        private void AbsentTotal(int pid)
        {
            string TeamName = db.TeamMember.Where(s => s.ID == pid).Select(s => s.TeamName).FirstOrDefault();
            double SignCount = db.SignUps.Join(db.Schedule, a => a.SchedulesID, b => b.ID, (a, b) => new { a, b }).Where(s => s.a.PlayerID == pid && s.b.Status == "已结算").Count();
            double Absent = db.SignUps.Join(db.Schedule, a => a.SchedulesID, b => b.ID, (a, b) => new { a, b }).Where(s => s.a.PlayerID == pid && s.a.SignUpStatus == "鸽子" && s.b.Status == "已结算").Count();
            TeamMembers teamMembers = new TeamMembers();
            teamMembers = db.TeamMember.Where(s => s.ID == pid).FirstOrDefault();
            if (SignCount != 0 && Absent != 0)
            {
                var AttendanceTotal = Math.Round(Absent / SignCount * 100, 1).ToString(); ;
                teamMembers.B_Appointment = AttendanceTotal;
            }
            else
            {
                teamMembers.B_Appointment = "0";
            }
            db.Entry(teamMembers).State = EntityState.Modified;
            db.SaveChanges();
        }

        /// <summary>
        /// 统计球员出场次数
        /// </summary>
        /// <param name="pid"></param>
        private void Appearances(int pid)
        {
            string TeamName = db.TeamMember.Where(s => s.ID == pid).Select(s => s.TeamName).FirstOrDefault();
            int SignCount = db.SignUps.Join(db.Schedule, a => a.SchedulesID, b => b.ID, (a, b) => new { a, b }).Where(s => s.a.PlayerID == pid && s.a.SignUpStatus == "报名中" && s.b.Status == "已结算").Count();
            TeamMembers teamMembers = new TeamMembers();
            teamMembers = db.TeamMember.Where(s => s.ID == pid).FirstOrDefault();
            if (SignCount != 0)
            {
                teamMembers.Appearance = SignCount;
            }
            else
            {
                teamMembers.Appearance = 0;
            }
            db.Entry(teamMembers).State = EntityState.Modified;
            db.SaveChanges();
        }

        public ActionResult ScheduleResult(int id)
        {
            string TeamName = Session["TeamName"].ToString();
            Schedules schedule = db.Schedule.Find(id);
            schedule.MatchType = db.Schedule.Where(s => s.ID == id).Select(s => s.MatchType).FirstOrDefault();
            schedule.Place = db.Schedule.Where(s => s.ID == id).Select(s => s.Place).FirstOrDefault();
            schedule.SdulsTime = db.Schedule.Where(s => s.ID == id).Select(s => s.SdulsTime).FirstOrDefault();
            schedule.Result = db.Schedule.Where(s => s.ID == id).Select(s => s.Result).FirstOrDefault();
            schedule.Rival = db.Schedule.Where(s => s.ID == id).Select(s => s.Rival).FirstOrDefault();
            schedule.SduFees = db.Schedule.Where(s => s.ID == id).Select(s => s.SduFees).FirstOrDefault();
            schedule.PersonFees = db.Schedule.Where(s => s.ID == id).Select(s => s.PersonFees).FirstOrDefault();
            schedule.LimitNum = db.Schedule.Where(s => s.ID == id).Select(s => s.LimitNum).FirstOrDefault();
            schedule.Note = db.Schedule.Where(s => s.ID == id).Select(s => s.Note).FirstOrDefault();
            ViewBag.Date = schedule.SdulsTime.ToLongDateString().Replace("年", "-").Replace("月", "-").Replace("日", "");
            ViewBag.Time = schedule.SdulsTime.ToLongTimeString();
            return View(schedule);
        }

        [HttpPost]
        public ActionResult ScheduleResult(int id, string MatchType, DateTime Date, DateTime Time, string Place, string Rival, decimal SduFees, decimal PersonFees, int LimitNum, string Result, string Note)
        {
            try
            {
                string TeamName = db.Schedule.Where(s => s.ID == id).Select(s => s.TeamName).FirstOrDefault();
                string status = db.Schedule.Where(s => s.ID == id).Select(s => s.Status).FirstOrDefault();
                string date = Date.ToShortDateString().ToString();//获取日期
                string time = Time.ToShortTimeString().ToString();//获取时间
                DateTime datetime = Convert.ToDateTime(date + ' ' + time);//获取日期时间
                if (datetime > DateTime.Now && status == "报名中")
                {
                    Schedules Matchchange = db.Schedule.Find(id);
                    Matchchange.TeamName = TeamName;
                    Matchchange.MatchType = MatchType;
                    Matchchange.SdulsTime = datetime;
                    Matchchange.Place = Place;
                    Matchchange.Rival = Rival;
                    Matchchange.SduFees = SduFees;
                    Matchchange.PersonFees = PersonFees;
                    Matchchange.LimitNum = LimitNum;
                    Matchchange.Note = Note;
                    Matchchange.Result = Result;
                    Matchchange.Status = "报名中";
                    db.Entry(Matchchange).State = EntityState.Modified;
                    db.SaveChanges();
                    var script = String.Format("<script>alert('修改成功');location.href='{0}'</script>", Url.Action("Index", "TeamManagement/MatchSchedule"));
                    return Content(script, "text/html");
                }
                else
                {
                    var script = String.Format("<script>alert('新增赛程日期必须大于现在的时间');location.href='{0}'</script>", Url.Action("Index", "TeamManagement/ScheduleResult/" + id));
                    return Content(script, "text/html");
                }
            }
            catch (Exception)
            {
                var script = String.Format("<script>alert('请输入正确数据');location.href='{0}'</script>", Url.Action("Index", "TeamManagement/ScheduleResult/" + id));
                return Content(script, "text/html");
            }
        }
        /// <summary>
        /// 球员折线图
        /// </summary>
        /// <returns></returns>
        public ActionResult PlayerLineChart(int id)
        {
            ViewBag.Playerid = id;
            return View();
        }
        public string Reload(string PeriodTime, int Playerid)
        {
            //图例数据
            List<object> legends = new List<object>();
            //x轴数据
            List<object> xAxisData = new List<object>();
            //图表数据
            var seriesList = new List<object>();
            //图标标题
            string headline = db.TeamMember.Where(s => s.ID == Playerid).Select(s => s.UserName).FirstOrDefault();
            //图表类型
            string genre = "category";
            //时间横坐标
            int[] Time;
            //7天时间
            List<DateTime> seventime = new List<DateTime>();
            string TeamName = db.TeamMember.Where(s => s.ID == Playerid).Select(s => s.TeamName).FirstOrDefault();
            decimal sum = 0;
            if (PeriodTime == "近7天总花费")
            {
                Time = new int[7] { 1, 2, 3, 4, 5, 6, 7 };
                for (int i = 1; i <= Time.Length; i++)
                {
                    seventime.Add(DateTime.Now.AddDays(-i)); //当前时间减去天数
                    xAxisData.Add("第" + Time[i - 1] + "天");
                }
                legends.Add(headline + "花费记录");
                //查找7天赛程id
                List<int> sech_id = new List<int>();
                for (int i = seventime.Count - 1; i >= 0; i--)
                {
                    var time1 = seventime[i];
                    var time2 = time1.AddDays(1);
                    sech_id = db.Schedule.Where(s => s.SdulsTime > time1 && s.SdulsTime < time2 && s.Status == "已结算" && s.TeamName == TeamName).Select(s => s.ID).ToList();
                    //通过赛程id查找对应数据
                    List<decimal> Cost_list = new List<decimal>();
                    for (int k = 0; k < sech_id.Count; k++)
                    {
                        int s_id = sech_id[k];
                        Cost_list.Add(db.SignUps.Where(s => s.PlayerID == Playerid && s.SchedulesID == s_id).Select(s => s.Cost).FirstOrDefault());
                    }
                    sum += Cost_list.Sum();
                    var series_C = new { name = headline + "花费记录", type = genre, data = sum };
                    seriesList.Add(series_C);

                }
            }
            else
            {
                Time = new int[30];
                for (int i = 0; i < Time.Length; i++)
                {
                    Time[i] = i + 1;
                }
                for (int i = 1; i <= Time.Length; i++)
                {
                    seventime.Add(DateTime.Now.AddDays(-i)); //当前时间减去天数
                    xAxisData.Add("第" + Time[i - 1] + "天");
                }
                legends.Add(headline + "花费记录");
                //查找30天赛程id
                List<int> sech_id = new List<int>();
                for (int i = seventime.Count - 1; i >= 0; i--)
                {
                    var time1 = seventime[i];
                    var time2 = time1.AddDays(1);
                    sech_id = db.Schedule.Where(s => s.SdulsTime > time1 && s.SdulsTime < time2 && s.Status == "已结算" && s.TeamName == TeamName).Select(s => s.ID).ToList();
                    //通过赛程id查找对应数据
                    List<decimal> Cost_list = new List<decimal>();
                    for (int k = 0; k < sech_id.Count; k++)
                    {
                        int s_id = sech_id[k];
                        Cost_list.Add(db.SignUps.Where(s => s.PlayerID == Playerid && s.SchedulesID == s_id).Select(s => s.Cost).FirstOrDefault());
                    }
                    sum += Cost_list.Sum();
                    var series_C = new { name = headline + "花费记录", type = genre, data = sum };
                    seriesList.Add(series_C);

                }
            }
            return ChartOperation.ChartLineDataProcess(legends, xAxisData, seriesList, headline, "时间(天)", "花费(元)");
        }
        public ActionResult TeamLineChart()
        {
            string TeamName = Session["TeamName"].ToString();
            ViewBag.TeamName = TeamName;
            return View();
        }
        public string TeamReload(string PeriodTime, string TeamName)
        {
            //图例数据
            List<object> legends = new List<object>();
            //x轴数据
            List<object> xAxisData = new List<object>();
            //图表数据
            var seriesList = new List<object>();
            //图标标题
            string headline = TeamName;
            //图表类型
            string genre = "category";
            //时间横坐标
            int[] Time;
            decimal sum = 0;
            //7天时间
            List<DateTime> seventime = new List<DateTime>();
            if (PeriodTime == "近7天总花费")
            {
                Time = new int[7] { 1, 2, 3, 4, 5, 6, 7 };
                for (int i = 1; i <= Time.Length; i++)
                {
                    seventime.Add(DateTime.Now.AddDays(-i)); //当前时间减去天数
                    xAxisData.Add("第" + Time[i - 1] + "天");
                }
                legends.Add(headline + "花费记录");
                //查找7天赛程id
                List<decimal> sumlist = new List<decimal>();
                List<int> sech_id = new List<int>();
                for (int i = seventime.Count - 1; i >= 0; i--)
                {
                    var time1 = seventime[i];
                    var time2 = time1.AddDays(1);
                    sech_id = db.Schedule.Where(s => s.SdulsTime > time1 && s.SdulsTime < time2 && s.Status == "已结算" && s.TeamName == TeamName).Select(s => s.ID).ToList();
                    //通过赛程id查找对应数据
                    List<decimal> Cost_list = new List<decimal>();
                    for (int k = 0; k < sech_id.Count; k++)
                    {
                        int s_id = sech_id[k];
                        Cost_list.Add(db.Schedule.Where(s => s.ID == s_id).Select(s => s.SduFees).FirstOrDefault());
                    }
                    sum += Cost_list.Sum();
                    sumlist.Add(sum);
                }
                var series_C = new { name = headline + "花费记录", type = genre, data = sumlist };
                seriesList.Add(series_C);
            }
            else
            {
                Time = new int[30];
                for (int i = 0; i < Time.Length; i++)
                {
                    Time[i] = i + 1;
                }
                for (int i = 1; i <= Time.Length; i++)
                {
                    seventime.Add(DateTime.Now.AddDays(-i)); //当前时间减去天数
                    xAxisData.Add("第" + Time[i - 1] + "天");
                }
                legends.Add(headline + "花费记录");
                //查找30天赛程id
                List<decimal> sumlist = new List<decimal>();
                List<int> sech_id = new List<int>();
                for (int i = seventime.Count - 1; i >= 0; i--)
                {
                    var time1 = seventime[i];
                    var time2 = time1.AddDays(1);
                    sech_id = db.Schedule.Where(s => s.SdulsTime > time1 && s.SdulsTime < time2 && s.Status == "已结算"&&s.TeamName== TeamName).Select(s => s.ID).ToList();
                    //通过赛程id查找对应数据
                    List<decimal> Cost_list = new List<decimal>();
                    for (int k = 0; k < sech_id.Count; k++)
                    {
                        int s_id = sech_id[k];
                        Cost_list.Add(db.Schedule.Where(s => s.ID == s_id).Select(s => s.SduFees).FirstOrDefault());
                    }
                    sum += Cost_list.Sum();
                    sumlist.Add(sum);
                }
                var series_C = new { name = headline + "花费记录", type = genre, data = sumlist };
                seriesList.Add(series_C);
            }
            return ChartOperation.ChartLineDataProcess(legends, xAxisData, seriesList, headline, "时间(天)", "花费(元)");
        }
    }
}