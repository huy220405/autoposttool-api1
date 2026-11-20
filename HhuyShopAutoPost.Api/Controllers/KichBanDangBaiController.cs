using HhuyShopAutoPost.Api.Data;
using HhuyShopAutoPost.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HhuyShopAutoPost.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KichBanDangBaiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public KichBanDangBaiController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPosts([FromQuery] string? pageId = "ALL")
        {
            var query = _context.KichBanDangBai.AsQueryable();

            if (!string.IsNullOrEmpty(pageId) && pageId != "ALL")
            {
                query = query.Where(p => p.PageId == pageId);
            }

            var posts = await query.OrderByDescending(p => p.ThoiGianDang).ToListAsync();
            return Ok(posts);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] KichBanDangBai newPost)
        {
            try
            {
                if (string.IsNullOrEmpty(newPost.PageId))
                {
                    return BadRequest(new { message = "Vui lòng chọn Fanpage đích!" });
                }

                newPost.NgayTao = DateTime.Now;
                newPost.TrangThai = "CHO_DANG";

                _context.KichBanDangBai.Add(newPost);
                await _context.SaveChangesAsync();

                return Ok(newPost);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi lưu dữ liệu: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.KichBanDangBai.FindAsync(id);
            if (post == null)
            {
                return NotFound(new { message = "Không tìm thấy bài viết!" });
            }

            _context.KichBanDangBai.Remove(post);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa kịch bản thành công!" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, [FromBody] KichBanDangBai updatedPost)
        {
            var post = await _context.KichBanDangBai.FindAsync(id);
            if (post == null)
            {
                return NotFound(new { message = "Không tìm thấy bài viết!" });
            }

            post.PageId = updatedPost.PageId;
            post.NoiDung = updatedPost.NoiDung;
            post.LinkHinhAnh = updatedPost.LinkHinhAnh;
            post.ThoiGianDang = updatedPost.ThoiGianDang;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã cập nhật thành công!" });
        }

        [HttpGet("thong-ke")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var tongSo = await _context.KichBanDangBai.CountAsync();
            var dangCho = await _context.KichBanDangBai.CountAsync(p => p.TrangThai == "CHO_DANG");
            var daDang = await _context.KichBanDangBai.CountAsync(p => p.TrangThai == "DA_DANG");
            var thatBai = await _context.KichBanDangBai.CountAsync(p => p.TrangThai == "LOI");

            return Ok(new
            {
                TongSo = tongSo,
                DangCho = dangCho,
                DaDang = daDang,
                ThatBai = thatBai
            });
        }
    }
}