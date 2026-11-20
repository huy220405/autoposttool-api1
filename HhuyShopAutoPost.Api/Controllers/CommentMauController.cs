using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HhuyShopAutoPost.Api.Data;
using HhuyShopAutoPost.Api.Models;

namespace HhuyShopAutoPost.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentMauController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CommentMauController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Lấy danh sách toàn bộ comment mẫu
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommentMau>>> GetDanhSach()
        {
            return await _context.CommentMaus.OrderByDescending(c => c.NgayTao).ToListAsync();
        }

        // 2. Thêm mới một comment mẫu (Sếp dùng chức năng này)
        [HttpPost]
        public async Task<ActionResult<CommentMau>> ThemComment(CommentMau commentMau)
        {
            commentMau.NgayTao = DateTime.Now;
            _context.CommentMaus.Add(commentMau);
            await _context.SaveChangesAsync();
            return Ok(commentMau);
        }

        // 3. Xóa một comment mẫu khi không dùng nữa
        [HttpDelete("{id}")]
        public async Task<IActionResult> XoaComment(int id)
        {
            var comment = await _context.CommentMaus.FindAsync(id);
            if (comment == null)
            {
                return NotFound("Không tìm thấy comment mẫu này.");
            }

            _context.CommentMaus.Remove(comment);
            await _context.SaveChangesAsync();
            return Ok("Đã xóa thành công.");
        }
    }
}