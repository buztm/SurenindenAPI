using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SurenindenAPI.DTOs;
using SurenindenAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;

    public AuthController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDTO model)
    {
        var user = new AppUser { UserName = model.UserName, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new AppRole { Name = "User" });

            await _userManager.AddToRoleAsync(user, "User");
            return Ok("Kayıt başarılı!");
        }
        return BadRequest(result.Errors);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDTO model)
    {
        var user = await _userManager.FindByNameAsync(model.UserName);

        if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
        {
            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);

            return Ok(new
            {
                token = token,
                expiration = DateTime.Now.AddDays(1),
                userName = user.UserName,
                roles = roles
            });
        }

        return Unauthorized("Kullanıcı adı veya şifre hatalı!");
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("user/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound("Kullanıcı bulunamadı.");

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded) return Ok("Kullanıcı silindi.");

        return BadRequest(result.Errors);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("user/{id}")]
    public async Task<IActionResult> UpdateUser(string id, RegisterDTO model)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound("Kullanıcı bulunamadı.");

        user.UserName = model.UserName;
        user.Email = model.Email;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded) return Ok("Kullanıcı bilgileri güncellendi.");

        return BadRequest(result.Errors);
    }

    private string GenerateJwtToken(AppUser user, IList<string> roles)
    {
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("BuCokHavaliVeUzunBirCumleHAHAHAHAHA!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.Now.AddDays(1);

        var token = new JwtSecurityToken(
            issuer: "Sureninden",
            audience: "Sureninden",
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}