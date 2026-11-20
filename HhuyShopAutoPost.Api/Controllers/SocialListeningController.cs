using HhuyShopAutoPost.Api.Data;
using HhuyShopAutoPost.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HhuyShopAutoPost.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SocialListeningController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SocialListeningController(AppDbContext context)
        {
            _context = context;
        }

        // =======================================================
        // API 1: Lấy danh sách toàn bộ từ khóa đang theo dõi
        // =======================================================
        [HttpGet("tu-khoa")]
        public async Task<IActionResult> GetDanhSachTuKhoa()
        {
            var tuKhoas = await _context.TuKhoaTheoDois
                .OrderByDescending(t => t.NgayTao) // Mới thêm lên đầu
                .ToListAsync();
            return Ok(tuKhoas);
        }

        // =======================================================
        // API 2: Thêm một từ khóa mới vào hệ thống
        // =======================================================
        [HttpPost("tu-khoa")]
        public async Task<IActionResult> ThemTuKhoa([FromBody] TuKhoaRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TuKhoa))
                return BadRequest("Từ khóa không được để trống");

            // Kiểm tra xem từ khóa đã tồn tại chưa để tránh thêm trùng
            var daTonTai = await _context.TuKhoaTheoDois
                .AnyAsync(t => t.TuKhoa.ToLower() == request.TuKhoa.ToLower());

            if (daTonTai)
                return BadRequest("Từ khóa này đã tồn tại trong danh sách theo dõi!");

            var tuKhoaMoi = new TuKhoaTheoDoi
            {
                TuKhoa = request.TuKhoa.Trim(),
                IsActive = true,
                NgayTao = DateTime.Now
            };

            _context.TuKhoaTheoDois.Add(tuKhoaMoi);
            await _context.SaveChangesAsync();

            return Ok(tuKhoaMoi);
        }

        // =======================================================
        // API 3: Bật/Tắt tính năng theo dõi của 1 từ khóa (Tạm dừng quét)
        // =======================================================
        [HttpPut("tu-khoa/{id}/toggle")]
        public async Task<IActionResult> ToggleTuKhoa(int id)
        {
            var tuKhoa = await _context.TuKhoaTheoDois.FindAsync(id);
            if (tuKhoa == null) return NotFound("Không tìm thấy từ khóa");

            tuKhoa.IsActive = !tuKhoa.IsActive; // Đảo trạng thái True/False
            await _context.SaveChangesAsync();

            return Ok(tuKhoa);
        }

        // =======================================================
        // API 4: Xóa vĩnh viễn từ khóa
        // =======================================================
        [HttpDelete("tu-khoa/{id}")]
        public async Task<IActionResult> XoaTuKhoa(int id)
        {
            var tuKhoa = await _context.TuKhoaTheoDois.FindAsync(id);
            if (tuKhoa == null) return NotFound("Không tìm thấy từ khóa");

            _context.TuKhoaTheoDois.Remove(tuKhoa);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // =======================================================
        // API 5: Lấy lịch sử cảnh báo (GIẢI QUYẾT LỖI F5 TRẮNG TRANG)
        // =======================================================
        [HttpGet("canh-bao")]
        public async Task<IActionResult> GetLichSuCanhBao()
        {
            try
            {
                // Lấy 50 bình luận mới nhất từ Database và đóng gói lại để tránh lỗi vòng lặp SignalR
                var danhSachPhot = await _context.DuLieuLangNghes
                    .OrderByDescending(d => d.NgayThuThap)
                    .Take(50)
                    .Select(d => new {
                        noiDung = d.NoiDung,
                        camXuc = d.CamXuc,
                        nguon = d.Nguon,
                        linkBaiViet = d.LinkBaiViet,
                        ngayThuThap = d.NgayThuThap
                    })
                    .ToListAsync();

                return Ok(danhSachPhot);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }

    // Class hứng dữ liệu từ Vue.js gửi lên
    public class TuKhoaRequest
    {
        public string TuKhoa { get; set; }
    }
}