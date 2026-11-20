using HhuyShopAutoPost.Api.Data;
using HhuyShopAutoPost.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Net.Http.Json;

namespace HhuyShopAutoPost.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TinNhanController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TinNhanController(AppDbContext context)
        {
            _context = context;
        }

        // =======================================================
        // API 1: Lấy danh sách ID kèm SỐ LƯỢNG CHƯA ĐỌC
        // =======================================================
        [HttpGet("khach-hang")]
        public async Task<IActionResult> GetDanhSachKhachHang()
        {
            var khachHangs = await _context.TinNhans
                .GroupBy(t => t.SenderId)
                .Select(g => new {
                    SenderId = g.Key,
                    // Đếm những tin nhắn gửi tới Shop (PageId != SENT) và chưa đọc
                    UnreadCount = g.Count(x => !x.DaDoc && x.PageId != "SENT")
                })
                .ToListAsync();

            return Ok(khachHangs);
        }

        // =======================================================
        // API 2: Lấy TỔNG số lượng tin chưa đọc của toàn Fanpage
        // =======================================================
        [HttpGet("tong-chua-doc")]
        public async Task<IActionResult> GetTongChuaDoc()
        {
            var count = await _context.TinNhans.CountAsync(t => !t.DaDoc && t.PageId != "SENT");
            return Ok(count);
        }

        // =======================================================
        // API 3: Đánh dấu ĐÃ ĐỌC toàn bộ tin nhắn của 1 khách hàng
        // =======================================================
        [HttpPost("doc-tin/{senderId}")]
        public async Task<IActionResult> DocTinNhan(string senderId)
        {
            var tinChuaDoc = await _context.TinNhans
                .Where(t => t.SenderId == senderId && !t.DaDoc && t.PageId != "SENT")
                .ToListAsync();

            if (tinChuaDoc.Any())
            {
                foreach (var t in tinChuaDoc) t.DaDoc = true;
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        // =======================================================
        // API 4: Lấy chi tiết lịch sử chat
        // =======================================================
        [HttpGet("chi-tiet/{senderId}")]
        public async Task<IActionResult> GetChiTietTinNhan(string senderId)
        {
            var tinNhans = await _context.TinNhans
                .Where(t => t.SenderId == senderId)
                .OrderBy(t => t.ThoiGianGui)
                .ToListAsync();

            return Ok(tinNhans);
        }

        // =======================================================
        // API 5: Lấy Profile Facebook
        // =======================================================
        [HttpGet("profile/{senderId}")]
        public async Task<IActionResult> GetFacebookProfile(string senderId)
        {
            string pageToken = "EABBV2Di8MHkBSicY6cm22MBwlALGnY263uw91DLKkPBI11xI9PZB7brSO7ZCBe7UL4hBfZCVZB3XtpBiZAhYiDWEKMAO2CuyO6TPBJheU4osPYCIBYhekQYP7H6FTrdr0qAutZBwbAvRRThZCTrD74HgC47kjiNaGFPq3wIVNRwaiv9Dm6MryOUiZAUZAYBXrIt3S7wapGUOnUgZDZD";
            string url = $"https://graph.facebook.com/{senderId}?fields=name,profile_pic&access_token={pageToken}";

            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }

            return Ok(new { name = "Khách hàng " + senderId, profile_pic = "https://cdn-icons-png.flaticon.com/512/149/149071.png" });
        }

        // =======================================================
        // API 6: Gửi tin nhắn trả lời
        // =======================================================
        [HttpPost("gui-tin")]
        public async Task<IActionResult> GuiTinNhan([FromBody] GuiTinRequest request)
        {
            string pageToken = "EABBV2Di8MHkBSicY6cm22MBwlALGnY263uw91DLKkPBI11xI9PZB7brSO7ZCBe7UL4hBfZCVZB3XtpBiZAhYiDWEKMAO2CuyO6TPBJheU4osPYCIBYhekQYP7H6FTrdr0qAutZBwbAvRRThZCTrD74HgC47kjiNaGFPq3wIVNRwaiv9Dm6MryOUiZAUZAYBXrIt3S7wapGUOnUgZDZD";
            string url = $"https://graph.facebook.com/v19.0/me/messages?access_token={pageToken}";

            var payload = new { recipient = new { id = request.SenderId }, message = new { text = request.NoiDung } };

            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsJsonAsync(url, payload);

            if (response.IsSuccessStatusCode)
            {
                var tinMoi = new TinNhan
                {
                    PageId = "SENT",
                    SenderId = request.SenderId,
                    NoiDung = request.NoiDung,
                    ThoiGianGui = DateTime.Now,
                    DaDoc = true
                };
                _context.TinNhans.Add(tinMoi);
                await _context.SaveChangesAsync();
                return Ok(tinMoi);
            }
            return BadRequest("Gửi tin nhắn thất bại, hãy kiểm tra lại Token.");
        }
    }

    public class GuiTinRequest { public string SenderId { get; set; } public string NoiDung { get; set; } }
}