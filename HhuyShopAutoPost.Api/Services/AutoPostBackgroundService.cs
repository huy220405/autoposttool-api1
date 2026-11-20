using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using HhuyShopAutoPost.Api.Data;
using HhuyShopAutoPost.Api.Models;

namespace HhuyShopAutoPost.Api.Services
{
    public class AutoPostBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        // Đã xóa hoàn toàn _pageId fix cứng ở đây

        public AutoPostBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("=> [Tiến trình ngầm] Đã khởi động và đang canh gác...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                        var baiCanDang = dbContext.KichBanDangBai
                            .Where(b => b.TrangThai == "CHO_DANG" && b.ThoiGianDang <= DateTime.Now)
                            .ToList();

                        if (baiCanDang.Any())
                        {
                            Console.WriteLine($"=> [Tiến trình ngầm] Phát hiện {baiCanDang.Count} bài viết tới giờ đăng!");

                            using (HttpClient client = new HttpClient())
                            {
                                foreach (var bai in baiCanDang)
                                {
                                    // BƯỚC NGOẶT: Tìm đúng Token của Fanpage được gắn với bài viết này
                                    var tokenConfig = dbContext.CauHinhs.FirstOrDefault(c => c.PageId == bai.PageId);

                                    if (tokenConfig == null || string.IsNullOrEmpty(tokenConfig.AccessToken))
                                    {
                                        bai.TrangThai = "LOI";
                                        bai.GhiChuLoi = "Không tìm thấy Token của Fanpage đích. Vui lòng kết nối lại Fanpage.";
                                        Console.WriteLine($"=> [Hệ thống] LỖI bài {bai.Id}: Chưa có Token cho PageId {bai.PageId}");
                                        continue; // Bỏ qua bài này, chạy tiếp bài khác
                                    }

                                    string pageAccessToken = tokenConfig.AccessToken;
                                    string targetPageId = tokenConfig.PageId; // Lấy đích danh ID Page từ Database

                                    List<string> tempFilesToDelete = new List<string>();

                                    try
                                    {
                                        bool postSuccess = false;
                                        string[] rawPaths = string.IsNullOrWhiteSpace(bai.LinkHinhAnh)
                                            ? new string[0]
                                            : bai.LinkHinhAnh.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                                        List<string> processedPaths = new List<string>();

                                        foreach (var p in rawPaths)
                                        {
                                            string path = p.Trim();
                                            if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                                            {
                                                Console.WriteLine($"=> Đang tải ảnh từ web: {path}");
                                                byte[] imageBytes = await client.GetByteArrayAsync(path);

                                                string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".jpg");
                                                await File.WriteAllBytesAsync(tempFile, imageBytes, stoppingToken);

                                                processedPaths.Add(tempFile);
                                                tempFilesToDelete.Add(tempFile);
                                            }
                                            else
                                            {
                                                processedPaths.Add(path);
                                            }
                                        }

                                        string[] filePaths = processedPaths.ToArray();

                                        if (filePaths.Length == 1)
                                        {
                                            string localPath = filePaths[0].Trim();
                                            if (File.Exists(localPath))
                                            {
                                                // Dùng targetPageId động thay vì _pageId fix cứng
                                                string url = $"https://graph.facebook.com/v19.0/{targetPageId}/photos";
                                                using (var form = new MultipartFormDataContent())
                                                {
                                                    form.Add(new StringContent(bai.NoiDung ?? ""), "message");
                                                    form.Add(new StringContent(pageAccessToken), "access_token");
                                                    form.Add(new ByteArrayContent(File.ReadAllBytes(localPath)), "source", Path.GetFileName(localPath));

                                                    HttpResponseMessage res = await client.PostAsync(url, form);
                                                    if (res.IsSuccessStatusCode) postSuccess = true;
                                                    else bai.GhiChuLoi = await res.Content.ReadAsStringAsync();
                                                }
                                            }
                                            else bai.GhiChuLoi = $"Không tìm thấy file ảnh tại: {localPath}";
                                        }
                                        else if (filePaths.Length > 1)
                                        {
                                            List<string> mediaIds = new List<string>();
                                            foreach (var p in filePaths)
                                            {
                                                string localPath = p.Trim();
                                                if (!File.Exists(localPath)) continue;

                                                string uploadUrl = $"https://graph.facebook.com/v19.0/{targetPageId}/photos";
                                                using (var form = new MultipartFormDataContent())
                                                {
                                                    form.Add(new StringContent("false"), "published");
                                                    form.Add(new StringContent(pageAccessToken), "access_token");
                                                    form.Add(new ByteArrayContent(File.ReadAllBytes(localPath)), "source", Path.GetFileName(localPath));

                                                    HttpResponseMessage uploadRes = await client.PostAsync(uploadUrl, form);
                                                    if (uploadRes.IsSuccessStatusCode)
                                                    {
                                                        using (JsonDocument doc = JsonDocument.Parse(await uploadRes.Content.ReadAsStringAsync()))
                                                        {
                                                            mediaIds.Add(doc.RootElement.GetProperty("id").GetString());
                                                        }
                                                    }
                                                }
                                            }

                                            if (mediaIds.Count > 0)
                                            {
                                                string feedUrl = $"https://graph.facebook.com/v19.0/{targetPageId}/feed";
                                                var postData = new List<KeyValuePair<string, string>>
                                                {
                                                    new KeyValuePair<string, string>("message", bai.NoiDung ?? ""),
                                                    new KeyValuePair<string, string>("access_token", pageAccessToken)
                                                };
                                                for (int i = 0; i < mediaIds.Count; i++)
                                                {
                                                    postData.Add(new KeyValuePair<string, string>($"attached_media[{i}]", $"{{\"media_fbid\":\"{mediaIds[i]}\"}}"));
                                                }

                                                var feedContent = new FormUrlEncodedContent(postData);
                                                HttpResponseMessage feedRes = await client.PostAsync(feedUrl, feedContent);

                                                if (feedRes.IsSuccessStatusCode) postSuccess = true;
                                                else bai.GhiChuLoi = await feedRes.Content.ReadAsStringAsync();
                                            }
                                            else bai.GhiChuLoi = "Không bốc được ảnh nào hợp lệ để tạo Album.";
                                        }
                                        else
                                        {
                                            bai.GhiChuLoi = "Thiếu đường dẫn ảnh.";
                                        }

                                        if (postSuccess)
                                        {
                                            bai.TrangThai = "DA_DANG";
                                            bai.GhiChuLoi = "Đăng thành công";
                                            Console.WriteLine($"=> [Tiến trình ngầm] Đã đăng thành công bài ID: {bai.Id} lên Page: {targetPageId}");
                                        }
                                        else
                                        {
                                            bai.TrangThai = "LOI";
                                            Console.WriteLine($"=> [Tiến trình ngầm] Báo lỗi bài ID {bai.Id} trên Page {targetPageId}: {bai.GhiChuLoi}");
                                        }
                                    }
                                    catch (Exception itemEx)
                                    {
                                        bai.TrangThai = "LOI";
                                        bai.GhiChuLoi = $"Lỗi code: {itemEx.Message}";
                                    }
                                    finally
                                    {
                                        foreach (var tempFile in tempFilesToDelete)
                                        {
                                            if (File.Exists(tempFile))
                                            {
                                                try { File.Delete(tempFile); }
                                                catch { }
                                            }
                                        }
                                    }
                                }

                                await dbContext.SaveChangesAsync(stoppingToken);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("=> [LỖI NGHIÊM TRỌNG]: " + ex.Message);
                }

                await Task.Delay(30000, stoppingToken);
            }
        }
    }
}