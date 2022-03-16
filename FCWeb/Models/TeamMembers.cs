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
        [Display(Name ="加入时间")]
        public DateTime DateTimes { get; set; }


    }
}