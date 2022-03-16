using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FCWeb.Models
{
    public class SignUp
    {
        public int ID { get; set; }
        [Display(Name = "球员ID")]
        public int PlayerID { get; set; }
        [Display(Name = "赛程ID")]
        public int SchedulesID { get; set; }
        [Display(Name ="报名状态")]
        public string SignUpStatus { get; set; }
    }
}