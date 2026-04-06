using Ecommerce_Common;
using Ecommerce_Entity.DTO;
using Ecommerce_Entity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DTO = Ecommerce_Entity.DTO;



namespace Ecommerce_Service.Repository
{
    //public class SecurityRepository : ISecurityRepo
    //{
    //    private readonly UserManager<ApplicationUser> _userManager;
    //    private readonly RoleManager<AppRole> _roleManager;
    //    private readonly IConfiguration _configuration;

    //    public SecurityRepository(
    //        UserManager<ApplicationUser> userManager,
    //        RoleManager<AppRole> roleManager,
    //        IConfiguration configuration)
    //    {
    //        _userManager = userManager;
    //        _roleManager = roleManager;
    //        _configuration = configuration;
    //    }
    //    public async Task<string> GenerateToken(ApplicationUser user)
    //    {
    //        var jwtSettings = _configuration.GetSection("Jwt");

    //        var userRoles = await _userManager.GetRolesAsync(user);
    //        var authClaims = new List<Claim>
    //        {
    //            new Claim(ClaimTypes.Name, user.UserName),
    //            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    //            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    //        };

    //        foreach (var role in userRoles)
    //            authClaims.Add(new Claim(ClaimTypes.Role, role));

    //        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));

    //        var token = new JwtSecurityToken(
    //            issuer: jwtSettings["Issuer"],
    //            audience: jwtSettings["Audience"],
    //            expires: DateTime.Now.AddHours(2),
    //            claims: authClaims,
    //            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
    //        );

    //        return new JwtSecurityTokenHandler().WriteToken(token);
    //    }

    //    public async Task<UserResponse> Login(DTO.LoginRequest request)
    //    {
    //        var user = await _userManager.FindByEmailAsync(request.Email);
    //        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
    //            throw new Exception("Invalid credentials");

    //        var roles = await _userManager.GetRolesAsync(user);
    //        var token = await GenerateToken(user);

    //        return new UserResponse
    //        {
    //            UserId = user.Id,
    //            Email = user.Email,
    //            FullName = user.FullName,
    //            Role = roles.FirstOrDefault(),
    //            Token = token
    //        };
    //    }

    //    public async Task<UserResponse> Register(DTO.RegisterRequest request)
    //    {
    //        var existingUser = await _userManager.FindByEmailAsync(request.Email);
    //        if (existingUser != null)
    //            throw new Exception("User already exists");

    //        var user = new ApplicationUser
    //        {
    //            UserName = request.Email,
    //            Email = request.Email,
    //            FullName = request.FullName
    //        };

    //        var defaultRole = "Customer";

    //        var result = await _userManager.CreateAsync(user, request.Password);
    //        if (!result.Succeeded)
    //            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

    //        if (!await _roleManager.RoleExistsAsync(defaultRole))
    //            await _roleManager.CreateAsync(new AppRole { Name = defaultRole, DisplayName = defaultRole });

    //        await _userManager.AddToRoleAsync(user, defaultRole);

    //        var token = await GenerateToken(user);

    //        return new UserResponse
    //        {
    //            UserId = user.Id,
    //            Email = user.Email,
    //            FullName = user.FullName,
    //            Role = defaultRole,
    //            Token = token
    //        };
    //    }
    //}
}

