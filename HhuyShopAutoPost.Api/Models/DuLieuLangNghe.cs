using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HhuyShopAutoPost.Api.Models
{
    public class DuLieuLangNghe
    {
        [Key]
        public int Id { get; set; }

        // Khóa ngoại liên kết với bảng TuKhoaTheoDoi
        public int TuKhoaId { get; set; }
        [ForeignKey("TuKhoaId")]
        public TuKhoaTheoDoi TuKhoaTheoDoi { get; set; }

        public string NoiDung { get; set; } // Nội dung bài viết hoặc comment chứa từ khóa

        [MaxLength(50)]
        public string Nguon { get; set; } // Nơi quét được: Facebook, TikTok, Website...

        public string LinkBaiViet { get; set; } // Link gốc để click vào xử lý khủng hoảng

        [MaxLength(20)]
        public string CamXuc { get; set; } // Phân loại: TieuCuc, TichCuc, TrungLap

        public DateTime NgayThuThap { get; set; } = DateTime.Now;
    }
}