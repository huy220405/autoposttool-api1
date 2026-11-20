using System.ComponentModel.DataAnnotations;

namespace HhuyShopAutoPost.Api.Models
{
    public class TuKhoaTheoDoi
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string TuKhoa { get; set; } // Ví dụ: "HhuyShop", "Shop Hhuy", "Phốt HhuyShop"

        public bool IsActive { get; set; } = true; // Công tắc bật/tắt theo dõi từ khóa này

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Mối quan hệ 1-N: 1 từ khóa có thể quét ra rất nhiều bài viết
        public ICollection<DuLieuLangNghe> DuLieuLangNghes { get; set; }
    }
}