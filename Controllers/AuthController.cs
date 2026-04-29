using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VetClinicAPI.Models;

namespace VetClinicAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login(Usuario user)
        {
            if (user.Email == "admin@vet.com" && user.Password == "1234")
            {
                var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes("VetClinicClaveSegura2026Token123"));

                var creds = new SigningCredentials(
                    key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    claims: new[] { new Claim(ClaimTypes.Name, user.Email) },
                    expires: DateTime.Now.AddHours(1),
                    signingCredentials: creds
                );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token)
                });
            }

            return Unauthorized("Credenciales incorrectas");
        }
    }
}
