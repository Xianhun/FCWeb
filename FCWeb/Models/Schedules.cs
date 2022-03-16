using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FCWeb.Models
{
    public class Schedules
    {
        public int ID { get; set; }
        [Display(Name = "所属球队")]
        public string TeamName { get; set; }
        [Display(Name = "比赛类型")]
        public string MatchType { get; set; }
        [Display(Name = "比赛地点")]
        public string Place { get; set; }
        [Display(Name = "比赛对手")]
        public string Rival { get; set; }
        [Display(Name = "比赛费用")]
        public decimal SduFees { get; set; }
        [Display(Name = "参与人数")]
        public int Participate { get; set; }
        [Display(Name = "人数上限")]
        public int LimitNum { get; set; }
        [Display(Name = "每人费用")]
        public decimal PersonFees { get; set; }
        [Display(Name = "比赛结果")]
        public string Result { get; set; }
        [Display(Name = "比赛时间")]
        public DateTime SdulsTime { get; set; }
        [Display(Name = "备注")]
        public string Note { get; set; }
        [Display(Name = "比赛状态")]
        public string Status { get; set; }

    }
}