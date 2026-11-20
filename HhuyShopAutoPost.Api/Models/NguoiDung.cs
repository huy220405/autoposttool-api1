using System.ComponentModel.DataAnnotations;

namespace HhuyShopAutoPost.Api.Models
{
    public class NguoiDung
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string TenDangNhap { get; set; }

        [Required]
        public string MatKhauHash { get; set; } // Lưu mật khẩu đã mã hóa, tuyệt đối không lưu mật khẩu gốc

        [MaxLength(100)]
        public string HoTen { get; set; }
    }
}