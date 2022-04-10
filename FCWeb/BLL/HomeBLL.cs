using FCWeb.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FCWeb.BLL
{
    public class HomeBLL
    {
        private static FCDbContext db = new FCDbContext();
        /// <summary>
        /// 1小时后成员删除执行
        /// </summary>
        public static void DeleteChange()
        {
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
        }
        /// <summary>
        /// 创建球队
        /// </summary>
        /// <param name="TeamName">球队姓名</param>
        /// <param name="TeamOpenType">球队公开类型</param>
        /// <param name="TeamIntroduce">球队简介</param>
        /// <param name="City">城市</param>
        /// <param name="Account">操作账号</param>
        /// <returns></returns>
        public static void CreateT(string UserName,string TeamName, string TeamOpenType, string TeamIntroduce, string City, string Account)
        { 
            var result = db.User.Where(s => s.Account == Account).FirstOrDefault();
            if (result.TeamName != null)
            {
                throw new Exception("您已加入球队！");
            }
            DateTime Time = DateTime.Now;
            CreateTeams createTeam = new CreateTeams
            {
                TeamName = TeamName,
                TeamOpenType = TeamOpenType,
                TeamIntroduce = TeamIntroduce,
                City = City,
                CreateTime = Time,
                Sponsors = "无",
                TeamCptain = UserName
            };
            db.CreateTeam.Add(createTeam);
            db.SaveChanges();
        }
        /// <summary>
        /// 球队信息填写验证
        /// </summary>
        /// <param name="TeamName"></param>
        public static void TeamVerify(string TeamName)
        {
            if (TeamName == null || TeamName == "")
            {
                throw new Exception("数据输入不完整!");
            }
        }
        /// <summary>
        /// 录入所属球队
        /// </summary>
        /// <param name="TeamName"></param>
        /// <param name="Account"></param>
        public static void TeamChange(string TeamName, string Account)
        {
            var result = db.User.Where(s => s.Account == Account).FirstOrDefault();
            if (result.TeamName != null)
            {
                throw new Exception("您已加入球队！");
            }
            result.TeamName = TeamName;
            db.Entry(result).State = EntityState.Modified;
            db.SaveChanges();
        }

        /// <summary>
        /// 球队成员增加
        /// </summary>
        /// <param name="TeamName"></param>
        /// <param name="Account"></param>
        /// <param name="cptain"></param>
        /// <param name="Location"></param>
        /// <param name="Age"></param>
        public static void MemberCreate(string TeamName, string Account, string UserName, string Location, int Age,string Sex,string Permission,string Permissionid,string Position)
        {
            TeamMembers teamMembers = new TeamMembers
            {
                TeamName = TeamName,
                Account = Account,
                UserName = UserName,
                Position = Position,
                Location = Location,
                Age = Age,
                Sex = Sex,
                Cost = 0,
                Goal = 0,
                Assists = 0,
                Appearance = 0,
                Permission = Permission,
                Permissionid = Permissionid,
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
        }
        /// <summary>
        /// 权限状态记录
        /// </summary>
        /// <param name="Account"></param>
        /// <returns></returns>
        public static int UserAccess(string Account)
        {
            var users = db.User.Where(s => s.Account == Account).FirstOrDefault();
            users.Access = 1;
            int access = users.Access;
            db.Entry(users).State = EntityState.Modified;
            db.SaveChanges();
            return access;
        }
        /// <summary>
        /// 球队信息状态异常状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="teamName"></param>
        public static void HomeTeamDetail(string teamName)
        {
            if (teamName == null)
            {
                throw new Exception("球队不存在");
            }
        }
        /// <summary>
        /// 球队申请记录
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="TeamName"></param>
        public static void ApplicationForm(string UserName, string TeamName)
        {
            ApplicationForm applicationForm = new ApplicationForm
            {
                TeamName = TeamName,
                UserName = UserName,
                ApplicationStatus = "申请中"
            };
            db.ApplicationForms.Add(applicationForm);
            db.SaveChanges();
        }
    }
}