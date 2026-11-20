using System;
using System.ComponentModel.DataAnnotations;

namespace HhuyShopAutoPost.Api.Models
{
    public class KichBanDangBai
    {
        [Key]
        public int Id { get; set; }
        public string TrangThai { get; set; } = "CHO_DANG";
        public DateTime ThoiGianDang { get; set; }

        // Thêm dấu ? để cho phép dữ liệu trống (NULL)
        public string? PhanLoai { get; set; }
        public string? NoiDung { get; set; }
        public string? LinkHinhAnh { get; set; }
        public string? GhiChuLoi { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;
        public string PageId { get; set; }
    }
}