using System.Text.Json;
using System.Text;
using HhuyShopAutoPost.Api.Data;
using HhuyShopAutoPost.Api.Hubs;
using HhuyShopAutoPost.Api.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HhuyShopAutoPost.Api.Services
{
    public class SocialListeningBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly HttpClient _httpClient;

        private readonly string _facebookToken = "EABBV2Di8MHkBSoLBZAPUGbUw9h6zjlrQBIFZCX2Jij0dmZCxWkDYIsf5l8owfWBFw5JwB1gNvcIAmtwN4ndZBax4IbxnIQrwZAaFq71uwAg5Xmjg0mGoBiFfXGltGH0G5SIRoZAgwN2FHWHOcpnTkrYJ7WVhkHNyQiOaQUjdoIzMTjKp9opIcMkzD8oljDSMYxWn7KFGCroUk4lxD2dAi2GF8Ck8mIZCiIhpUYiUNRQ";
        private readonly string _aiApiKey = "YOUR_REAL_AI_API_KEY";

        public SocialListeningBackgroundService(IServiceScopeFactory scopeFactory, IHubContext<ChatHub> hubContext)
        {
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
            _httpClient = new HttpClient();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("🚀 [Bot Lắng Nghe MXH THỰC TẾ] Đã khởi động!");
            while (!stoppingToken.IsCancellationRequested)
            {
                await QuetDuLieuMangXaHoiThucTe();
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }

        private async Task QuetDuLieuMangXaHoiThucTe()
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var tuKhoas = dbContext.TuKhoaTheoDois.Where(t => t.IsActive).ToList();
            if (!tuKhoas.Any()) return;

            try
            {
                string fbUrl = $"https://graph.facebook.com/v19.0/me/posts?fields=id,message,comments.order(reverse_chronological){{id,message,permalink_url,created_time}}&access_token={_facebookToken}";

                var fbResponse = await _httpClient.GetAsync(fbUrl);
                var fbContent = await fbResponse.Content.ReadAsStringAsync();

                if (!fbResponse.IsSuccessStatusCode) return;

                using var jsonDoc = JsonDocument.Parse(fbContent);
                var posts = jsonDoc.RootElement.GetProperty("data").EnumerateArray();

                foreach (var post in posts)
                {
                    if (!post.TryGetProperty("comments", out var commentsObj)) continue;
                    var comments = commentsObj.GetProperty("data").EnumerateArray();

                    foreach (var comment in comments)
                    {
                        string message = comment.GetProperty("message").GetString() ?? "";
                        string link = comment.GetProperty("permalink_url").GetString() ?? "";

                        foreach (var tk in tuKhoas)
                        {
                            if (message.Contains(tk.TuKhoa, StringComparison.OrdinalIgnoreCase))
                            {
                                bool daTonTai = dbContext.DuLieuLangNghes.Any(d => d.LinkBaiViet == link);
                                if (!daTonTai)
                                {
                                    string camXucReal = await PhanTichCamXucVoiAI(message);

                                    var duLieuMoi = new DuLieuLangNghe
                                    {
                                        TuKhoaId = tk.Id,
                                        NoiDung = message,
                                        Nguon = "Facebook Comment",
                                        LinkBaiViet = link,
                                        CamXuc = camXucReal,
                                        NgayThuThap = DateTime.Now
                                    };

                                    dbContext.DuLieuLangNghes.Add(duLieuMoi);
                                    await dbContext.SaveChangesAsync();

                                    Console.WriteLine($"[+] BẮT ĐƯỢC BÌNH LUẬN: '{message}' - CẢM XÚC: {camXucReal.ToUpper()}");

                                    // Đóng gói dữ liệu sạch sẽ để tránh lỗi "A possible object cycle was detected"
                                    var payload = new
                                    {
                                        noiDung = duLieuMoi.NoiDung,
                                        camXuc = duLieuMoi.CamXuc,
                                        nguon = duLieuMoi.Nguon,
                                        linkBaiViet = duLieuMoi.LinkBaiViet,
                                        ngayThuThap = duLieuMoi.NgayThuThap
                                    };

                                    await _hubContext.Clients.All.SendAsync("CanhBaoKhungHoang", payload);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [LỖI HỆ THỐNG]: {ex.Message}");
            }
        }

        private async Task<string> PhanTichCamXucVoiAI(string textToAnalyze)
        {
            try
            {
                var tieuCucWords = new[] { "tệ", "chán", "lừa đảo", "đắt", "xấu", "phốt", "thất vọng", "rách", "chậm" };
                var tichCucWords = new[] { "tốt", "đẹp", "tuyệt", "ưng", "xịn", "nhanh", "hài lòng", "ủng hộ" };

                string lowerText = textToAnalyze.ToLower();
                if (tieuCucWords.Any(w => lowerText.Contains(w))) return "Tiêu cực";
                if (tichCucWords.Any(w => lowerText.Contains(w))) return "Tích cực";
                return "Trung lập";
            }
            catch { return "Trung lập"; }
        }
    }
}