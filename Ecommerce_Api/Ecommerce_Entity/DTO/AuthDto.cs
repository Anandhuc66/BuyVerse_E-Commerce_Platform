using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce_Entity.DTO
{
    public class RegisterDto
    {
        [Required] public string FullName { get; set; }
        [Required] public string Gender { get; set; }
        [Required][EmailAddress] public string Email { get; set; }
        [Required] public string Password { get; set; }
        [Required] public string PhoneNumber { get; set; }
    }


    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class UserResponse
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }

        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public int? SupplierId { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required] public string CurrentPassword { get; set; }
        [Required][MinLength(6)] public string NewPassword { get; set; }
    }

    public class UpdateProfileDto
    {
        [Required] public string FullName { get; set; }
        [Required] public string Gender { get; set; }
        [Required] public string PhoneNumber { get; set; }
    }

    public class UserListDto
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
