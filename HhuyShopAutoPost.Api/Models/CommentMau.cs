using System;
using System.ComponentModel.DataAnnotations;

namespace HhuyShopAutoPost.Api.Models
{
    public class CommentMau
    {
        [Key]
        public int Id { get; set; }

        // Các từ khóa khách hay hỏi (ví dụ: "giá", "size", "bao nhiêu")
        public string? TuKhoa { get; set; }

        // Câu trả lời tương ứng soạn sẵn của sếp
        public string? NoiDungTraLoi { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
}