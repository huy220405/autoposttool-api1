using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HhuyShopAutoPost.Api.Models
{
    public class CauHinh
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int NguoiDungId { get; set; } // Liên kết Token này với Tài khoản nào

        [Required]
        [MaxLength(100)]
        public string PageId { get; set; } // ID định danh của Fanpage trên Facebook

        [Required]
        [MaxLength(255)]
        public string TenFanpage { get; set; } // Tên hiển thị trên giao diện

        [Required]
        public string AccessToken { get; set; } // Chìa khóa dùng để đăng bài

        public string AvatarUrl { get; set; } // Đường dẫn ảnh đại diện của Fanpage

        public DateTime NgayKetNoi { get; set; } = DateTime.Now;

        // Thiết lập mối quan hệ với bảng NguoiDung
        [ForeignKey("NguoiDungId")]
        public virtual NguoiDung NguoiDung { get; set; }
    }
}