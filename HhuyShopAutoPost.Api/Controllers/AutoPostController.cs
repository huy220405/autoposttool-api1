using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using HhuyShopAutoPost.Api.Data;
using HhuyShopAutoPost.Api.Models;

namespace HhuyShopAutoPost.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutoPostController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AutoPostController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("upload-excel")]
        // NÂNG CẤP: Hứng thêm pageId từ Frontend
        public async Task<IActionResult> UploadExcel(IFormFile file, [FromForm] string pageId)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Vui lòng chọn một file Excel hợp lệ!" });
            }

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Hệ thống chỉ hỗ trợ định dạng file .xlsx!" });
            }

            // Gán giá trị mặc định nếu Frontend quên gửi để tránh lỗi Database
            string finalPageId = string.IsNullOrWhiteSpace(pageId) ? "Fanpage_Mac_Dinh" : pageId;

            var danhSachBai = new List<KichBanDangBai>();

            try
            {
                using (var stream = file.OpenReadStream())
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RangeUsed().RowsUsed();

                    foreach (var row in rows)
                    {
                        // Bỏ qua dòng tiêu đề
                        if (row.RowNumber() == 1) continue;

                        string thoiGianStr = row.Cell(2).Value.ToString().Trim();
                        string phanLoai = row.Cell(3).Value.ToString().Trim();
                        string noiDung = row.Cell(4).Value.ToString().Trim();
                        string linkAnh = row.Cell(5).Value.ToString().Trim();

                        if (string.IsNullOrWhiteSpace(noiDung)) continue;

                        if (!DateTime.TryParse(thoiGianStr, out DateTime thoiGianDang))
                        {
                            thoiGianDang = DateTime.Now;
                        }

                        var baiMoi = new KichBanDangBai
                        {
                            // NÂNG CẤP: Lưu đúng PageId mà người dùng đã chọn
                            PageId = finalPageId,
                            TrangThai = "CHO_DANG",
                            ThoiGianDang = thoiGianDang,
                            PhanLoai = phanLoai,
                            NoiDung = noiDung,
                            LinkHinhAnh = linkAnh,
                            NgayTao = DateTime.Now
                        };

                        danhSachBai.Add(baiMoi);
                    }
                }

                if (danhSachBai.Count > 0)
                {
                    await _context.KichBanDangBai.AddRangeAsync(danhSachBai);
                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        thanhCong = true,
                        soLuong = danhSachBai.Count,
                        message = $"Đã lưu thành công {danhSachBai.Count} bài viết vào SQL Server!"
                    });
                }

                return BadRequest(new { message = "Không tìm thấy nội dung bài viết nào trong file Excel." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi xử lý file: {ex.Message}" });
            }
        }
    }
}