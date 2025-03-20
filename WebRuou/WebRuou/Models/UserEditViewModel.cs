using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebRuou.Models
{
    public class UserEditViewModel
    {
        public string ExternalId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        public string FullName { get; set; }
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ.")]
        public string Address { get; set; }
    }
}