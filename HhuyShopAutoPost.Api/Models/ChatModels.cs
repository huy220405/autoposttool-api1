using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HhuyShopAutoPost.Api.Models
{
    // 1. Hồ sơ Khách hàng đa kênh (Nền tảng cho Mini-CRM sau này)
    public class KhachHangDaKenh
    {
        [Key]
        public int Id { get; set; }
        public string TenKhachHang { get; set; }
        public string AnhDaiDien { get; set; }
        public string SoDienThoai { get; set; }

        // Phân loại khách đến từ đâu: "Zalo", "Instagram", "Facebook", "YouTube"
        public string NenTang { get; set; }

        // ID định danh của khách trên nền tảng đó (UID Zalo, UID Facebook...)
        public string PlatformUserId { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public ICollection<CuocHoiThoai> CuocHoiThoais { get; set; }
    }

    // 2. Quản lý Phiên chat (Để hiển thị danh sách bên trái Chatbox)
    public class CuocHoiThoai
    {
        [Key]
        public int Id { get; set; }
        public int KhachHangId { get; set; }

        // ID của luồng tin nhắn do API trả về (nếu có)
        public string ThreadId { get; set; }

        public DateTime ThoiGianCapNhatCuoi { get; set; }
        public bool IsRead { get; set; } // Trạng thái đã đọc/chưa đọc

        public KhachHangDaKenh KhachHang { get; set; }
        public ICollection<TinNhanDaKenh> TinNhanDaKenhs { get; set; } // Đã sửa liên kết
    }

    // 3. Nội dung Tin nhắn chi tiết (Đã đổi tên thành TinNhanDaKenh)
    public class TinNhanDaKenh
    {
        [Key]
        public int Id { get; set; }
        public int HoiThoaiId { get; set; }

        // Cờ xác định: True = Shop gửi đi, False = Khách gửi đến
        public bool IsShopSend { get; set; }

        public string NoiDung { get; set; }

        // Phân loại: "text", "image", "video", "sticker"
        public string LoaiTinNhan { get; set; }

        public DateTime ThoiGianGui { get; set; } = DateTime.Now;

        public CuocHoiThoai CuocHoiThoai { get; set; }
    }
}