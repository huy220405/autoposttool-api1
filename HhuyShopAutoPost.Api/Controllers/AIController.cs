using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Text;
using System.Text.Json;

namespace HhuyShopAutoPost.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIController : ControllerBase
    {
        // NÂNG CẤP: Mở rộng Model để nhận thêm thông tin Nền tảng và Số lượng biến thể
        public class AiRequest
        {
            public string Prompt { get; set; }
            public string Platform { get; set; } = "Facebook"; // Mặc định là Facebook
            public int Variants { get; set; } = 3; // Mặc định đẻ ra 3 bài khác nhau
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateContent([FromBody] AiRequest request)
        {
            if (string.IsNullOrEmpty(request.Prompt))
                return BadRequest(new { message = "Vui lòng nhập từ khóa!" });

            // 1. Phân vai chuyên gia dựa trên nền tảng (Platform)
            string roleContext = request.Platform switch
            {
                "Zalo" => "chuyên gia marketing Zalo OA. Hãy viết tin nhắn Zalo ZNS/Broadcast lịch sự, súc tích, nhắm thẳng vào lợi ích khách hàng, có kèm lời kêu gọi hành động (CTA) rõ ràng.",
                "Blog" => "chuyên gia SEO Website. Hãy viết một bài Blog chuẩn SEO, có mở bài, thân bài chia thành các Heading (H2, H3) và kết bài. Phân bổ từ khóa tự nhiên.",
                "Tiktok" => "đạo diễn nội dung TikTok/Reels. Hãy viết kịch bản video ngắn dưới 1 phút, chia rõ 2 cột: Hình ảnh (Góc máy/Cảnh quay) và Âm thanh (Lời thoại/Nhạc lồng).",
                _ => "chuyên gia content Facebook. Viết bài quảng cáo bán hàng thật hay, chốt đơn, có sử dụng icon biểu cảm hợp lý."
            };

            // 2. Ép AI đẻ ra nhiều biến thể
            string finalPrompt = $"Đóng vai {roleContext}. Dựa vào từ khóa/yêu cầu sau: '{request.Prompt}'. " +
                                 $"Hãy tạo ra đúng {request.Variants} biến thể nội dung hoàn toàn khác biệt nhau để tôi làm A/B Testing. " +
                                 $"Trình bày rõ ràng bằng cách đánh số: [Biến thể 1], [Biến thể 2]... Bắt buộc dùng Tiếng Việt.";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");

                    var payload = new
                    {
                        messages = new[]
                        {
                            new { role = "user", content = finalPrompt }
                        }
                    };

                    var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("https://text.pollinations.ai/", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var generatedText = await response.Content.ReadAsStringAsync();
                        return Ok(new { content = generatedText });
                    }

                    return Ok(new { content = $"Máy chủ AI đang bận (Lỗi {response.StatusCode}). Bạn thử bấm lại nhé!" });
                }
            }
            catch (Exception ex)
            {
                return Ok(new { content = $"Lỗi hệ thống C#: {ex.Message}" });
            }
        }

        [HttpPost("generate-image")]
        public IActionResult GenerateImage([FromBody] AiRequest request)
        {
            if (string.IsNullOrEmpty(request.Prompt))
                return BadRequest(new { message = "Vui lòng nhập mô tả ảnh!" });

            try
            {
                string safePrompt = Uri.EscapeDataString(request.Prompt);
                Random rnd = new Random();
                int seed = rnd.Next(1, 100000);

                string imageUrl = $"https://image.pollinations.ai/prompt/{safePrompt}?width=1024&height=1024&seed={seed}";

                return Ok(new { url = imageUrl, message = "Tạo ảnh thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}