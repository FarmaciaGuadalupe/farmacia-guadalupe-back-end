using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApi.Data;
using WebApi.Interfaces;
using WebApi.Models.DTOs;
using WebApi.Models.Empleados;
using WebApi.Models;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _dataBase;
        private readonly JwtOptions _jwt;
        private readonly ITelegramService _telegramService;
        private readonly ITelegramMessageService _telegramMessageService;

        public AuthController(AppDbContext db, IOptions<JwtOptions> jwtOptions,  ITelegramService telegramService, ITelegramMessageService telegramMessageService)
        {
            _dataBase = db;
            _jwt = jwtOptions.Value;
            _telegramService = telegramService;
            _telegramMessageService = telegramMessageService;
        }

        [HttpGet("probar-bot")]
        public async Task<IActionResult> ProbarBot()
        {
            try {
                string message = _telegramMessageService.GetDailySalesReportMessage(DateTime.Now);
                await _telegramService.SendDailyReport(message);
                return Ok("Mensaje enviado con éxito.");
            }
            catch (Exception ex) {
                return BadRequest($"Error: {ex.Message}");
            }
        }
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse>> Login([FromBody] LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto?.user) || string.IsNullOrWhiteSpace(dto?.password))
                return BadRequest(new ApiResponse { success = false, message = "Usuario y contraseña son obligatorios" });

            var username = dto.user.Trim();

            var emp = await _dataBase.Employees
                .Include(e => e.EmployeeRole)
                .Include(e => e.EmployeeStatus)
                .FirstOrDefaultAsync(e => e.user.ToLower() == username.ToLower());

            if (emp == null)
                return Unauthorized(new ApiResponse { success = false, message = "Credenciales inválidas" });

            // var okPassword = BCrypt.Net.BCrypt.Verify(dto.password, emp.password);
            // /*if (!okPassword)
            //     return Unauthorized(new ApiResponse { success = false, message = "Credenciales inválidas" });
            //     */

            if (emp.EmployeeStatusId != 1)
                return Forbid(); // o Unauthorized con mensaje

            var (tokenString, expires) = GenerateJwt(emp);

            var resp = new LoginResponseDto
            {
                token = tokenString,
                expiresAt = expires,
                EmployeeId = emp.EmployeeId,
                names = emp.names,
                lastnames = emp.lastnames,
                username = emp.user,
                roleId = emp.EmployeeRoleId,
                roleName = emp.EmployeeRole?.name,
                statusId = emp.EmployeeStatusId,
                statusName = emp.EmployeeStatus?.name
            };

            return Ok(new ApiResponse { success = true, data = resp });
        }

        private (string token, DateTime expires) GenerateJwt(Employee emp)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(_jwt.ExpireMinutes);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, emp.EmployeeId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, emp.user ?? ""),
                new Claim("employeeId", emp.EmployeeId.ToString()),
                new Claim("username", emp.user ?? ""),
                new Claim("names", emp.names ?? ""),
                new Claim("lastnames", emp.lastnames ?? ""),
                //new Claim("email", emp.email ?? ""),
                new Claim("phone", emp.phone ?? ""),
                new Claim("email", emp.email ?? ""),
                new Claim("urlPhoto", emp.url_photo ?? ""),
                new Claim("roleId", emp.EmployeeRoleId.ToString()),
                new Claim("roleName", emp.EmployeeRole?.name ?? ""),
                new Claim("statusId", emp.EmployeeStatusId.ToString()), 
                new Claim("statusName", emp.EmployeeStatus?.name ?? ""),
                //new Claim("status", emp.status ?? ""),
                new Claim("hiringDate", emp.hiring_date.ToString("yyyy-MM-dd"))
            };

            var jwt = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(jwt);
            return (tokenString, expires);
        }

    }
}
