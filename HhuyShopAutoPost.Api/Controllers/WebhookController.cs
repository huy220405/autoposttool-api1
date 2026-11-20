using HhuyShopAutoPost.Api.Data;
using HhuyShopAutoPost.Api.Hubs;
using HhuyShopAutoPost.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace HhuyShopAutoPost.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private const string VERIFY_TOKEN = "hhuyshop_webhook_2026";
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext; // Khai báo ống nối SignalR

        // Bơm AppDbContext và SignalR vào Webhook
        public WebhookController(AppDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet("facebook")]
        public IActionResult VerifyFacebookWebhook(
            [FromQuery(Name = "hub.mode")] string mode,
            [FromQuery(Name = "hub.verify_token")] string token,
            [FromQuery(Name = "hub.challenge")] string challenge)
        {
            if (mode == "subscribe" && token == VERIFY_TOKEN)
            {
                return Ok(int.Parse(challenge));
            }
            return Forbid();
        }

        [HttpPost("facebook")]
        public async Task<IActionResult> ReceiveFacebookEvent([FromBody] JsonElement payload)
        {
            try
            {
                var root = payload;

                // Kiểm tra xem có đúng là sự kiện từ Page không
                if (root.TryGetProperty("object", out var obj) && obj.GetString() == "page")
                {
                    if (root.TryGetProperty("entry", out var entries))
                    {
                        foreach (var entry in entries.EnumerateArray())
                        {
                            if (entry.TryGetProperty("messaging", out var messagings))
                            {
                                foreach (var msg in messagings.EnumerateArray())
                                {
                                    var senderId = msg.GetProperty("sender").GetProperty("id").GetString();
                                    var pageId = msg.GetProperty("recipient").GetProperty("id").GetString();

                                    if (msg.TryGetProperty("message", out var messageData) &&
                                        messageData.TryGetProperty("text", out var textData))
                                    {
                                        string text = textData.GetString();

                                        var tinMoi = new TinNhan
                                        {
                                            PageId = pageId,
                                            SenderId = senderId,
                                            NoiDung = text,
                                            ThoiGianGui = DateTime.Now,
                                            DaDoc = false
                                        };

                                        _context.TinNhans.Add(tinMoi);
                                        await _context.SaveChangesAsync();

                                        // BÁO ĐỘNG REAL-TIME: Gửi tin nhắn mới này sang Vue.js ngay lập tức
                                        await _hubContext.Clients.All.SendAsync("NhanTinNhanMoi", tinMoi);

                                        Console.WriteLine($"[+] Đã lưu và bắn SignalR tin từ {senderId}: {text}");
                                    }
                                }
                            }
                        }
                    }
                }

                return Ok("EVENT_RECEIVED");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Lỗi Webhook]: {ex.Message}");
                return StatusCode(500);
            }
        }
    }
}