using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using HhuyShopAutoPost.Api.Data;

namespace HhuyShopAutoPost.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaoCaoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BaoCaoController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{pageId}")]
        public async Task<IActionResult> GetBaoCao(string pageId)
        {
            // 1. Thống kê từ Database Hệ thống (SQL Server)
            var tongSoBai = await _context.KichBanDangBai.CountAsync(x => x.PageId == pageId);
            var daDang = await _context.KichBanDangBai.CountAsync(x => x.PageId == pageId && x.TrangThai == "DA_DANG");
            var choDang = await _context.KichBanDangBai.CountAsync(x => x.PageId == pageId && x.TrangThai == "CHO_DANG");
            var loi = await _context.KichBanDangBai.CountAsync(x => x.PageId == pageId && x.TrangThai == "LOI");

            // 2. Kéo số liệu từ Facebook Graph API
            var config = await _context.CauHinhs.FirstOrDefaultAsync(c => c.PageId == pageId);
            if (config == null) return NotFound(new { message = "Không tìm thấy cấu hình Fanpage này" });

            int impressions = 0;
            int engagement = 0;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Lấy Lượt tiếp cận (Vẫn dùng Insights, có độ trễ 24-48h)
                    string insightsUrl = $"https://graph.facebook.com/v19.0/{pageId}/insights?metric=page_impressions&period=day&access_token={config.AccessToken}";
                    var responseInsights = await client.GetAsync(insightsUrl);
                    if (responseInsights.IsSuccessStatusCode)
                    {
                        var jsonString = await responseInsights.Content.ReadAsStringAsync();
                        var data = JsonDocument.Parse(jsonString);

                        if (data.RootElement.TryGetProperty("data", out var dataArray))
                        {
                            foreach (var item in dataArray.EnumerateArray())
                            {
                                if (item.GetProperty("name").GetString() == "page_impressions")
                                {
                                    var values = item.GetProperty("values");
                                    if (values.GetArrayLength() > 0)
                                    {
                                        impressions = values[values.GetArrayLength() - 1].GetProperty("value").GetInt32();
                                    }
                                }
                            }
                        }
                    }

                    // Lấy Lượt tương tác REAL-TIME (Đếm trực tiếp Like & Comment từ 10 bài viết gần nhất)
                    string postsUrl = $"https://graph.facebook.com/v19.0/{pageId}/posts?fields=likes.summary(true).limit(0),comments.summary(true).limit(0)&limit=10&access_token={config.AccessToken}";
                    var responsePosts = await client.GetAsync(postsUrl);
                    if (responsePosts.IsSuccessStatusCode)
                    {
                        var jsonString = await responsePosts.Content.ReadAsStringAsync();
                        var data = JsonDocument.Parse(jsonString);

                        if (data.RootElement.TryGetProperty("data", out var postsArray))
                        {
                            foreach (var post in postsArray.EnumerateArray())
                            {
                                // Cộng dồn số Like
                                if (post.TryGetProperty("likes", out var likes) && likes.TryGetProperty("summary", out var likesSummary))
                                {
                                    engagement += likesSummary.GetProperty("total_count").GetInt32();
                                }

                                // Cộng dồn số Comment
                                if (post.TryGetProperty("comments", out var comments) && comments.TryGetProperty("summary", out var commentsSummary))
                                {
                                    engagement += commentsSummary.GetProperty("total_count").GetInt32();
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Bỏ qua nếu có lỗi mạng, vẫn trả về thống kê SQL Server
            }

            return Ok(new
            {
                SystemStats = new { Tong = tongSoBai, DaDang = daDang, ChoDang = choDang, Loi = loi },
                FacebookStats = new { Impressions = impressions, Engagement = engagement }
            });
        }
    }
}