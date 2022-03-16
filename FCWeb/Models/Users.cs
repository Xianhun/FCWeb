using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FCWeb.Models
{
    public class Users
    {
        [Key]
        public int ID { get; set; }
        [Required]
        [Display(Name = "账号")]
        public string Account { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }
        [Display(Name ="用户名")]
        public string UserName { get; set; }
        [Display(Name = "位置")]
        public string Location { get; set; }
        [Display(Name = "年龄")]
        public int Age { get; set; }
        [Display(Name = "性别")]
        public string Sex { get; set; }
        [Display(Name = "登录状态")]
        public string Status { get; set; }
        [Display(Name ="所属球队")]
        public string TeamName { get; set; }
        [Display(Name = "权限等级")]
        public int Access { get; set; }

    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }


    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "代码")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "记住此浏览器?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }


    public class LoginViewModel
    {

        [Required]
        [Display(Name = "用户名")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Display(Name = "记住我?")]
        public bool RememberMe { get; set; }

        [Display(Name = "验证码")]
        //[Required(ErrorMessage = "{0}必须填写")]
        public string ValidCode { get; set; }

    }
    public class ManageAccountModel
    {
        [Display(Name = "用户名")]
        public string Username { get; set; }

        [Display(Name = "id")]
        public string id { get; set; }

        [Display(Name = "锁定账号")]
        public bool LockoutEnabled { get; set; }

        [Display(Name = "锁定结束日期")]
        public string LocLockoutEndDateUtckoutEnabled { get; set; }
    }

    public class PersonalCenter
    {
        [Display(Name = "用户名")]
        public string Username { get; set; }

        [EmailAddress]
        [Display(Name = "电子邮件")]
        public string Email { get; set; }

        [Display(Name = "生日")]
        public DateTime BirthDate { get; set; }
    }


    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "用户名")]
        public string Username { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} 必须至少包含 {2} 个字符。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "确认密码")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "密码和确认密码不匹配。")]
        public string ConfirmPassword { get; set; }


    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "电子邮件")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} 必须至少包含 {2} 个字符。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "确认密码")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "密码和确认密码不匹配。")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "密码错误")]
        [Display(Name = "当前密码")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} 必须至少包含 {2} 个字符。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "新密码")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "确认新密码")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "新密码和确认密码不匹配。")]
        public string ConfirmPassword { get; set; }
    }

    public class IndexViewModel
    {
        public bool HasPassword { get; set; }
        public IList<UserLoginInfo> Logins { get; set; }
        public string PhoneNumber { get; set; }
        public bool TwoFactor { get; set; }
        public bool BrowserRemembered { get; set; }
    }
}