using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FCWeb.Models
{
    public class Logins
    {
        [Key]
        public int ID { get; set; }
        [Required]
        [Display(Name = "用户名")]
        public string UserName { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }
        [Display(Name = "登录状态")]
        public string Status { get; set; }
    }
}