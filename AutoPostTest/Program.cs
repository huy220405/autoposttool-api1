using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using ClosedXML.Excel;
using System.Text.Json;

namespace AutoPostTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            string pageId = "1340615389142362";
            // Đã cập nhật Token mới của bạn vào đây:
            string pageAccessToken = "EABBV2Di8MHkBSq7j8AV916Sx5tKCS4ODWozrjbBqRAj7qKm3QJaSjeiIl2F1DpiZA9ECRocZAdaCYhRDq9iLjjKpGnfxrkWeRKfPkQeYOKxOQM6lE1NYyU8ZCOuqxoSl70hxPiX0EbJ8IRf1W49rj4xl2jHGBOqQdZCBIfjF4NWrdj2HfV61EwHwJbiateCSvi25TRADFWjXnKLq098nhIkKi7v5lHdSIdWhdw7PBXAZD";
            string filePath = @"C:\Users\emper\OneDrive\KichBan.xlsx";

            Console.WriteLine("=== HỆ THỐNG AUTO POST HhuyShop ĐÃ KHỞI ĐỘNG ===");
            Console.WriteLine("Tool đang chạy ngầm và tự động kiểm tra lịch đăng mỗi 30 giây...");
            Console.WriteLine("LƯU Ý: Vui lòng KHÔNG MỞ file Excel KichBan.xlsx trong lúc tool đang chạy.\n");

            using (HttpClient client = new HttpClient())
            {
                while (true)
                {
                    try
                    {
                        bool hasUpdates = false;

                        using (var workbook = new XLWorkbook(filePath))
                        {
                            var worksheet = workbook.Worksheet(1);
                            var rows = worksheet.RangeUsed().RowsUsed();

                            foreach (var row in rows)
                            {
                                if (row.RowNumber() == 1) continue;

                                string trangThai = row.Cell(1).Value.ToString();
                                string thoiGianStr = row.Cell(2).Value.ToString();
                                string noiDung = row.Cell(4).Value.ToString();
                                string linkAnhGoc = row.Cell(5).Value.ToString();

                                if (trangThai == "CHO_DANG" && DateTime.TryParse(thoiGianStr, out DateTime thoiGianHen))
                                {
                                    if (DateTime.Now >= thoiGianHen)
                                    {
                                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Đến giờ! Đang xử lý bài: {noiDung}");
                                        string[] filePaths = linkAnhGoc.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                        bool postSuccess = false;

                                        if (filePaths.Length == 1)
                                        {
                                            string localPath = filePaths[0].Trim();
                                            if (File.Exists(localPath))
                                            {
                                                string url = $"https://graph.facebook.com/v19.0/{pageId}/photos";
                                                using (var form = new MultipartFormDataContent())
                                                {
                                                    form.Add(new StringContent(noiDung), "message");
                                                    form.Add(new StringContent(pageAccessToken), "access_token");
                                                    form.Add(new ByteArrayContent(File.ReadAllBytes(localPath)), "source", Path.GetFileName(localPath));

                                                    HttpResponseMessage res = await client.PostAsync(url, form);
                                                    if (res.IsSuccessStatusCode)
                                                    {
                                                        Console.WriteLine("-> [THÀNH CÔNG] Đã đăng 1 ảnh!");
                                                        postSuccess = true;
                                                    }
                                                    else Console.WriteLine("-> [THẤT BẠI] " + await res.Content.ReadAsStringAsync());
                                                }
                                            }
                                            else Console.WriteLine($"-> [LỖI] Không tìm thấy file: {localPath}");
                                        }
                                        else
                                        {
                                            List<string> mediaIds = new List<string>();
                                            foreach (var p in filePaths)
                                            {
                                                string localPath = p.Trim();
                                                if (!File.Exists(localPath)) continue;

                                                string uploadUrl = $"https://graph.facebook.com/v19.0/{pageId}/photos";
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
                                                string feedUrl = $"https://graph.facebook.com/v19.0/{pageId}/feed";
                                                var postData = new List<KeyValuePair<string, string>>
                                                {
                                                    new KeyValuePair<string, string>("message", noiDung),
                                                    new KeyValuePair<string, string>("access_token", pageAccessToken)
                                                };
                                                for (int i = 0; i < mediaIds.Count; i++)
                                                {
                                                    postData.Add(new KeyValuePair<string, string>($"attached_media[{i}]", $"{{\"media_fbid\":\"{mediaIds[i]}\"}}"));
                                                }

                                                var feedContent = new FormUrlEncodedContent(postData);
                                                HttpResponseMessage feedRes = await client.PostAsync(feedUrl, feedContent);

                                                if (feedRes.IsSuccessStatusCode)
                                                {
                                                    Console.WriteLine($"-> [THÀNH CÔNG] Đã đăng Album {mediaIds.Count} ảnh!");
                                                    postSuccess = true;
                                                }
                                                else Console.WriteLine("-> [THẤT BẠI GỘP BÀI] " + await feedRes.Content.ReadAsStringAsync());
                                            }
                                        }

                                        if (postSuccess)
                                        {
                                            row.Cell(1).Value = "DA_DANG";
                                            hasUpdates = true;
                                            Console.WriteLine("-> Đã cập nhật trạng thái thành DA_DANG trong Excel.\n");
                                        }
                                    }
                                }
                            }

                            if (hasUpdates)
                            {
                                workbook.Save();
                            }
                        }
                    }
                    catch (IOException)
                    {
                        // Bỏ qua lỗi khóa file, thử lại sau 30s
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[LỖI HỆ THỐNG]: {ex.Message}");
                    }

                    await Task.Delay(30000);
                }
            }
        }
    }
}