using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FCWeb.Models
{
    public class Permissions
    {
        public int ID { get; set; }
        [Display(Name = "权限名称")]
        public string PermissionName { get; set; }
    }
}