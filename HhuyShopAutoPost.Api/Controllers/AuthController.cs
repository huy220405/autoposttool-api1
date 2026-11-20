using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using HhuyShopAutoPost.Api.Data;
using HhuyShopAutoPost.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HhuyShopAutoPost.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Giữ nguyên thông tin App Facebook của bạn
        private readonly string _appId = "4597986780524665";
        private readonly string _appSecret = "9673fe31129497b5e8a75465669c2ab5";
        private readonly string _redirectUri = "https://localhost:7006/api/Auth/callback";

        // Đã xóa _pageId fix cứng vì bây giờ hệ thống sẽ tự quét nhiều Fanpage

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // Bổ sung tham số userId để biết tài khoản nào đang bấm kết nối
        [HttpGet("login")]
        public IActionResult Login([FromQuery] int userId)
        {
            string scope = "pages_manage_posts,pages_read_engagement,pages_show_list";
            // Gói userId vào tham số 'state' để Facebook gửi trả lại ở hàm Callback
            string fbLoginUrl = $"https://www.facebook.com/v19.0/dialog/oauth?client_id={_appId}&redirect_uri={_redirectUri}&scope={scope}&state={userId}";
            return Redirect(fbLoginUrl);
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state)
        {
            if (string.IsNullOrEmpty(code))
                return BadRequest("Không nhận được mã xác thực từ Facebook.");

            // Giải mã userId từ state
            int userId = 0;
            if (!int.TryParse(state, out userId))
            {
                // Phòng trường hợp lỗi, tự động gán cho tài khoản đầu tiên trong Database
                var firstUser = await _context.NguoiDungs.FirstOrDefaultAsync();
                if (firstUser != null) userId = firstUser.Id;
            }

            using (HttpClient client = new HttpClient())
            {
                // Bước 1: Đổi code lấy User Token
                string tokenUrl = $"https://graph.facebook.com/v19.0/oauth/access_token?client_id={_appId}&redirect_uri={_redirectUri}&client_secret={_appSecret}&code={code}";
                var tokenResponse = await client.GetAsync(tokenUrl);
                var tokenData = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync());

                if (!tokenData.RootElement.TryGetProperty("access_token", out var userTokenElement))
                    return BadRequest("Lỗi khi lấy User Token.");

                string userToken = userTokenElement.GetString();

                // Bước 2: API lấy toàn bộ Fanpage mà user này quản lý (Thay vì chỉ 1 page như trước)
                string pagesUrl = $"https://graph.facebook.com/v19.0/me/accounts?fields=id,name,access_token,picture&access_token={userToken}";
                var pagesResponse = await client.GetAsync(pagesUrl);
                var pagesData = JsonDocument.Parse(await pagesResponse.Content.ReadAsStringAsync());

                if (pagesData.RootElement.TryGetProperty("data", out var dataArray))
                {
                    // Lặp qua từng Fanpage và lưu vào cấu trúc bảng CauHinh mới
                    foreach (var page in dataArray.EnumerateArray())
                    {
                        string pageId = page.GetProperty("id").GetString();
                        string pageName = page.GetProperty("name").GetString();
                        string pageToken = page.GetProperty("access_token").GetString();
                        string avatarUrl = "";

                        if (page.TryGetProperty("picture", out var pic) &&
                            pic.TryGetProperty("data", out var picData) &&
                            picData.TryGetProperty("url", out var urlElement))
                        {
                            avatarUrl = urlElement.GetString();
                        }

                        var existingPage = await _context.CauHinhs
                            .FirstOrDefaultAsync(c => c.PageId == pageId && c.NguoiDungId == userId);

                        if (existingPage == null)
                        {
                            _context.CauHinhs.Add(new CauHinh
                            {
                                NguoiDungId = userId,
                                PageId = pageId,
                                TenFanpage = pageName,
                                AccessToken = pageToken,
                                AvatarUrl = avatarUrl,
                                NgayKetNoi = DateTime.Now
                            });
                        }
                        else
                        {
                            existingPage.AccessToken = pageToken;
                            existingPage.TenFanpage = pageName;
                            existingPage.AvatarUrl = avatarUrl;
                            existingPage.NgayKetNoi = DateTime.Now;
                        }
                    }
                    await _context.SaveChangesAsync();

                    // Điều hướng về lại trang Cài đặt của Vue.js
                    return Redirect("http://localhost:5173/about?status=success");
                }

                return BadRequest("Không lấy được danh sách Fanpage.");
            }
        }

        // Bổ sung API cho Frontend gọi để lấy danh sách Fanpage đang kết nối
        [HttpGet("danh-sach/{nguoiDungId}")]
        public async Task<IActionResult> LayDanhSach(int nguoiDungId)
        {
            var pages = await _context.CauHinhs
                .Where(c => c.NguoiDungId == nguoiDungId)
                .Select(c => new {
                    c.Id,
                    c.PageId,
                    c.TenFanpage,
                    c.AvatarUrl,
                    c.NgayKetNoi
                }).ToListAsync();

            return Ok(pages);
        }

        // Bổ sung API Xóa Token khi người dùng bấm "Ngắt kết nối"
        [HttpDelete("ngat-ket-noi/{id}")]
        public async Task<IActionResult> NgatKetNoi(int id)
        {
            var page = await _context.CauHinhs.FindAsync(id);
            if (page != null)
            {
                _context.CauHinhs.Remove(page);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Đã ngắt kết nối Fanpage thành công!" });
            }
            return NotFound(new { message = "Không tìm thấy Fanpage này." });
        }
    }
}