using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace HhuyShopAutoPost.Api.Services
{
    public class FacebookSeedingService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public FacebookSeedingService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> ExecuteCommentAsync(string cookie, string postId, string noiDungComment)
        {
            var client = _httpClientFactory.CreateClient();

            // Giả lập Mobile để qua mặt Checkpoint
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 13; SM-S918B) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Mobile Safari/537.36");

            // Đảm bảo Cookie được gắn đúng chuẩn
            client.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", cookie);

            try
            {
                // BƯỚC 1: Thử truy cập theo định dạng bài viết thông thường
                string mbasicUrl = $"https://mbasic.facebook.com/{postId}";
                var getResponse = await client.GetAsync(mbasicUrl);
                string html = await getResponse.Content.ReadAsStringAsync();

                // BƯỚC 2: Nếu bài viết không chứa form comment, khả năng cao đây là bài viết ẢNH
                if (!html.Contains("comment_text"))
                {
                    mbasicUrl = $"https://mbasic.facebook.com/photo.php?fbid={postId}";
                    getResponse = await client.GetAsync(mbasicUrl);
                    html = await getResponse.Content.ReadAsStringAsync();
                }

                // Bóc tách Token bảo mật
                string fb_dtsg = Regex.Match(html, "name=\"fb_dtsg\" value=\"([^\"]+)\"").Groups[1].Value;
                string jazoest = Regex.Match(html, "name=\"jazoest\" value=\"([^\"]+)\"").Groups[1].Value;
                string actionUrl = Regex.Match(html, "action=\"(/a/comment\\.php\\?[^\"]+)\"").Groups[1].Value;

                // Kiểm tra xem có lấy được form comment không
                if (string.IsNullOrEmpty(fb_dtsg) || string.IsNullOrEmpty(actionUrl))
                {
                    Console.WriteLine($"[-] Lỗi: Không tìm thấy form comment cho PostID {postId}. Cookie có thể đã Die hoặc sai PostID.");
                    return false;
                }

                // BƯỚC 3: Cấu hình gói dữ liệu
                var formVariables = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("fb_dtsg", fb_dtsg),
                    new KeyValuePair<string, string>("jazoest", jazoest),
                    new KeyValuePair<string, string>("comment_text", noiDungComment)
                };

                var content = new FormUrlEncodedContent(formVariables);

                // Fix lỗi URL encode
                string postUrl = "https://mbasic.facebook.com" + actionUrl.Replace("&amp;", "&");

                // BƯỚC 4: Bắn Comment
                var postResponse = await client.PostAsync(postUrl, content);

                // FB thường trả về 302 Redirect khi thao tác thành công
                if (postResponse.IsSuccessStatusCode || (int)postResponse.StatusCode == 302)
                {
                    Console.WriteLine($"[+] Thành công! Clone đã nhả comment: {noiDungComment}");
                    return true;
                }

                Console.WriteLine($"[-] Comment thất bại. HTTP Status: {postResponse.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Lỗi Hệ Thống Seeding]: {ex.Message}");
                return false;
            }
        }
    }
}