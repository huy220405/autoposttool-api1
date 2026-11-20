using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HhuyShopAutoPost.Api.Models
{
    [Table("TinNhan")]
    public class TinNhan
    {
        [Key]
        public int Id { get; set; }

        public string PageId { get; set; } // ID của Fanpage (recipient.id)

        public string SenderId { get; set; } // ID của khách hàng nhắn tin (sender.id)

        public string NoiDung { get; set; } // Nội dung chat (message.text)

        public DateTime ThoiGianGui { get; set; } // Thời gian nhận tin

        // Trạng thái để sau này làm tính năng "Đánh dấu chưa đọc"
        public bool DaDoc { get; set; } = false;
    }
}