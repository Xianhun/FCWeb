using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FCWeb.Models
{
    public class Announcements
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateTime { get; set; }
    }
}