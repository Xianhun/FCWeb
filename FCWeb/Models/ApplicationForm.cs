using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FCWeb.Models
{
    /// <summary>
    /// 球队申请
    /// </summary>
    public class ApplicationForm
    {
        public int ID { get; set; }
        [Display(Name = "球队名称")]
        public string TeamName { get; set; }
        [Display(Name = "用户名")]
        public string UserName { get; set; }
        [Display(Name = "申请状态")]
        public string ApplicationStatus { get; set; }
    }
}