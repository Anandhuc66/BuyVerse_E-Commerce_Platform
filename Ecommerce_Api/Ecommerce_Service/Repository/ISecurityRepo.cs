using Ecommerce_Common;
using Ecommerce_Entity.DTO;
using Ecommerce_Entity.Models;
using Microsoft.AspNetCore.Identity.Data;
using DTO = Ecommerce_Entity.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce_Service.Repository
{
    public interface ISecurityRepo
    {
        //Task<Result<UserResponse>> Authenticate(UserRequest request);
        //Task<Result<UserResponse>> Register(UserRequest request);
        Task<Result<string>> RegisterAsync(RegisterDto model);
        Task<Result<string>> LoginAsync(LoginDto model);
        Task<string> GenerateToken(ApplicationUser user);
    }
}
