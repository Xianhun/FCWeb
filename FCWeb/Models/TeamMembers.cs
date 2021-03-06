using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FCWeb.Models
{
    public class TeamMembers
    {
        public int ID { get; set; }
        [Display(Name = "所属球队")]
        public string TeamName { get; set; }
        [Display(Name = "用户ID")]
        public string Account { get; set; }
        [Display(Name = "用户名")]
        public string UserName { get; set; }
        [Display(Name = "职位")]
        public string Position { get; set; }
        [Display(Name = "年龄")]
        public int Age { get; set; }
        [Display(Name = "性别")]
        public string Sex { get; set; }
        [Display(Name = "场上位置")]
        public string Location { get; set; }
        [Display(Name = "花费")]
        public decimal Cost { get; set; }
        [Display(Name = "出场次数")]
        public int Appearance { get; set; }
        [Display(Name = "出勤率")]
        public string Attendance { get; set; }
        [Display(Name = "鸽子率")]
        public string B_Appointment { get; set; }
        [Display(Name = "请假率")]
        public string LeaveRate { get; set; }
        [Display(Name = "上次出勤")]
        public string LastAttendance { get; set; }
        [Display(Name = "球队权限")]
        public string Permission { get; set; }
        [Display(Name = "球队权限ID")]
        public string Permissionid { get; set; }
        [Display(Name = "权限状态")]
        public string PermissionStatus { get; set; }
        [Display(Name = "总进球")]
        public int Goal { get; set; }
        [Display(Name = "总助攻")]
        public int Assists { get; set; }
        [Display(Name = "加入时间")]
        public DateTime DateTimes { get; set; }
        [Display(Name = "离队时间")]
        public DateTime LeaveTimes { get; set; }
        [Display(Name = "成员状态")]
        public string Status { get; set; }



    }
}