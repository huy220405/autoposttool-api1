using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
// Thêm 2 using này để nhận diện thư mục Data và Models của bạn
using HhuyShopAutoPost.Api.Data;
using HhuyShopAutoPost.Api.Models;

namespace HhuyShopAutoPost.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaiKhoanController : ControllerBase
    {
        // Đã sửa lại đúng tên AppDbContext theo cấu trúc project của bạn
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public TaiKhoanController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public class DangNhapRequest
        {
            public string TenDangNhap { get; set; }
            public string MatKhau { get; set; }
        }

        [HttpPost("dang-ky")]
        public async Task<IActionResult> DangKy([FromBody] DangNhapRequest request)
        {
            if (await _context.NguoiDungs.AnyAsync(u => u.TenDangNhap == request.TenDangNhap))
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại!" });

            var user = new NguoiDung
            {
                TenDangNhap = request.TenDangNhap,
                MatKhauHash = HashPassword(request.MatKhau),
                HoTen = "Admin " + request.TenDangNhap
            };

            _context.NguoiDungs.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký thành công!" });
        }

        [HttpPost("dang-nhap")]
        public async Task<IActionResult> DangNhap([FromBody] DangNhapRequest request)
        {
            var hashedPass = HashPassword(request.MatKhau);
            var user = await _context.NguoiDungs
                .FirstOrDefaultAsync(u => u.TenDangNhap == request.TenDangNhap && u.MatKhauHash == hashedPass);

            if (user == null)
                return Unauthorized(new { message = "Sai tên đăng nhập hoặc mật khẩu!" });

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    // Chỉ định rõ Namespace System.Security.Claims để triệt tiêu lỗi CS1503
                    new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new System.Security.Claims.Claim(ClaimTypes.Name, user.TenDangNhap),
                    new System.Security.Claims.Claim("HoTen", user.HoTen)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new
            {
                message = "Đăng nhập thành công!",
                token = tokenHandler.WriteToken(token),
                hoTen = user.HoTen
            });
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}