using HhuyShopAutoPost.Api.Data;
using HhuyShopAutoPost.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HhuyShopAutoPost.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatboxController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChatboxController(AppDbContext context)
        {
            _context = context;
        }

        // 1. API lấy danh sách khách hàng & hội thoại
        [HttpGet("danh-sach-hoi-thoai")]
        public async Task<IActionResult> GetDanhSachHoiThoai()
        {
            var danhSach = await _context.CuocHoiThoais
                .Include(c => c.KhachHang)
                .OrderByDescending(c => c.ThoiGianCapNhatCuoi)
                .Select(c => new {
                    HoiThoaiId = c.Id,
                    TenKhachHang = c.KhachHang.TenKhachHang,
                    AnhDaiDien = c.KhachHang.AnhDaiDien,
                    NenTang = c.KhachHang.NenTang,
                    ThoiGianCapNhat = c.ThoiGianCapNhatCuoi,
                    IsRead = c.IsRead
                })
                .ToListAsync();

            return Ok(danhSach);
        }

        // 2. API lấy chi tiết tin nhắn
        [HttpGet("tin-nhan/{hoiThoaiId}")]
        public async Task<IActionResult> GetTinNhan(int hoiThoaiId)
        {
            // Đã đổi _context.TinNhans thành _context.TinNhanDaKenhs
            var tinNhans = await _context.TinNhanDaKenhs
                .Where(t => t.HoiThoaiId == hoiThoaiId)
                .OrderBy(t => t.ThoiGianGui)
                .Select(t => new {
                    t.Id,
                    t.IsShopSend,
                    t.NoiDung,
                    t.LoaiTinNhan,
                    t.ThoiGianGui
                })
                .ToListAsync();

            return Ok(tinNhans);
        }

        // 3. CỔNG WEBHOOK 
        [HttpPost("webhook/{nhaCungCap}")]
        public async Task<IActionResult> NhanTinNhanTuMangXaHoi(string nhaCungCap, [FromBody] dynamic payload)
        {
            try
            {
                Console.WriteLine($"[WEBHOOK {nhaCungCap.ToUpper()}] Nhận dữ liệu: {payload}");
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}