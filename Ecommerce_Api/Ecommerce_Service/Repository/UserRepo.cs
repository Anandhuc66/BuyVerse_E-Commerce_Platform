using Ecommerce_Common;
using Ecommerce_Entity.Data;
using Ecommerce_Entity.DTO;
using Ecommerce_Entity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce_Service.Repository
{
    public class UserRepo : IUserRepo
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        public UserRepo(
         UserManager<ApplicationUser> userManager,
         RoleManager<AppRole> roleManager,
         IConfiguration configuration,
         ApplicationDbContext context
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = context;
        }

        // This is now public to satisfy the interface
        public async Task<string> GenerateToken(ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));

            var authClaims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.FullName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var roles = await _userManager.GetRolesAsync(user);
            authClaims.AddRange(roles.Select(role => new Claim("role", role)));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: authClaims,
                expires: DateTime.UtcNow.AddHours(5),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        //public async Task<Result<UserResponse>> RegisterAsync(RegisterDto model)
        //{
        //    var result = new Result<UserResponse>();
        //    var existingUser = await _userManager.FindByEmailAsync(model.Email);
        //    if (existingUser != null)
        //    {
        //        result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = "Email already exists." });
        //        return result;
        //    }

        //    var user = new ApplicationUser
        //    {
        //        UserName = model.Email,
        //        Email = model.Email,
        //        FullName = model.FullName,
        //        PhoneNumber = model.PhoneNumber,
        //        Gender = model.Gender
        //    };

        //    var createResult = await _userManager.CreateAsync(user, model.Password);
        //    if (!createResult.Succeeded)
        //    {
        //        foreach (var error in createResult.Errors)
        //        {
        //            result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = error.Description });
        //        }
        //        return result;
        //    }

        //    // Assign default role with Description to avoid SQL error
        //    var defaultRole = "User";
        //    if (!await _roleManager.RoleExistsAsync(defaultRole))
        //    {
        //        await _roleManager.CreateAsync(new AppRole
        //        {
        //            Name = defaultRole,
        //            DisplayName = defaultRole,
        //            Description = "Default user role" // ✅ Important
        //        });
        //    }

        //    await _userManager.AddToRoleAsync(user, defaultRole);

        //    var token = await GenerateToken(user);

        //    result.Response = new UserResponse
        //    {
        //        UserId = user.Id,
        //        Email = user.Email,
        //        FullName = user.FullName,
        //        Role = defaultRole,
        //        Token = token
        //    };
        //    result.Message = "Registration successful.";
        //    //result.Token = token;

        //    return result;
        //}
        public async Task<Result<UserResponse>> RegisterUserAsync(RegisterDto model)
        {
            var result = new Result<UserResponse>();

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = "Email already exists." });
                return result;
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                Gender = model.Gender
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = error.Description });
                }
                return result;
            }

            var role = "User";
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new AppRole { Name = role, DisplayName = role, Description = "Default user role" });
            }
            await _userManager.AddToRoleAsync(user, role);

            var token = await GenerateToken(user);

            result.Response = new UserResponse
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = role,
                Token = token,
                Gender = user.Gender,
                PhoneNumber = user.PhoneNumber
            };

            result.Message = "User registered successfully.";

            return result;
        }

        public async Task<Result<UserResponse>> RegisterSupplierAsync(SupplierCreateDto model)
        {
            var result = new Result<UserResponse>();

            // Step 1: Check if email already used
            var existingUser = await _userManager.FindByEmailAsync(model.ContactEmail);
            if (existingUser != null)
            {
                result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = "Email already exists." });
                return result;
            }

            // Step 2: Create identity user
            var user = new ApplicationUser
            {
                UserName = model.ContactEmail,
                Email = model.ContactEmail,
                FullName = model.CompanyName,
                PhoneNumber = model.Phone,

                // FIX 
                Gender = model.Gender ?? "Not Specified"
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = error.Description });
                }
                return result;
            }

            // Step 3: Assign "Supplier" role
            var role = "Supplier";
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(
                    new AppRole { Name = role, DisplayName = role, Description = "Default supplier role" }
                );
            }
            await _userManager.AddToRoleAsync(user, role);

            // Step 4: Create supplier record linked to user
            var supplier = new Supplier
            {
                CompanyName = model.CompanyName,
                ContactEmail = model.ContactEmail,
                Phone = model.Phone,
                FullName = model.CompanyName,   // OPTIONAL – if you want supplier full name
                Gender = model.Gender,         // OPTIONAL
                UserId = user.Id
            };

            _context.SuppliersSet.Add(supplier);
            await _context.SaveChangesAsync();

            // Step 5: Generate token
            var token = await GenerateToken(user);

            result.Response = new UserResponse
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = role,
                Token = token,
                SupplierId = supplier.Id
            };
            result.Message = "Supplier registered successfully.";

            return result;
        }



        public async Task<Result<UserResponse>> LoginAsync(LoginDto model)
        {
            var result = new Result<UserResponse>();
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "User not found." });
                return result;
            }

            if (!await _userManager.CheckPasswordAsync(user, model.Password))
            {
                result.Errors.Add(new Errors { ErrorCode = 401, ErrorMessage = "Invalid password." });
                return result;
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = await GenerateToken(user);

            var supplier = await _context.SuppliersSet
            .FirstOrDefaultAsync(s => s.UserId == user.Id);

            result.Response = new UserResponse
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = roles.FirstOrDefault(),
                Token = token,
                Gender = user.Gender,
                PhoneNumber = user.PhoneNumber,
                SupplierId = supplier?.Id
            };

            result.Message = "Login successful.";
            //result.Token = token;

            return result;
        }

        public async Task<Result<List<UserListDto>>> GetAllUsersAsync()
        {
            var result = new Result<List<UserListDto>>();

            // Single query with join to avoid N+1 (one GetRolesAsync per user)
            var userList = await (
                from user in _context.Users
                join userRole in _context.UserRoles on user.Id equals userRole.UserId into userRoles
                from ur in userRoles.DefaultIfEmpty()
                join role in _context.Roles on ur.RoleId equals role.Id into roles
                from r in roles.DefaultIfEmpty()
                group r by new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.Gender,
                    user.PhoneNumber,
                    user.CreatedAt
                } into g
                select new UserListDto
                {
                    UserId = g.Key.Id,
                    FullName = g.Key.FullName,
                    Email = g.Key.Email,
                    Gender = g.Key.Gender,
                    PhoneNumber = g.Key.PhoneNumber,
                    Role = g.Where(r => r != null).Select(r => r.Name).FirstOrDefault() ?? "No Role",
                    CreatedAt = g.Key.CreatedAt
                }
            ).ToListAsync();

            result.Response = userList;
            result.Message = $"Found {userList.Count} users.";
            return result;
        }

        public async Task<Result<bool>> ChangePasswordAsync(string userId, ChangePasswordDto model)
        {
            var result = new Result<bool>();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "User not found." });
                return result;
            }

            var changeResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!changeResult.Succeeded)
            {
                foreach (var error in changeResult.Errors)
                    result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = error.Description });
                return result;
            }

            result.Response = true;
            result.Message = "Password changed successfully.";
            return result;
        }

        public async Task<Result<bool>> UpdateProfileAsync(string userId, UpdateProfileDto model)
        {
            var result = new Result<bool>();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "User not found." });
                return result;
            }

            user.FullName = model.FullName;
            user.Gender = model.Gender;
            user.PhoneNumber = model.PhoneNumber;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                    result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = error.Description });
                return result;
            }

            result.Response = true;
            result.Message = "Profile updated successfully.";
            return result;
        }
    }
}
