using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FCWeb.Models
{
    public class CreateTeams
    {
        public int ID { get; set; }
        [Display(Name = "球队ID")]
        public int TeamID { get; set; }
        [Display(Name = "球队名称")]
        public string TeamName { get; set; }
        [Display(Name = "公开类型")]
        public string TeamOpenType { get; set; }
        [Display(Name = "球队简介")]
        public string TeamIntroduce { get; set; }
        [Display(Name = "城市")]
        public string City { get; set; }
        [Display(Name = "赞助商")]
        public string Sponsors { get; set; }
        [Display(Name = "创建时间")]
        public DateTime CreateTime { get; set; }

    }
}