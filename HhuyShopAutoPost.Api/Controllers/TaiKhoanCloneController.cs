using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HhuyShopAutoPost.Api.Data;
using HhuyShopAutoPost.Api.Models;
using HhuyShopAutoPost.Api.Services;

namespace HhuyShopAutoPost.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaiKhoanCloneController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly FacebookSeedingService _seedingService;

        // Tiêm Service Seeding vào Controller
        public TaiKhoanCloneController(AppDbContext context, FacebookSeedingService seedingService)
        {
            _context = context;
            _seedingService = seedingService;
        }

        // 1. Lấy toàn bộ danh sách tài khoản Clone
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var danhSach = await _context.TaiKhoanClone
                                         .OrderByDescending(t => t.NgayThem)
                                         .ToListAsync();
            return Ok(danhSach);
        }

        // 2. Thêm mới một tài khoản Clone
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TaiKhoanClone taiKhoan)
        {
            taiKhoan.NgayThem = DateTime.Now;

            if (string.IsNullOrEmpty(taiKhoan.TrangThai))
            {
                taiKhoan.TrangThai = "LIVE";
            }

            _context.TaiKhoanClone.Add(taiKhoan);
            await _context.SaveChangesAsync();

            return Ok(taiKhoan);
        }

        // 3. Cập nhật thông tin tài khoản
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TaiKhoanClone request)
        {
            var taiKhoan = await _context.TaiKhoanClone.FindAsync(id);
            if (taiKhoan == null)
                return NotFound(new { message = "Không tìm thấy tài khoản này trong hệ thống" });

            taiKhoan.TenTaiKhoan = request.TenTaiKhoan;
            taiKhoan.Uid = request.Uid;
            taiKhoan.Cookie = request.Cookie;
            taiKhoan.AccessToken = request.AccessToken;
            taiKhoan.TrangThai = request.TrangThai;

            await _context.SaveChangesAsync();
            return Ok(taiKhoan);
        }

        // 4. Xóa tài khoản Clone
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var taiKhoan = await _context.TaiKhoanClone.FindAsync(id);
            if (taiKhoan == null)
                return NotFound(new { message = "Không tìm thấy tài khoản này trong hệ thống" });

            _context.TaiKhoanClone.Remove(taiKhoan);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa tài khoản clone thành công!" });
        }

        // 5. API TEST NHẢ COMMENT
        [HttpPost("TestComment")]
        public async Task<IActionResult> TestComment([FromBody] TestCommentRequest request)
        {
            // Tìm tài khoản clone trong Database
            var clone = await _context.TaiKhoanClone.FindAsync(request.CloneId);
            if (clone == null)
                return NotFound(new { message = "Không tìm thấy tài khoản Clone này!" });

            if (string.IsNullOrEmpty(clone.Cookie))
                return BadRequest(new { message = "Tài khoản này chưa có Cookie để đi comment!" });

            // Gọi Service thực thi bắn comment lên Facebook
            bool isSuccess = await _seedingService.ExecuteCommentAsync(clone.Cookie, request.PostId, request.Message);

            if (isSuccess)
            {
                return Ok(new { message = $"Đã dùng clone {clone.TenTaiKhoan} thả comment thành công!" });
            }
            else
            {
                return BadRequest(new { message = "Comment thất bại. Kiểm tra lại Cookie hoặc PostID (Có thể bài viết bị khóa comment)." });
            }
        }
    }

    // DTO (Data Transfer Object) để hứng dữ liệu từ Vue.js gửi lên
    public class TestCommentRequest
    {
        public int CloneId { get; set; }
        public string PostId { get; set; }
        public string Message { get; set; }
    }
}